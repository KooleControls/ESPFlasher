using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;

namespace ESP_Flasher.Parsers
{
    public class ZipArchiveLoader
    {
        private const string SettingsFileName = "Settings.json"; // Use a constant for clarity
        private readonly ILogger<ZipArchiveLoader> _logger;

        public ZipArchiveLoader(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ZipArchiveLoader>();
        }

        /// <summary>
        /// Loads a firmware archive from a ZIP file.
        /// </summary>
        /// <param name="zipFile">Path to the ZIP file.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Firmware archive with BIN and ELF files loaded.</returns>
        public async Task<FirmwareArchive?> LoadArchiveAsync(string zipFile, CancellationToken token = default)
        {
            try
            {
                using Stream zipStream = File.OpenRead(zipFile);
                using ZipArchive archiveZip = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: false);

                // Initialize FirmwareArchive and load the bin and elf files
                FirmwareArchive firmwareArchive = new FirmwareArchive
                {
                    BinFiles = await LoadBinFilesFromSettingsJsonAsync(archiveZip, token),
                    ElfFiles = await LoadElfFilesAsync(archiveZip, token)
                };

                return firmwareArchive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading firmware archive from ZIP: {ZipFile}", zipFile);
                return null;
            }
        }

        /// <summary>
        /// Loads BIN files based on the settings JSON from the archive.
        /// </summary>
        private async Task<List<BinFile>> LoadBinFilesFromSettingsJsonAsync(ZipArchive archiveZip, CancellationToken token = default)
        {
            List<BinFile> binFiles = new List<BinFile>();
            try
            {
                ZipArchiveEntry? settingsEntry = archiveZip.GetEntry(SettingsFileName);
                if (settingsEntry == null)
                {
                    _logger.LogError("Settings file {SettingsFileName} not found in the archive.", SettingsFileName);
                    return binFiles;
                }

                // Read and deserialize the JSON content from the settings file
                using StreamReader reader = new StreamReader(settingsEntry.Open());
                string jsonContent = await reader.ReadToEndAsync();
                binFiles = JsonSerializer.Deserialize<List<BinFile>>(jsonContent) ?? new List<BinFile>();

                // Load the contents of each BIN file from the ZIP
                foreach (var binFile in binFiles)
                {
                    ZipArchiveEntry? entry = archiveZip.GetEntry(binFile.File);
                    if (entry == null)
                    {
                        _logger.LogError("BIN file {File} not found in the archive.", binFile.File);
                        continue;
                    }

                    binFile.Contents = new byte[entry.Length];
                    using Stream entryStream = entry.Open();
                    await entryStream.ReadAsync(binFile.Contents, 0, binFile.Contents.Length, token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading BIN files from {SettingsFileName}.", SettingsFileName);
            }

            return binFiles;
        }

        /// <summary>
        /// Loads ELF files directly from the archive.
        /// </summary>
        private async Task<List<ElfFile>> LoadElfFilesAsync(ZipArchive archiveZip, CancellationToken token = default)
        {
            List<ElfFile> elfFiles = new List<ElfFile>();
            try
            {
                // Search the ZIP for all entries with the .elf extension
                foreach (var entry in archiveZip.Entries.Where(e => e.Name.EndsWith(".elf", StringComparison.OrdinalIgnoreCase)))
                {
                    ElfFile elfFile = new ElfFile
                    {
                        File = entry.Name,
                        Contents = new byte[entry.Length]
                    };

                    using Stream entryStream = entry.Open();
                    await entryStream.ReadAsync(elfFile.Contents, 0, elfFile.Contents.Length, token);
                    elfFiles.Add(elfFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ELF files from archive.");
            }

            return elfFiles;
        }
    }
}

