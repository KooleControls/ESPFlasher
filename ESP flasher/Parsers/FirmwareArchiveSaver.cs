using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;

namespace ESP_Flasher.Parsers
{
    public class FirmwareArchiveSaver
    {
        // Configurable properties for file names
        public string SettingsFileName { get; set; } = "Settings.json";
        public string PartitionTableFileName { get; set; } = "partition-table.bin";

        private readonly ILogger<FirmwareArchiveSaver> _logger;

        public FirmwareArchiveSaver(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FirmwareArchiveSaver>();
        }

        // Save the archive to a zip file
        public async Task SaveToZip(string outputZipFile, FirmwareArchive firmwareArchive, CancellationToken token = default)
        {
            try
            {
                using FileStream zipToOpen = new FileStream(outputZipFile, FileMode.Create);
                using ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create);

                // Save settings.json
                await SaveSettingsJson(archive, firmwareArchive, token);

                // Save each firmware binary entry
                foreach (var entry in firmwareArchive.BinFiles)
                {
                    await SaveBinFile(archive, entry, token);
                }

                // Save each elf file
                foreach (var entry in firmwareArchive.ElfFiles)
                {
                    await SaveBinFile(archive, entry, token);
                }

                _logger.LogInformation($"Firmware archive successfully saved to {outputZipFile}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving firmware archive to ZIP.");
                throw; // Re-throw the exception to inform the caller.
            }
        }

        // Private method to save the settings.json file
        private async Task SaveSettingsJson(ZipArchive archive, FirmwareArchive firmwareArchive, CancellationToken token)
        {
            try
            {
                ZipArchiveEntry settingsEntry = archive.CreateEntry(SettingsFileName);
                using StreamWriter writer = new StreamWriter(settingsEntry.Open());
                string jsonContent = JsonSerializer.Serialize(firmwareArchive.BinFiles);
                await writer.WriteAsync(jsonContent);
                await writer.FlushAsync();
                _logger.LogInformation("Settings file {SettingsFileName} saved successfully.", SettingsFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving {SettingsFileName}.", SettingsFileName);
                throw;
            }
        }

        // Private method to save individual binary files in the firmware archive
        private async Task SaveBinFile(ZipArchive archive, BinFile binFile, CancellationToken token)
        {
            try
            {
                ZipArchiveEntry binEntry = archive.CreateEntry(binFile.File);
                using Stream binStream = binEntry.Open();
                using MemoryStream memoryStream = new MemoryStream(binFile.Contents);
                await memoryStream.CopyToAsync(binStream, token);
                await binStream.FlushAsync(token);
                _logger.LogInformation($"Firmware file {binFile.File} saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving firmware file {binFile.File}.");
                throw;
            }
        }
    }
}
