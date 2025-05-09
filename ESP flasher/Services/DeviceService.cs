using ESP_Flasher.Models;
using EspDotNet;
using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Loaders;
using EspDotNet.Tools.Firmware;
using Microsoft.Extensions.Logging;

namespace ESP_Flasher.Services
{
    public class DeviceService
    {
        public int BaudRate { get; set; } = 921600;
        public string SerialPort { get; set; } = "COM30";
        public bool UseCompression { get; set; } = false;

        private readonly ESPToolbox _toolbox;
        private readonly ILogger<DeviceService> _logger;

        // Internals needed for device session
        private Communicator? _communicator;
        private ILoader? _bootloader;
        private SoftLoader? _softloader;
        private ChipTypes _chipType;

        public DeviceService(ILoggerFactory loggerFactory)
        {
            _toolbox = new ESPToolbox();
            _logger = loggerFactory.CreateLogger<DeviceService>();
        }

        private async Task InitializeDevice(CancellationToken token = default)
        {

            _communicator = _toolbox.CreateCommunicator();
            _toolbox.OpenSerial(_communicator, SerialPort, 115200); // start at default baud
            _logger.LogInformation("Opened port {SerialPort}", SerialPort);

            _bootloader = await _toolbox.StartBootloaderAsync(_communicator, token);
            _logger.LogInformation("Bootloader started");

            _chipType = await _toolbox.DetectChipTypeAsync(_bootloader, token);
            _logger.LogInformation("Detected chip type: {ChipType}", _chipType);

            _softloader = await _toolbox.StartSoftloaderAsync(_communicator, _bootloader, _chipType, token);
            _logger.LogInformation("Softloader started");

            await _toolbox.ChangeBaudAsync(_communicator, _softloader, BaudRate, token);
            _logger.LogInformation("Baudrate changed to {BaudRate}", BaudRate);

        }

        public async Task FlashAsync(FirmwareArchive archive, CancellationToken token = default, IProgress<float>? progress = null)
        {
            try
            {
                await InitializeDevice(token);

                var uploadTool = UseCompression
                    ? _toolbox.CreateUploadFlashDeflatedTool(_softloader!, _chipType)
                    : _toolbox.CreateUploadFlashTool(_softloader!, _chipType);

                await _toolbox.UploadFirmwareAsync(uploadTool, archive, token, progress);
                _logger.LogInformation("Firmware uploaded");

                await _toolbox.ResetDeviceAsync(_communicator!, token);
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
                await _toolbox.EraseFlashAsync(_softloader!, token);
                _logger.LogInformation("Flash erased");

                await _toolbox.ResetDeviceAsync(_communicator!, token);
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
            if (_communicator != null)
            {
                _communicator.Dispose();
                _logger.LogInformation("Closed port {SerialPort}", SerialPort);
            }

            _communicator = null;
            _bootloader = null;
            _softloader = null;
            _chipType = ChipTypes.Unknown;
        }
    }
}
