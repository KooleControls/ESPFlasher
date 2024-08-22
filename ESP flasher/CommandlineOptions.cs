using CmdArgParser;

namespace ESP_flasher
{
    public class CommandlineOptions
    {
        [Option('b', "Baudrate", "The baudrate used to comminucate with the ESP32")]
        public string Baudrate { get; set; }

        [Option('p', "Port", "The name of the comport used to comminucate with the ESP32")]
        public string Port { get; set; }

        [Option('f', "File", "The file to flash to the ESP32")]
        public string File { get; set; }

        [Option('e', "Erase", "Erases the ESP32")]
        public bool Erase { get; set; }

        [Option('F', "Flash", "Flashes the ESP32 with the software specified by -f")]
        public bool Flash { get; set; }

    }


}


