using EspDotNet.Tools.Firmware;

namespace ESP_Flasher.Models
{
    public class FirmwareArchive : IFirmwareProvider
    {
        public List<BinFile> BinFiles { get; set; } = new List<BinFile>();
        public List<ElfFile> ElfFiles { get; set; } = new List<ElfFile>();

        public uint EntryPoint => 0x0;

        public IReadOnlyList<IFirmwareSegmentProvider> Segments => BinFiles;
    }


}

