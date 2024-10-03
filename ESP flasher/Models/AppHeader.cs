namespace ESP_Flasher.Models
{
    public class AppHeader
    {
        public byte Magic { get; set; }
        public byte SegmentCount { get; set; }
        public byte SpiMode { get; set; }
        public byte SpiSpeed { get; set; }
        public byte SpiSize { get; set; }
        public uint EntryAddr { get; set; }
        public byte WpPin { get; set; }
        public byte[] SpiPinDrv { get; set; } = new byte[3];
        public byte[] Reserved { get; set; } = new byte[11];
        public byte HashAppended { get; set; }

        public UInt32 MagicWord { get; set; }      
        public UInt32 SecureVersion { get; set; }
        public UInt32 Reserv1 { get; set; }
        public UInt32 Reserv2 { get; set; }
        public string Version { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string CompileTime { get; set; } = string.Empty;
        public string CompileDate { get; set; } = string.Empty;
        public string IdfVer { get; set; } = string.Empty;
        public byte[] AppElfSha256 { get; set; } = new byte[32];
    }

}
