using ESP_Flasher.Models;
using ESPTool;
using ESPTool.Loaders;
using ESPTool.Loaders.SoftLoader;
using ESPTool.Tools;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ESP_Flasher.Services
{
    public class DeviceService
    {
        public int BaudRate { get; set; } = 921600;
        public string SerialPort { get; set; } = "COM30";
        public bool UseCompression { get; set; } = false;

        private readonly ArchiveService _archiveService;
        private readonly DeviceManager _deviceManager;
        private readonly ILogger<DeviceService> _logger;
        private SoftLoader? loader;

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
            _deviceManager.OpenSerial(SerialPort, 115200);

            // Enter the bootloader
            var bootloader = await _deviceManager.StartBootloader();

            // Change baud if required
            if (BaudRate != 115200)
            {
                _logger.LogInformation("Switching baud rate to {BaudRate}", BaudRate);
                await bootloader.ChangeBaudAsync(BaudRate, 115200, token);
            }

            // Enter the softloader
            loader = await _deviceManager.StartSoftloader(bootloader);

        }

        public void DisposeDevice()
        {
            //_device?.Dispose();
        }

        // Flash the firmware to the device
        public async Task FlashAsync(FirmwareArchive archive, CancellationToken token = default, IProgress<float> progress = null)
        {
            await InitializeDevice(token);
            if (loader == null)
            {
                _logger.LogError("Device initialization failed");
                throw new Exception("Device initialization failed");
            }

            // Calculate total size
            long totalSize = archive.BinFiles.Sum(entry => entry.Contents.Length);
            long bytesUploaded = 0;

            foreach (var entry in archive.BinFiles)
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

                FirmwareSender sender = new FirmwareSender(loader);
                sender.Progress = progress; 
                sender.UploadMethod = UseCompression ? FirmwareUploadOptions.FlashDeflated : FirmwareUploadOptions.Flash;

                // Update the total bytes uploaded after each entry is successfully uploaded
                bytesUploaded += size;

                try
                {
                    if (UseCompression)
                    {
                        

                        await loader.UploadCompressedToFlashAsync(stream, size, (UInt32)entry.Address, false, 0, token, entryProgress);
                    }
                    else
                    {
                        await loader.UploadToFlashAsync(stream, size, (UInt32)entry.Address, false, 0, token, entryProgress);
                    }

                    
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
