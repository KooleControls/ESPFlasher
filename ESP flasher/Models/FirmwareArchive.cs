namespace ESP_Flasher.Models
{
    public class FirmwareArchive
    {
        public string ZipFile { get; set; } = string.Empty;
        public List<BinFile> Entries { get; set; } = new List<BinFile>();
        public PartitionTable? PartitionTable { get; set; }
    }


}

