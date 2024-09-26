namespace ESP_Flasher.Models
{
    public class FirmwareArchive
    {
        public List<BinFile> Entries { get; set; } = new List<BinFile>();
        public PartitionTable PartitionTable { get; set; } = new PartitionTable();
    }


}

