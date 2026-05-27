using ESP_Flasher.Models;
using EspDotNet;
using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools;
using EspDotNet.Tools.Firmware;
using Microsoft.Extensions.Logging;

namespace ESP_Flasher.Services
{
    public class DeviceService
    {
        public int BaudRate { get; set; } = 921600;
        public string SerialPort { get; set; } = "COM30";
        public bool UseCompression { get; set; } = false;

        private const int BootloaderBaudRate = 115200; // ROM bootloader default

        private readonly ESPToolConfig _config;
        private readonly ILogger<DeviceService> _logger;

        // Internals needed for device session
        private System.IO.Ports.SerialPort? _port;
        private Communicator? _communicator;
        private ILoader? _bootloader;
        private SoftLoader? _softloader;
        private DeviceConfig? _deviceConfig;

        public DeviceService(ILoggerFactory loggerFactory)
        {
            _config = ConfigProvider.LoadDefaultConfig();
            _logger = loggerFactory.CreateLogger<DeviceService>();
        }

        private async Task InitializeDevice(CancellationToken token = default)
        {
            // The serial port is owned by us, not the library, so we open and dispose it ourselves.
            _port = new System.IO.Ports.SerialPort(SerialPort, BootloaderBaudRate);
            _port.Open();
            _communicator = new Communicator(_port);
            _logger.LogInformation("Opened port {SerialPort}", SerialPort);

            _bootloader = await new BootloaderTool(_communicator, _config.BootloaderSequence).StartBootloaderAsync(token);
            _logger.LogInformation("Bootloader started");

            _deviceConfig = await new ChipTypeDetectTool(_bootloader, _config).DetectAndGetDeviceConfigAsync(token);
            _logger.LogInformation("Detected chip type: {ChipType}", _deviceConfig.ChipType);

            var stub = DefaultFirmwareProviders.GetSoftloaderForDevice(_deviceConfig.ChipType);
            var ramUploadTool = new RamUploadTool(_bootloader, _deviceConfig);
            _softloader = await new SoftLoaderTool(_communicator, ramUploadTool).StartAsync(stub, token);
            _logger.LogInformation("Softloader started");

            await new ChangeBaudRateTool(_softloader).ChangeBaudAsync(BaudRate, BootloaderBaudRate, token);
            _logger.LogInformation("Baudrate changed to {BaudRate}", BaudRate);
        }

        public async Task FlashAsync(FirmwareArchive archive, CancellationToken token = default, IProgress<float>? progress = null)
        {
            try
            {
                await InitializeDevice(token);

                IUploadTool uploadTool = UseCompression
                    ? new FlashUploadDeflatedTool(_softloader!, _deviceConfig!)
                    : new FlashUploadTool(_softloader!, _deviceConfig!);

                var firmwareUploadTool = new FirmwareUploadTool(uploadTool);
                if (progress != null)
                    firmwareUploadTool.Progress = progress;
                await firmwareUploadTool.UploadFirmwareAsync(archive, token);
                _logger.LogInformation("Firmware uploaded");

                await new ResetDeviceTool(_communicator!, _config.ResetSequence).ResetAsync(token);
                _logger.LogInformation("Device reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flash device");
                throw;
            }
            finally
            {
                DisposeDevice();
            }
        }

        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            try
            {
                await InitializeDevice(token);

                await new FlashEraseTool(_softloader!).EraseFlashAsync(token);
                _logger.LogInformation("Flash erased");

                await new ResetDeviceTool(_communicator!, _config.ResetSequence).ResetAsync(token);
                _logger.LogInformation("Device reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to erase flash");
                throw;
            }
            finally
            {
                DisposeDevice();
            }
        }

        public void DisposeDevice()
        {
            if (_port != null)
            {
                _port.Dispose();
                _logger.LogInformation("Closed port {SerialPort}", SerialPort);
            }

            _port = null;
            _communicator = null;
            _bootloader = null;
            _softloader = null;
            _deviceConfig = null;
        }
    }
}
