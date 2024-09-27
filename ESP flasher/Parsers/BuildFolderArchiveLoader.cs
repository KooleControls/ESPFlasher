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
            archive.BinFiles = await ParseArgsFile(argsFilePath, buildFolderPath, token);

            // Find all elf files
            archive.ElfFiles = await ParseElfFiles(buildFolderPath, token);

            // Try to load the partition table
            var partitionTable = archive.BinFiles.FirstOrDefault(e => e.File == PartitionTableFileName);
            if (partitionTable == null) {
                _logger.LogError($"Partition table {PartitionTableFileName} not found.");
            }
            else {
                archive.PartitionTable = await LoadPartitionTable(partitionTable, token) ?? new PartitionTable();
            }

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
                        string? binFilePath = Directory.GetFiles(buildFolderPath, match.Groups[2].Value.Replace('/', '\\'), SearchOption.AllDirectories).FirstOrDefault();

                        if (string.IsNullOrEmpty(binFilePath) || !File.Exists(binFilePath))
                        {
                            _logger.LogWarning($"File {match.Groups[2].Value} not found.");
                            continue;
                        }

                        BinFile binFile = new BinFile
                        {
                            File = Path.GetFileName(binFilePath),
                            Address = Convert.ToInt32(match.Groups[1].Value, 16),
                            Contents = await File.ReadAllBytesAsync(binFilePath, token)
                        };

                        _logger.LogInformation($"File {match.Groups[2].Value} loaded.");
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
        private async Task<PartitionTable?> LoadPartitionTable(BinFile partitionFile, CancellationToken token = default)
        {
            try
            {
                PartitionTableParser partitionParser = new PartitionTableParser();
                using MemoryStream stream = new MemoryStream(partitionFile.Contents); 
                return partitionParser.Parse(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading partition table {PartitionTableFileName}.", PartitionTableFileName);
                return null;
            }
        }

        private async Task<List<BinFile>> ParseElfFiles(string buildFolderPath, CancellationToken token)
        {
            List<BinFile> entries = new List<BinFile>();

            try
            {
                var elfFiles = Directory.GetFiles(buildFolderPath, "*.elf", SearchOption.AllDirectories);

                foreach(var elfFile in elfFiles)
                {
                    var file = new BinFile
                    {
                        Address = 0x0,
                        File = Path.GetFileName(elfFile),
                        Contents = await File.ReadAllBytesAsync(elfFile, token)
                    };

                    entries.Add(file);
                    _logger.LogInformation($"File {file.File} loaded.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing the args file.");
            }

            return entries;
        }
    }
}
