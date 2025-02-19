using ESP_Flasher.Models;
using EspDotNet;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools.Firmware;
using Microsoft.Extensions.Logging;
using System;

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
        }

        public void DisposeDevice()
        {
            _espTool.CloseSerial();
        }

        public async Task FlashAsync(FirmwareArchive archive, CancellationToken token = default, IProgress<float> progress = null)
        {
            await InitializeDevice(token);

            FirmwareUploadConfig config = new FirmwareUploadConfig
            {
                BlockSize = 2048,
                ExecuteAfterSending = false,
                UploadMethod = FirmwareUploadOptions.FlashDeflated
            };

            await _espTool.UploadFirmwareAsync(archive, token, progress);
            await _espTool.ResetDeviceAsync(token);
        }

        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            await InitializeDevice(token);

            await _espTool.EraseFlashAsync(token);
            _logger.LogInformation("Erasing flash finished");
        }
    }
}
