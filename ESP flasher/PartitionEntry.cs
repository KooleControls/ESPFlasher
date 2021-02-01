using System;
using System.Text;

namespace ESP_flasher
{
    public class PartitionEntry
    {
        private static int nameLength = 0;
        public byte Type { get; set; }
        public byte Subtype { get; set; }
        public UInt32 Address { get; set; }
        public UInt32 Size { get; set; }
        public string Name { get; set; }

        public PartitionEntry(byte[] rawData)
        {
            Type = rawData[2];
            Subtype = rawData[3];
            Address = BitConverter.ToUInt32(rawData, 4);
            Size = BitConverter.ToUInt32(rawData, 8);
            Name = Encoding.ASCII.GetString(rawData, 12, 32 - 12).TrimEnd((Char)0);
        }


        string hr(float size)
        {
            string[] units = new string[] { "B", "kB", "MB", "GB", "TB", "PB" };
            int m = 0;

            while (size > 1024)
            {
                size /= 1024;
                m++;
            }

            return size.ToString("0." + new string('0', 3 - (int)Math.Log10(size))) + " " + units[m];
        }

        public override string ToString()
        {
            if (Name.Length > nameLength)
                nameLength = Name.Length;

            string name = Name + new string(' ', nameLength - Name.Length);

            string st = "";

            if (Type == 0)
            {
                switch (Subtype)
                {
                    case 0:
                        st = "Factory";
                        break;
                    case byte n when (0x10 >= n && n <= 0x1F):
                        st = "OTA_" + n.ToString("##") + " ";
                        break;
                    case 0x20:
                        st = "Test   ";
                        break;
                    default:
                        st = String.Format("0x{0:X2}   ", Subtype);
                        break;
                }
            }
            else if (Type == 1)
            {
                switch (Subtype)
                {
                    case 0:
                        st = "OTA    ";
                        break;
                    case 1:
                        st = "PHY    ";
                        break;
                    case 2:
                        st = "NVS    ";
                        break;
                    case 4:
                        st = "NVSKeys";
                        break;
                    default:
                        st = String.Format("0x{0:X2}   ", Subtype);
                        break;
                }
            }


            return String.Format("{0}    {1}    {2}    0x{3:X8}    {4}",
                name,
                Type == 0 ? "APP " : "DATA",
                st,
                Address,
                hr(Size));
        }
    }
}
