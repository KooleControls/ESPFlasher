namespace ESP_Flasher.Models
{
    public class PartitionEntry
    {
        public byte Type { get; set; }
        public byte Subtype { get; set; }
        public uint Address { get; set; }
        public uint Size { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => $"{Name} Type: {Type:X2}.{Subtype:X2}, Address: 0x{Address:X8}, Size: 0x{Size:X8}";
    }


}

