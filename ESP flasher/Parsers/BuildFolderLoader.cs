using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ESP_Flasher.Parsers
{
    public class BuildFolderLoader
    {
        private const string SettingsFileName = "Settings.json"; // Use a constant for clarity
        private readonly ILogger<BuildFolderLoader> _logger;

        public BuildFolderLoader(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BuildFolderLoader>();
        }

        /// <summary>
        /// Loads a firmware archive from a build directory using the args file.
        /// </summary>
        /// <param name="argsFilePath">Path to the args file.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Returns a firmware archive object.</returns>
        public async Task<FirmwareArchive?> LoadArchiveAsync(string argsFilePath, CancellationToken token = default)
        {
            FirmwareArchive archive = new FirmwareArchive();
            string? buildFolderPath = Path.GetDirectoryName(argsFilePath);

            if (string.IsNullOrEmpty(buildFolderPath))
            {
                _logger.LogError("Invalid build directory path.");
                return null;
            }

            // Load BIN files from the args file
            archive.BinFiles = await LoadBinFilesUsingArgsFileAsync(argsFilePath, buildFolderPath, token);

            // Load ELF files from the build directory
            archive.ElfFiles = await LoadElfFilesAsync(buildFolderPath, token);

            return archive;
        }

        /// <summary>
        /// Loads BIN files based on the entries from the args file.
        /// </summary>
        private async Task<List<BinFile>> LoadBinFilesUsingArgsFileAsync(string argsFilePath, string buildFolderPath, CancellationToken token)
        {
            List<BinFile> binFiles = new List<BinFile>();

            try
            {
                using StreamReader reader = new StreamReader(argsFilePath);

                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Parse the line to extract the address and file path
                    Match match = Regex.Match(line, "(0x[0-9a-fA-F]+) (.+)");
                    if (match.Success)
                    {
                        string? binFilePath = Directory.GetFiles(buildFolderPath, match.Groups[2].Value.Replace('/', '\\'), SearchOption.AllDirectories).FirstOrDefault();

                        if (string.IsNullOrEmpty(binFilePath) || !File.Exists(binFilePath))
                        {
                            _logger.LogWarning($"File {match.Groups[2].Value} not found.");
                            continue;
                        }

                        // Load the BIN file and its address
                        BinFile binFile = new BinFile
                        {
                            File = Path.GetFileName(binFilePath),
                            Address = Convert.ToInt32(match.Groups[1].Value, 16),
                            Contents = await File.ReadAllBytesAsync(binFilePath, token)
                        };

                        binFiles.Add(binFile);
                        _logger.LogInformation($"BIN file {binFile.File} loaded.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing the args file.");
            }

            return binFiles;
        }

        /// <summary>
        /// Loads ELF files from the build directory.
        /// </summary>
        private async Task<List<ElfFile>> LoadElfFilesAsync(string buildFolderPath, CancellationToken token)
        {
            List<ElfFile> elfFiles = new List<ElfFile>();

            try
            {
                // Get all ELF files from the build folder
                var elfFilePaths = Directory.GetFiles(buildFolderPath, "*.elf", SearchOption.AllDirectories);

                foreach (var elfFilePath in elfFilePaths)
                {
                    ElfFile elfFile = new ElfFile
                    {
                        File = Path.GetFileName(elfFilePath),
                        Contents = await File.ReadAllBytesAsync(elfFilePath, token)
                    };

                    elfFiles.Add(elfFile);
                    _logger.LogInformation($"ELF file {elfFile.File} loaded.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ELF files.");
            }

            return elfFiles;
        }
    }
}

