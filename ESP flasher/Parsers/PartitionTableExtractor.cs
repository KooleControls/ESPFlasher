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
            var partitionTableFile = archive.BinFiles.FirstOrDefault(file => file.File == PartitionTableFileName);

            if (partitionTableFile == null)
            {
                _logger.LogWarning("Partition table file {PartitionTableFileName} not found in the archive.", PartitionTableFileName);
                return null;
            }

            return await ParsePartitionTableAsync(new MemoryStream(partitionTableFile.Contents), token);
        }

        /// <summary>
        /// Parses the partition table from a binary stream.
        /// </summary>
        /// <param name="stream">The memory stream containing the partition table data.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        /// <returns>A PartitionTable object or null if parsing fails.</returns>
        private async Task<PartitionTable?> ParsePartitionTableAsync(MemoryStream stream, CancellationToken token)
        {
            PartitionTable table = new PartitionTable();
            byte[] buffer = new byte[0x20]; // 32-byte partition entries

            try
            {
                while (await stream.ReadAsync(buffer, 0, buffer.Length, token) > 0)
                {
                    token.ThrowIfCancellationRequested();

                    if (buffer[0] == 0xFF)
                        break; // end-of-table sentinel

                    if (buffer[0] == 0xAA)
                        table.Partitions.Add(ParsePartition(buffer));
                }

                _logger.LogInformation("Loaded partition table ({Count} entries) from {File}", table.Partitions.Count, PartitionTableFileName);
                return table;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while parsing the partition table.");
                return null;
            }
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