using EspDotNet.Tools.Firmware;
using System.Text.Json.Serialization;

namespace ESP_Flasher.Models
{
    public class BinFile : IFirmwareSegmentProvider
    {
        public int Address { get; set; }
        public string File { get; set; } = string.Empty;

        [JsonIgnore]
        public byte[] Contents { get; set; } = new byte[0];

        [JsonIgnore]
        public uint Offset => (uint)Address;

        [JsonIgnore]
        public uint Size => (uint)Contents.Length;

        public Task<Stream> GetStreamAsync(CancellationToken token = default) => Task.FromResult((Stream)new MemoryStream(Contents));
    }
}

