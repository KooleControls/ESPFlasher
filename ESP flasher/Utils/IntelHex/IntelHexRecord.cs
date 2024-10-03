namespace ESP_Flasher.Utils.IntelHex
{
    public class IntelHexRecord
    {
        public char StartCode { get; } = ':';
        public byte ByteCount => (byte)Data.Length;
        public ushort Address { get; set; }
        public IntelHexRecordType RecordType { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
