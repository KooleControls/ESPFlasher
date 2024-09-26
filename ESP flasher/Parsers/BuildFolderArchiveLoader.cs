using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ESP_Flasher.Parsers
{
    public class BuildFolderArchiveLoader
    {
        private readonly ILogger<BuildFolderArchiveLoader> _logger;
        public string PartitionTableFileName { get; set; } = "partition-table.bin"; // Configurable name for partition table file

        public BuildFolderArchiveLoader(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BuildFolderArchiveLoader>();
        }

        // Load archive from a build directory and an args file
        public async Task<FirmwareArchive?> LoadFromBuildDirectory(string argsFilePath, CancellationToken token = default)
        {
            FirmwareArchive archive = new FirmwareArchive();
            string? buildFolderPath = Path.GetDirectoryName(argsFilePath);

            if (string.IsNullOrEmpty(buildFolderPath))
            {
                _logger.LogError("Invalid build directory path.");
                return null;
            }

            // Load entries from the args file
            archive.Entries = await ParseArgsFile(argsFilePath, buildFolderPath, token);

            // Try to load the partition table
            string partitionTableFilePath = Path.Combine(buildFolderPath, PartitionTableFileName);
            archive.PartitionTable = await LoadPartitionTable(partitionTableFilePath, token) ?? new PartitionTable();

            return archive;
        }

        // Separate method to parse the args file and extract binary files
        private async Task<List<BinFile>> ParseArgsFile(string argsFilePath, string buildFolderPath, CancellationToken token)
        {
            List<BinFile> entries = new List<BinFile>();

            try
            {
                using StreamReader rdr = new StreamReader(argsFilePath);

                while (!rdr.EndOfStream)
                {
                    string? line = await rdr.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;

                    Match match = Regex.Match(line, "(0x[0-9a-fA-F]+) (.+)");
                    if (match.Success)
                    {
                        string binFilePath = Path.Combine(buildFolderPath, match.Groups[2].Value.Replace('/', '\\'));
                        if (!File.Exists(binFilePath))
                        {
                            _logger.LogWarning($"File {binFilePath} not found.");
                            continue;
                        }

                        BinFile binFile = new BinFile
                        {
                            File = Path.GetFileName(binFilePath),
                            Address = Convert.ToInt32(match.Groups[1].Value, 16),
                            Contents = await File.ReadAllBytesAsync(binFilePath, token)
                        };

                        entries.Add(binFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing the args file.");
            }

            return entries;
        }

        // Private method to load partition-table.bin
        private async Task<PartitionTable?> LoadPartitionTable(string partitionTableFilePath, CancellationToken token = default)
        {
            try
            {
                if (!File.Exists(partitionTableFilePath))
                {
                    _logger.LogWarning($"Partition table file {PartitionTableFileName} not found.");
                    return null;
                }

                PartitionTableParser partitionParser = new PartitionTableParser();
                using Stream partitionTableStream = File.OpenRead(partitionTableFilePath);
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
