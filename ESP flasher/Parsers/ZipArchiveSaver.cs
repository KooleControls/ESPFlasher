using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;

namespace ESP_Flasher.Parsers
{
    public class ZipArchiveSaver
    {
        private const string SettingsFileName = "Settings.json"; // Use a constant for clarity
        private readonly ILogger<ZipArchiveSaver> _logger;

        public ZipArchiveSaver(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ZipArchiveSaver>();
        }

        /// <summary>
        /// Saves a firmware archive to a ZIP file.
        /// </summary>
        /// <param name="zipFile">Path to the ZIP file to save.</param>
        /// <param name="firmwareArchive">The firmware archive object to save.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task SaveArchiveAsync(string zipFile, FirmwareArchive firmwareArchive, CancellationToken token = default)
        {
            try
            {
                using FileStream zipToOpen = new FileStream(zipFile, FileMode.Create);
                using ZipArchive zipArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Create);

                // Save settings.json file
                await SaveSettingsJsonAsync(zipArchive, firmwareArchive, token);

                // Save each firmware binary entry
                foreach (var binFile in firmwareArchive.BinFiles)
                {
                    await SaveBinFileAsync(zipArchive, binFile, token);
                }

                // Save each ELF file
                foreach (var elfFile in firmwareArchive.ElfFiles)
                {
                    await SaveElfFileAsync(zipArchive, elfFile, token);
                }

                _logger.LogInformation($"Firmware archive successfully saved to {zipFile}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving firmware archive to ZIP.");
                throw; // Re-throw the exception to inform the caller.
            }
        }

        /// <summary>
        /// Saves the settings.json file that contains BIN file metadata.
        /// </summary>
        private async Task SaveSettingsJsonAsync(ZipArchive archive, FirmwareArchive firmwareArchive, CancellationToken token)
        {
            try
            {
                ZipArchiveEntry settingsEntry = archive.CreateEntry(SettingsFileName);
                using StreamWriter writer = new StreamWriter(settingsEntry.Open());

                // Serialize the BIN file metadata to JSON
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

        /// <summary>
        /// Saves an individual BIN file from the firmware archive to the ZIP.
        /// </summary>
        private async Task SaveBinFileAsync(ZipArchive archive, BinFile binFile, CancellationToken token)
        {
            try
            {
                ZipArchiveEntry binEntry = archive.CreateEntry(binFile.File);
                using Stream binStream = binEntry.Open();

                // Write the BIN file contents to the ZIP archive
                using MemoryStream contentStream = new MemoryStream(binFile.Contents);
                await contentStream.CopyToAsync(binStream, token);
                await binStream.FlushAsync(token);

                _logger.LogInformation("BIN file {FileName} saved successfully.", binFile.File);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving BIN file {FileName}.", binFile.File);
                throw;
            }
        }

        /// <summary>
        /// Saves an individual ELF file from the firmware archive to the ZIP.
        /// </summary>
        private async Task SaveElfFileAsync(ZipArchive archive, ElfFile elfFile, CancellationToken token)
        {
            try
            {
                ZipArchiveEntry elfEntry = archive.CreateEntry(elfFile.File);
                using Stream elfStream = elfEntry.Open();

                // Write the ELF file contents to the ZIP archive
                using MemoryStream contentStream = new MemoryStream(elfFile.Contents);
                await contentStream.CopyToAsync(elfStream, token);
                await elfStream.FlushAsync(token);

                _logger.LogInformation("ELF file {FileName} saved successfully.", elfFile.File);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving ELF file {FileName}.", elfFile.File);
                throw;
            }
        }
    }
}

