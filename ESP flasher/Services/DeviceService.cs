using ESP_Flasher.Models;
using ESPTool.Devices;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            // Get the device, default baud rate is 115200
            _logger.LogInformation("Initializing device on {SerialPort}", SerialPort);
            IDevice device = await _deviceManager.InitializeAsync(SerialPort, 115200, token);

            switch (device)
            {
                case ESP32Device espDevice:
                    _device = espDevice;
                    break;
                default:
                    _logger.LogError("Unsupported device type: {DeviceType}", device.GetType());
                    throw new Exception($"Device not supported {device.GetType()}");
            }

            // Change baud if required
            if (BaudRate != 115200)
            {
                _logger.LogInformation("Switching baud rate to {BaudRate}", BaudRate);
                await _device.ChangeBaudAsync(BaudRate, token);
            }

            // Start the softloader if required
            if (UseSoftLoader)
            {
                _logger.LogInformation("Starting softloader");
                await _device.StartSoftloaderAsync(token);
            }
        }

        // Flash the firmware to the device
        public async Task FlashAsync(FirmwareArchive archive, CancellationToken token = default, IProgress<float> progress = null)
        {
            await InitializeDevice(token);
            if (_device == null)
            {
                _logger.LogError("Device initialization failed");
                throw new Exception("Device initialization failed");
            }

            // Calculate total size
            long totalSize = archive.Entries.Sum(entry => entry.Contents.Length);
            long bytesUploaded = 0;

            foreach (var entry in archive.Entries)
            {
                using MemoryStream stream = new MemoryStream(entry.Contents);
                UInt32 size = (UInt32)entry.Contents.Length;

                _logger.LogInformation($"Uploading {entry.File}, {size} bytes");

                var entryProgress = new Progress<float>(entryProgressValue =>
                {
                    // Adjust entry progress to reflect overall progress
                    float overallProgress = ((float)bytesUploaded + entryProgressValue * size) / totalSize;
                    progress?.Report(overallProgress);
                });

                try
                {
                    if (UseCompression)
                    {
                        await _device.UploadCompressedToFlashAsync(stream, size, (UInt32)entry.Address, false, 0, token, entryProgress);
                    }
                    else
                    {
                        await _device.UploadToFlashAsync(stream, size, (UInt32)entry.Address, false, 0, token, entryProgress);
                    }

                    // Update the total bytes uploaded after each entry is successfully uploaded
                    bytesUploaded += size;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload entry {File} to address {Address}", entry.File, entry.Address);
                    throw;
                }
            }

            await _device.ResetDeviceAsync(token); // Reset the device after flashing
        }

        // Erase the firmware from the device
        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            await InitializeDevice(token);
            if (_device == null)
            {
                _logger.LogError("Device initialization failed");
                throw new Exception("Device initialization failed");
            }

            _logger.LogInformation("Erasing flash...");
            await _device.EraseFlashAsync(token);
        }
    }
}
