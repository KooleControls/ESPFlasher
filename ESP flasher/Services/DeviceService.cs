using ESP_Flasher.Models;
using ESPTool.Devices;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.IO;
using System.Security.Policy;

namespace ESP_Flasher.Services
{
    public class DeviceService
    {
        public int BaudRate { get; set; } = 921600;
        public string SerialPort { get; set; } = "COM30";
        public bool UseSoftLoader { get; set; } = true;
        public bool UseCompression { get; set; } = false;

        ESP32Device? _device = null;

        private readonly ArchiveService _archiveService;
        private readonly DeviceManager _deviceManager;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(ArchiveService archiveService, ILoggerFactory loggerFactory)
        {
            _archiveService = archiveService;
            _deviceManager = new DeviceManager(loggerFactory);
            _logger = loggerFactory.CreateLogger<DeviceService>();
        }

        public async Task InitializeDevice(CancellationToken token = default)
        {
            // Get the device, default baudrate is 115200
            IDevice device = await _deviceManager.InitializeAsync(SerialPort, 115200, token);

            switch (device)
            {
                case ESP32Device espDevice:
                    _device = espDevice;
                    break;
                default:
                    throw new Exception($"Device not supported {device.GetType()}");
            }

            // Change baud if required
            if (BaudRate != 115200)
            {
                _logger.LogError($"Switch baudrate {BaudRate}");
                await _device.ChangeBaudAsync(BaudRate, token);
            }


            // Start the softloader, if required
            if (UseSoftLoader)
            {
                _logger.LogError($"Starting softloader");
                await _device.StartSoftloaderAsync(token);
            }
        }

        // Flash the firmware to the device
        public async Task FlashAsync(FirmwareArchive archive, CancellationToken token = default, IProgress<float> progress = null)
        {
            await InitializeDevice(token);
            if (_device == null)
                throw new Exception("Device initialisation failed");

            // Calculate the total size of all entries
            long totalSize = archive.Entries.Sum(entry => entry.Size);
            long bytesUploaded = 0; // Track the number of bytes uploaded so far

            foreach (var entry in archive.Entries)
            {
                await _archiveService.UseFileStream(archive, entry.File, async (stream, size) =>
                {
                    _logger.LogError($"Uploading {entry.File}, {size} bytes");

                    var entryProgress = new Progress<float>(entryProgressValue =>
                    {
                        // Adjust entry progress to reflect overall progress
                        float overallProgress = ((float)bytesUploaded + entryProgressValue * size) / totalSize;
                        progress?.Report(overallProgress);
                    });

                    if (UseCompression)
                        await _device.UploadCompressedToFlashAsync(stream, (UInt32)size, (UInt32)entry.Address, token, entryProgress);
                    else
                        await _device.UploadToFlashAsync(stream, (UInt32)size, (UInt32)entry.Address, token, entryProgress);
                });

                // After each entry is uploaded, update the total bytes uploaded
                bytesUploaded += entry.Size;

                if (UseCompression)
                    await _device.UploadCompressedToFlashFinishAsync(false, 0, token);
                else
                    await _device.UploadToFlashFinishAsync(false, 0, token);
            }

            await _device.ResetDeviceAsync(token); // Reset the device after flashing
        }

        // Erase the firmware from the device
        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            await InitializeDevice(token);
            if (_device == null)
                throw new Exception("Device initialisation failed");
            await _device.EraseFlashAsync(token);
        }
    }
}

