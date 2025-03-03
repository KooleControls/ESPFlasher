using ESP_Flasher.Models;
using EspDotNet;
using EspDotNet.Config;
using EspDotNet.Tools.Firmware;
using Microsoft.Extensions.Logging;

namespace ESP_Flasher.Services
{
    public class DeviceService
    {
        public int BaudRate { get; set; } = 921600;
        public string SerialPort { get; set; } = "COM30";
        public bool UseCompression { get; set; } = false;

        private readonly ArchiveService _archiveService;
        private readonly ESPTool _espTool;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(ArchiveService archiveService, ILoggerFactory loggerFactory)
        {
            _archiveService = archiveService;
            ESPToolConfig config = new ESPToolConfig
            { 
                BootloaderSequence = new PinSequence
                {
                    Steps =
                    [
                        new PinSequenceStep {  Dtr = false, Rts = true, Delay = TimeSpan.FromMilliseconds(100) },
                        new PinSequenceStep {  Dtr = true,  Rts = false, Delay = TimeSpan.FromMilliseconds(600) },
                        new PinSequenceStep {  Dtr = false, Rts = false, Delay = TimeSpan.FromMilliseconds(0) },
                    ]
                }
            };


            _espTool = new ESPTool();
            _logger = loggerFactory.CreateLogger<DeviceService>();
        }

        public async Task InitializeDevice(CancellationToken token = default)
        {
            _espTool.OpenSerial(SerialPort, 115200); // Default baud rate is 115200
            _logger.LogInformation("Opened port {SerialPort}", SerialPort);

            await _espTool.StartBootloaderAsync(token);
            _logger.LogInformation("Bootloader started");

            var chipType = await _espTool.DetectChipTypeAsync(token);
            _logger.LogInformation("Detected '{ChipType}'", chipType);

            var softloader = DefaultFirmwareProviders.GetSoftloaderForDevice(chipType);
            await _espTool.StartSoftloaderAsync(softloader, token);
            _logger.LogInformation("Softloader started");

            await _espTool.ChangeBaudAsync(BaudRate, token);
            _logger.LogInformation($"Switched baudrade to {BaudRate}");
        }

        public void DisposeDevice()
        {
            _espTool.CloseSerial();
            _logger.LogInformation("Closed port {SerialPort}", SerialPort);
        }

        public async Task FlashAsync(FirmwareArchive archive, CancellationToken token = default, IProgress<float> progress = null)
        {
            await InitializeDevice(token);
            var uploadMethod = UseCompression ? FirmwareUploadMethods.FlashDeflated : FirmwareUploadMethods.Flash;
            await _espTool.UploadFirmwareAsync(archive, uploadMethod, token, progress);
            _logger.LogInformation("Firmware uploaded");
            await _espTool.ResetDeviceAsync(token);
            _logger.LogInformation("Device resetted");
        }

        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            await InitializeDevice(token);

            await _espTool.EraseFlashAsync(token);
            _logger.LogInformation("Erasing flash finished");
        }
    }
}
