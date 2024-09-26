using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;

namespace ESP_Flasher.Parsers
{
    public class FirmwareArchiveLoader
    {
        // Configurable properties for file names
        public string SettingsFileName { get; set; } = "Settings.json";
        public string PartitionTableFileName { get; set; } = "partition-table.bin";

        private readonly ILogger<FirmwareArchiveLoader> _logger;

        public FirmwareArchiveLoader(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FirmwareArchiveLoader>();
        }

        // Load the archive from a zip file stream
        public async Task<FirmwareArchive?> LoadFromZip(string zipFile, CancellationToken token = default)
        {
            try
            {
                using Stream stream = File.OpenRead(zipFile);
                using ZipArchive archiveZip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

                FirmwareArchive firmwareArchive = new FirmwareArchive
                {
                    Entries = await LoadEntriesUsingSettingsJson(archiveZip, token),
                    PartitionTable = await LoadPartitionTable(archiveZip, token) ?? new PartitionTable()
                };

                return firmwareArchive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading firmware archive from ZIP.");
                return null;
            }
        }

        // Private method to load entries from settings.json
        private async Task<List<BinFile>> LoadEntriesUsingSettingsJson(ZipArchive archiveZip, CancellationToken token = default)
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

                using StreamReader reader = new StreamReader(settingsEntry.Open());
                string jsonContent = await reader.ReadToEndAsync();

                binFiles = JsonSerializer.Deserialize<List<BinFile>>(jsonContent) ?? new List<BinFile>();
                foreach (var binFile in binFiles)
                {
                    ZipArchiveEntry? entry = archiveZip.GetEntry(binFile.File) ?? throw new Exception($"File {binFile.File} not found in the archive.");
                    binFile.Contents = new byte[entry.Length];
                    using MemoryStream copyStream = new MemoryStream(binFile.Contents);
                    using Stream entryStream = entry.Open();
                    await entryStream.CopyToAsync(copyStream, token);
                    copyStream.Position = 0;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading entries from {SettingsFileName}.", SettingsFileName);
            }

            return binFiles;
        }

        // Private method to load partition-table.bin
        private async Task<PartitionTable?> LoadPartitionTable(ZipArchive archiveZip, CancellationToken token = default)
        {
            try
            {
                ZipArchiveEntry? partitionTableEntry = archiveZip.GetEntry(PartitionTableFileName);
                if (partitionTableEntry == null)
                {
                    _logger.LogWarning("Partition table file {PartitionTableFileName} not found in the archive.", PartitionTableFileName);
                    return null;
                }

                PartitionTableParser partitionParser = new PartitionTableParser();
                using Stream partitionTableStream = partitionTableEntry.Open();
                return partitionParser.Parse(partitionTableStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading partition table {PartitionTableFileName}.", PartitionTableFileName);
                return null;
            }
        }
    }
}
