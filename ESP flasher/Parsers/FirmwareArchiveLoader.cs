using ESP_Flasher.Models;
using System.IO.Compression;
using System.Text.Json;

namespace ESP_Flasher.Parsers
{
    public class FirmwareArchiveLoader
    {
        // Configurable properties for file names
        public string SettingsFileName { get; set; } = "Settings.json";
        public string PartitionTableFileName { get; set; } = "partition-table.bin";

        // Parse the firmware archive from a stream
        public FirmwareArchive? Parse(Stream stream)
        {
            var firmwareArchive = new FirmwareArchive();

            try
            {
                using ZipArchive archiveZip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

                // Attempt to load various components of the firmware archive
                firmwareArchive.Entries = LoadSettingsFile(archiveZip);
                firmwareArchive.PartitionTable = LoadPartitionTable(archiveZip);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading firmware archive: {ex.Message}");
                return null;
            }

            return firmwareArchive;
        }

        // Private method to load settings.json
        private List<BinFile> LoadSettingsFile(ZipArchive archiveZip)
        {
            try
            {
                ZipArchiveEntry? settingsEntry = archiveZip.GetEntry(SettingsFileName);
                if (settingsEntry == null)
                {
                    MessageBox.Show($"{SettingsFileName} not found in the archive.");
                    return null;
                }

                using StreamReader reader = new StreamReader(settingsEntry.Open());
                string jsonContent = reader.ReadToEnd();

                var binFiles = JsonSerializer.Deserialize<List<BinFile>>(jsonContent) ?? new List<BinFile>();

                foreach(var binFile in binFiles)
                {
                    ZipArchiveEntry? entry = archiveZip.GetEntry(binFile.File) ?? throw new Exception("File not found");
                    binFile.Size = entry.Length;
                }

                return binFiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {SettingsFileName}: {ex.Message}");
            }

            return new List<BinFile>();
        }

        // Private method to load partition-table.bin
        private PartitionTable? LoadPartitionTable(ZipArchive archiveZip)
        {
            try
            {
                ZipArchiveEntry? partitionTableEntry = archiveZip.GetEntry(PartitionTableFileName);
                if (partitionTableEntry == null)
                {
                    // No partition table found, this may be acceptable in some cases
                    return null;
                }

                PartitionTableParser partitionParser = new PartitionTableParser();
                using Stream partitionTableStream = partitionTableEntry.Open();
                return partitionParser.Parse(partitionTableStream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {PartitionTableFileName}: {ex.Message}");
                return null;
            }
        }
    }


}

