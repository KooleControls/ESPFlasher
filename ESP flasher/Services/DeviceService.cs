using EspDotNet;
using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools;
using EspDotNet.Tools.Firmware;
using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace ESP_Flasher.Services
{
    public class DeviceService : IDisposable
    {
        public string SerialPort { get; set; } = "COM1"; // Default port, can be overridden
        public int BaudRate { get; set; } = 115200;
        public bool UseCompression { get; set; } = false;

        private readonly ESPToolbox _toolbox;
        private readonly ILogger<DeviceService> _logger;
        private SerialPort? serialPort;
        private Communicator? communicator;
        private ILoader? bootLoader;
        private SoftLoader? softLoader;
        private DeviceConfig? deviceConfig;
        public bool IsReady { get; private set; } = false;

        public DeviceService(ILogger<DeviceService> logger)
        {
            _toolbox = new ESPToolbox();
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            if (IsReady)
                return;

            serialPort = new SerialPort();
            serialPort.PortName = SerialPort;
            serialPort.BaudRate = 115200;
            serialPort.Open();
            _logger.LogInformation("Serial port {Port} @ {BaudRate} opened", serialPort.PortName, serialPort.BaudRate);

            communicator = _toolbox.CreateCommunicator(serialPort);
            bootLoader = await _toolbox.CreateBootloaderTool(communicator).StartBootloaderAsync(token);
            _logger.LogInformation("Bootloader started");

            if (BaudRate != serialPort.BaudRate)
            {
                await _toolbox.CreateChangeBaudRateTool(bootLoader).ChangeBaudAsync(BaudRate, serialPort.BaudRate, token);
                _logger.LogInformation("BaudRate changed from {OldBaud} to {NewBaud}", serialPort.BaudRate, BaudRate);
                serialPort.BaudRate = BaudRate;
            }

            deviceConfig = await _toolbox.CreateChipTypeDetectTool(bootLoader).DetectAndGetDeviceConfig(token);
            _logger.LogInformation("Detected chip type: {Chip}", deviceConfig.ChipType);

            var softLoaderFirmware = DefaultFirmwareProviders.GetSoftloaderForDevice(deviceConfig.ChipType);
            var ramUploadTool = _toolbox.CreateRamUploadTool(bootLoader, deviceConfig);
            softLoader = await _toolbox.CreateSoftLoaderTool(communicator, ramUploadTool).StartAsync(softLoaderFirmware, token);
            _logger.LogInformation("SoftLoader started");

            IsReady = true;
        }


        public Stream GetReadFlashStream(uint address, uint size)
        {
            if(!IsReady) throw new InvalidOperationException($"DeviceService is not initialized. Call {nameof(InitializeAsync)} first.");
            if (softLoader == null) throw new InvalidOperationException("SoftLoader is not initialized.");
            if (communicator == null) throw new InvalidOperationException("Communicator is not initialized.");

            var readTool = _toolbox.CreateReadFlashTool(communicator, softLoader);
            return readTool.OpenFlashReadStream(address, size);

        }





        // ----------------------------
        // Reset State
        // ----------------------------
        public void Dispose()
        {
            _logger.LogInformation("Disposing DeviceService state...");

            communicator = null;
            bootLoader = null;
            softLoader = null;
            deviceConfig = null;

            serialPort?.Close();
            serialPort?.Dispose();
            serialPort = null;

            IsReady = false;
        }
    }
}
