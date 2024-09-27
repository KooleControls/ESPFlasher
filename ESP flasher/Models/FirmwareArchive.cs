namespace ESP_Flasher.Models
{
    public class FirmwareArchive
    {
        public List<BinFile> BinFiles { get; set; } = new List<BinFile>();
        public List<ElfFile> ElfFiles { get; set; } = new List<ElfFile>();
    }


}

