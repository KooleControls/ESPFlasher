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

            // ChangeBaudRateTool only tells the device to switch; since we own the SerialPort,
            // we must reconfigure the host side too. The brief delay lets the device finish its
            // switch, and DiscardInBuffer clears any garbage that arrived at the old baud during
            // the transition.
            await Task.Delay(50, token);
            _port.BaudRate = BaudRate;
            _port.DiscardInBuffer();

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flash device");
                throw;
            }
            finally
            {
                await SafeResetAsync();
                DisposeDevice();
            }
        }

        public async Task<byte[]> ReadFlashAsync(uint offset, uint size, CancellationToken token = default, IProgress<float>? progress = null)
        {
            try
            {
                await InitializeDevice(token);

                var downloadTool = new FlashDownloadTool(_softloader!, _communicator!);
                downloadTool.OnTrace = msg => _logger.LogInformation("[flash-rd] {Msg}", msg);
                if (progress != null)
                    downloadTool.Progress = progress;

                using var output = new MemoryStream((int)size);
                await downloadTool.ReadFlashAsync(offset, size, output, token);
                _logger.LogInformation("Read {Size} bytes from 0x{Offset:X}", size, offset);

                return output.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read flash");
                throw;
            }
            finally
            {
                await SafeResetAsync();
                DisposeDevice();
            }
        }

        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            try
            {
                await InitializeDevice(token);

                // Whole-chip erase only ACKs at the end and can take ~minute on slow flash chips.
                using var eraseCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                eraseCts.CancelAfter(TimeSpan.FromSeconds(120));
                await new FlashEraseTool(_softloader!).EraseFlashAsync(eraseCts.Token);
                _logger.LogInformation("Flash erased");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to erase flash");
                throw;
            }
            finally
            {
                await SafeResetAsync();
                DisposeDevice();
            }
        }

        // Reset is in `finally` so the chip always boots back to user firmware after an operation
        // (or operation failure). Without this, leaving the chip running the softloader stub looks
        // exactly like "firmware gone" until the device is power-cycled. Swallow errors so a reset
        // failure doesn't mask the original exception.
        private async Task SafeResetAsync()
        {
            if (_communicator == null)
                return;
            try
            {
                await new ResetDeviceTool(_communicator, _config.ResetSequence).ResetAsync(CancellationToken.None);
                _logger.LogInformation("Device reset");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not reset device cleanly: {Message}. Power-cycle may be needed.", ex.Message);
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
