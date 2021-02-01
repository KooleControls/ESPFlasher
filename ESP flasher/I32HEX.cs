using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ESP_flasher
{
    public class I32HEX
    {
        public static byte ByteCount { get; set; } = 0x20;


        public static string ToHex(string binFile)
        {
            string result;
            using (Stream stream = File.OpenRead(binFile))
                result = ToHex(stream);
            return result;
        }

        public static string ToHex(byte[] data)
        {
            string result;
            using (MemoryStream stream = new MemoryStream(data))
                result = ToHex(stream);
            return result;
        }

        public static string ToHex(Stream stream)
        {
            StringBuilder result = new StringBuilder();

            UInt16 currentHighAddress = 0xFFFF;
            Record record;

            while (stream.Position < stream.Length)
            {
                long address = stream.Position;
                UInt16 lowAddress = (UInt16)(address & 0xFFFF);
                UInt16 highAddress = (UInt16)((address >> 16) & 0xFFFF);
                byte len = (byte)Math.Min(ByteCount, stream.Length - stream.Position);
                byte[] buf = new byte[len];
                stream.Read(buf, 0, len);



                if (highAddress != currentHighAddress)
                {
                    record = new Record();
                    record.Address = 0x00;
                    record.Type = RecordType.ExtendedLinearAddress;
                    record.Data = BitConverter.GetBytes(highAddress).Reverse().ToArray();
                    result.AppendLine(record.ToString());
                    currentHighAddress = highAddress;
                }

                record = new Record();
                record.Address = lowAddress;
                record.Data = buf;
                record.Type = RecordType.Data;
                result.AppendLine(record.ToString());
            }

            record = new Record();
            record.Address = 0;
            record.Type = RecordType.EndOfFile;
            result.AppendLine(record.ToString());

            return result.ToString();
        }


        private class Record
        {
            public char StartCode { get; set; } = ':';
            public byte ByteCount { get { return (byte)Data.Length; } }
            public UInt16 Address { get; set; } = 0;
            public RecordType Type { get; set; } = RecordType.Data;
            public byte[] Data { get; set; } = new byte[0];

            public byte GetChecksum()
            {
                byte checksum;

                checksum = (byte)ByteCount;
                checksum += (byte)Type;
                checksum += (byte)Address;
                checksum += (byte)((Address & 0xFF00) >> 8);
                for (int i = 0; i < ByteCount; i++)
                    checksum += Data[i];
                checksum = (byte)(~checksum + 1);
                return checksum;
            }

            public override string ToString()
            {
                return
                    $"{StartCode}" +
                    $"{ByteCount.ToString("X2")}" +
                    $"{Address.ToString("X4")}" +
                    $"{((byte)Type).ToString("X2")}" +
                    $"{ByteArrayToHexViaLookup32(Data)}" +
                    $"{GetChecksum().ToString("X2")}";
            }
        }


        enum RecordType : byte
        {
            Data = 0,
            EndOfFile = 1,
            ExtendedSegmentAddress = 2,
            StartSegmentAddress = 3,
            ExtendedLinearAddress = 4,
            StartLinearAddress = 5,
        }

        private static readonly uint[] _lookup32 = CreateLookup32();
        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        public static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }
    }
}
