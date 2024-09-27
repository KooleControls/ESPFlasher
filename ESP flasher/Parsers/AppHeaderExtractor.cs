using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ESP_Flasher.Services
{
    public class AppHeaderExtractor
    {
        // Constants for magic bytes and words
        private const byte ESP_IMAGE_HEADER_MAGIC = 0xE9;
        private const uint ESP_APP_DESC_MAGIC_WORD = 0xABCD5432;

        private readonly ILogger<AppHeaderExtractor> _logger;

        public AppHeaderExtractor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AppHeaderExtractor>();
        }

        /// <summary>
        /// Extracts the application header from a firmware archive.
        /// </summary>
        /// <param name="archive">The firmware archive to extract from.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        /// <returns>An AppHeader object if a valid header is found, otherwise null.</returns>
        public async Task<AppHeader?> ExtractHeaderAsync(FirmwareArchive archive, CancellationToken token = default)
        {
            foreach (var binFile in archive.BinFiles)
            {
                using MemoryStream stream = new MemoryStream(binFile.Contents);
                AppHeader? header = await ParseAppHeaderAsync(stream, token);

                // Check if the header was valid
                if (header != null && header.Magic == ESP_IMAGE_HEADER_MAGIC && header.MagicWord == ESP_APP_DESC_MAGIC_WORD)
                {
                    _logger.LogInformation($"App header found in {binFile.File}");
                    return header;
                }
            }

            _logger.LogWarning("No valid app header found in the firmware archive.");
            return null;
        }

        /// <summary>
        /// Parses the application header from the binary stream.
        /// </summary>
        /// <param name="stream">The memory stream containing the binary data.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        /// <returns>An AppHeader object if valid, otherwise null.</returns>
        public async Task<AppHeader?> ParseAppHeaderAsync(MemoryStream stream, CancellationToken token = default)
        {
            try
            {
                AppHeader header = new AppHeader();
                using BinaryReader reader = new BinaryReader(stream);

                // Parse header fields
                header.Magic = reader.ReadByte(); // Byte 0xE9

                // Validate the magic byte
                if (header.Magic != ESP_IMAGE_HEADER_MAGIC)
                {
                    _logger.LogWarning("Invalid magic byte found in app header.");
                    return null;
                }

                header.SegmentCount = reader.ReadByte(); // Number of segments
                header.SpiMode = reader.ReadByte(); // SPI mode

                byte spiData = reader.ReadByte();
                header.SpiSpeed = (byte)(spiData & 0xF); // SPI speed (last 4 bits)
                header.SpiSize = (byte)(spiData >> 4); // SPI size (first 4 bits)

                header.EntryAddr = reader.ReadUInt32(); // Entry address
                header.WpPin = reader.ReadByte(); // WP Pin

                header.SpiPinDrv = reader.ReadBytes(3); // SPI Pin drive settings (3 bytes)
                header.Reserved = reader.ReadBytes(11); // Reserved bytes (11 bytes)

                header.HashAppended = reader.ReadByte(); // Hash Appended flag

                // Skip junk data (8 bytes)
                reader.ReadBytes(8);

                header.MagicWord = reader.ReadUInt32(); // Magic word

                // Validate the magic word
                if (header.MagicWord != ESP_APP_DESC_MAGIC_WORD)
                {
                    _logger.LogWarning("Invalid magic word found in app header.");
                    return null;
                }

                // Parse additional header fields
                header.SecureVersion = reader.ReadUInt32();
                header.Reserv1 = reader.ReadUInt32();
                header.Reserv2 = reader.ReadUInt32();

                header.Version = Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0');
                header.ProjectName = Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0');
                header.CompileTime = Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd('\0');
                header.CompileDate = Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd('\0');
                header.IdfVer = Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0');

                header.AppElfSha256 = reader.ReadBytes(32); // SHA256 of ELF file

                return header;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing app header.");
                return null;
            }
        }
    }

}

