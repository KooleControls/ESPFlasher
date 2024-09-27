namespace ESP_Flasher.Models
{
    public class FirmwareArchive
    {
        public List<BinFile> BinFiles { get; set; } = new List<BinFile>();
        public List<BinFile> ElfFiles { get; set; } = new List<BinFile>();
        public PartitionTable PartitionTable { get; set; } = new PartitionTable();
    }


}

