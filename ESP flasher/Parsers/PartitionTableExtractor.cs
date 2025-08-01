using ESP_Flasher.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ESP_Flasher.Services
{
    public class PartitionTableExtractor
    {
        private const string PartitionTableFileName = "partition-table.bin"; // Configurable name for the partition table file

        private readonly ILogger<PartitionTableExtractor> _logger;

        public PartitionTableExtractor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PartitionTableExtractor>();
        }

        /// <summary>
        /// Extracts the partition table from the provided firmware archive.
        /// </summary>
        /// <param name="archive">The firmware archive to extract from.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        /// <returns>A parsed PartitionTable object or null if extraction fails.</returns>
        public async Task<PartitionTable?> ExtractTableAsync(FirmwareArchive archive, CancellationToken token = default)
        {
            _logger.LogInformation("Extracting partition table from the archive...");

            // Find the partition table file in the archive
            var partitionTableFile = archive.BinFiles.FirstOrDefault(file => file.File == PartitionTableFileName);

            if (partitionTableFile == null)
            {
                _logger.LogWarning("Partition table file {PartitionTableFileName} not found in the archive.", PartitionTableFileName);
                return null; // Partition table not found
            }

            _logger.LogInformation("Partition table file found: {PartitionTableFileName}", partitionTableFile.File);

            // Parse the partition table from the binary file
            return await ParsePartitionTableAsync(new MemoryStream(partitionTableFile.Contents), token);
        }

        /// <summary>
        /// Parses the partition table from a binary stream.
        /// </summary>
        /// <param name="stream">The memory stream containing the partition table data.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        /// <returns>A PartitionTable object or null if parsing fails.</returns>
        public async Task<PartitionTable> ParsePartitionTableAsync(Stream stream, CancellationToken token)
        {
            PartitionTable table = new PartitionTable();
            byte[] buffer = new byte[0x20]; // Buffer size for each partition entry

            // Read the partition data in chunks of 32 bytes (0x20)
            while (await stream.ReadAsync(buffer, 0, buffer.Length, token) > 0)
            {
                token.ThrowIfCancellationRequested();

                // If the entry starts with 0xFF, it's the end of the partition table
                if (buffer[0] == 0xFF)
                    break;
                
                // Only process entries that start with 0xAA
                if (buffer[0] == 0xAA)
                {
                    PartitionEntry entry = ParsePartition(buffer);
                    table.Partitions.Add(entry);
                }
            }

            return table;
        }

        /// <summary>
        /// Parses a partition entry from raw binary data.
        /// </summary>
        /// <param name="rawData">A byte array representing a single partition entry.</param>
        /// <returns>A PartitionEntry object with parsed data.</returns>
        private PartitionEntry ParsePartition(byte[] rawData)
        {
            return new PartitionEntry
            {
                Type = rawData[2], // Partition type
                Subtype = rawData[3], // Partition subtype
                Address = BitConverter.ToUInt32(rawData, 4), // Starting address of the partition
                Size = BitConverter.ToUInt32(rawData, 8), // Size of the partition
                Name = Encoding.ASCII.GetString(rawData, 12, 32 - 12).TrimEnd('\0') // Partition name
            };
        }
    }
}