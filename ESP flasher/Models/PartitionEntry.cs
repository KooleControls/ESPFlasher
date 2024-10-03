namespace ESP_Flasher.Models
{
    public class PartitionEntry
    {
        public byte Type { get; set; }
        public byte Subtype { get; set; }
        public uint Address { get; set; }
        public uint Size { get; set; }
        public string Name { get; set; } = string.Empty;
    }


}

