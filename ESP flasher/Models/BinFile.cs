using System.Text.Json.Serialization;

namespace ESP_Flasher.Models
{
    public class BinFile
    {
        public int Address { get; set; }
        public string File { get; set; } = string.Empty;

        [JsonIgnore]
        public byte[] Contents { get; set; } = new byte[0];
    }
}

