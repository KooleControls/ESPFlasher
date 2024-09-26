using System.Text;
using ESP_Flasher.Models;

namespace ESP_Flasher.Parsers
{
    public class PartitionTableParser
    {
        public PartitionTable Parse(Stream stream)
        {
            PartitionTable table = new PartitionTable();
            byte[] data = new byte[0x20];
            stream.Read(data, 0, data.Length);

            while (data[0] != 0xFF)
            {
                if (data[0] == 0xAA)
                {
                    PartitionEntry entry = ParsePartition(data);
                    table.Partitions.Add(entry);
                }
                stream.Read(data, 0, data.Length);
            }

            return table;
        }

        private PartitionEntry ParsePartition(byte[] rawData)
        {
            return new PartitionEntry
            {
                Type = rawData[2],
                Subtype = rawData[3],
                Address = BitConverter.ToUInt32(rawData, 4),
                Size = BitConverter.ToUInt32(rawData, 8),
                Name = Encoding.ASCII.GetString(rawData, 12, 32 - 12).TrimEnd('\0')
            };
        }
    }


}

