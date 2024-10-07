using ESP_Flasher.Models;
using ESP_Flasher.Services;
using ESP_Flasher.Utils.IntelHex;
using Microsoft.Extensions.Logging;

namespace ESP_Flasher.Parsers
{
    public class IntelHexSaver
    {
        private readonly ILogger<IntelHexSaver> _logger;
        private readonly AppHeaderExtractor _appHeaderExtractor;

        public IntelHexSaver(ILoggerFactory loggerFactory, AppHeaderExtractor appHeaderExtractor)
        {
            _logger = loggerFactory.CreateLogger<IntelHexSaver>();
            _appHeaderExtractor = appHeaderExtractor;
        }

        /// <summary>
        /// Saves the firmware archive into a hex file format, identifying the correct bin file based on a valid AppHeader.
        /// </summary>
        /// <param name="hexFile">The destination file path for the hex file.</param>
        /// <param name="firmwareArchive">The firmware archive to save.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        public async Task SaveArchiveAsync(string hexFile, FirmwareArchive firmwareArchive, CancellationToken token = default)
        {
            try
            {
                await using var writer = new IntelHexWriter(hexFile);

                // Store all bin files to hex format
                foreach (var binFile in firmwareArchive.BinFiles)
                {
                    await using var contentStream = new MemoryStream(binFile.Contents);
                    await writer.WriteHexRecordsAsync(contentStream, binFile.Address, token);
                }

                await writer.WriteEndOfFileAsync(token);

                _logger.LogInformation($"Firmware archive successfully saved to {hexFile} in hex format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving firmware archive to hex file.");
                throw;
            }
        }

        public async Task SaveApplicationAsync(string hexFile, FirmwareArchive firmwareArchive, CancellationToken token = default)
        {
            try
            {
                AppHeader? validHeader = null;
                BinFile? targetBinFile = null;

                foreach (var binFile in firmwareArchive.BinFiles)
                {
                    await using var binStream = new MemoryStream(binFile.Contents);
                    var header = await _appHeaderExtractor.ParseAppHeaderAsync(binStream, token);

                    if (header != null)
                    {
                        validHeader = header;
                        targetBinFile = binFile;
                        break;
                    }
                }

                if (validHeader == null || targetBinFile == null)
                {
                    _logger.LogError("No valid app header found in the provided firmware archive.");
                    throw new Exception("Unable to find a valid app header in the firmware archive.");
                }

                await using var writer = new IntelHexWriter(hexFile, recordLength: 32);
                await using var contentStream = new MemoryStream(targetBinFile.Contents);
                await writer.WriteHexRecordsAsync(contentStream, 0, token);

                await writer.WriteEndOfFileAsync(token);

                _logger.LogInformation($"Firmware archive successfully saved to {hexFile} in hex format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving firmware archive to hex file.");
                throw;
            }
        }
    }
}
