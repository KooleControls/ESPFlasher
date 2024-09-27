using ESP_Flasher.Models;
using ESP_Flasher.Services;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace ESP_Flasher.Parsers
{
    public class IntelHexSaver
    {
        private readonly ILogger<IntelHexSaver> _logger;
        private readonly AppHeaderExtractor _appHeaderExtractor;

        public IntelHexSaver(ILoggerFactory loggerFactory, AppHeaderExtractor appHeaderExtractor)
        {
            _logger = loggerFactory.CreateLogger<IntelHexSaver>();
            _appHeaderExtractor = appHeaderExtractor;
        }


        /// <summary>
        /// Saves the firmware archive into a hex file format, identifying the correct bin file based on a valid AppHeader.
        /// </summary>
        /// <param name="hexFile">The destination file path for the hex file.</param>
        /// <param name="firmwareArchive">The firmware archive to save.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        public async Task SaveArchiveAsync(string hexFile, FirmwareArchive firmwareArchive, CancellationToken token = default)
        {
            try
            {
                // First, extract the app header to find the correct bin file
                AppHeader? validHeader = null;
                BinFile? targetBinFile = null;

                foreach (var binFile in firmwareArchive.BinFiles)
                {
                    // Create a memory stream from the bin file content to extract the header
                    using MemoryStream binStream = new MemoryStream(binFile.Contents);
                    var header = await _appHeaderExtractor.ParseAppHeaderAsync(binStream, token);

                    if (header != null)
                    {
                        validHeader = header;
                        targetBinFile = binFile;
                        break; // We found the correct bin file with a valid header
                    }
                }

                if (validHeader == null || targetBinFile == null)
                {
                    _logger.LogError("No valid app header found in the provided firmware archive.");
                    throw new Exception("Unable to find a valid app header in the firmware archive.");
                }

                // Proceed to save the correct bin file as a hex file
                await SaveBinFileAsHexAsync(hexFile, targetBinFile, token);

                _logger.LogInformation($"Firmware archive successfully saved to {hexFile} in hex format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving firmware archive to hex file.");
                throw;
            }
        }

        /// <summary>
        /// Saves a bin file to a hex format.
        /// </summary>
        /// <param name="hexFile">The destination hex file path.</param>
        /// <param name="binFile">The bin file to save.</param>
        /// <param name="token">Cancellation token for async operations.</param>
        private async Task SaveBinFileAsHexAsync(string hexFile, BinFile binFile, CancellationToken token)
        {
            try
            {
                using var fileStream = new FileStream(hexFile, FileMode.Create, FileAccess.Write, FileShare.None);
                using var contentStream = new MemoryStream(binFile.Contents);
                I32HEX.ToHex(contentStream, fileStream);
                await fileStream.FlushAsync(token);
                _logger.LogInformation($"Bin file {binFile.File} saved to hex format at {hexFile}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving bin file {binFile.File} to hex format.");
                throw;
            }
        }
    }


    public class I32HEX
    {
        public static byte ByteCount { get; set; } = 0x20;


        public static void ToHex(Stream srcStream, Stream dstStream)
        {
            StreamWriter result = new StreamWriter(dstStream);
            UInt16 currentHighAddress = 0xFFFF;
            Record record;

            while (srcStream.Position < srcStream.Length)
            {
                long address = srcStream.Position;
                UInt16 lowAddress = (UInt16)(address & 0xFFFF);
                UInt16 highAddress = (UInt16)((address >> 16) & 0xFFFF);
                byte len = (byte)Math.Min(ByteCount, srcStream.Length - srcStream.Position);
                byte[] buf = new byte[len];
                srcStream.Read(buf, 0, len);

                if (highAddress != currentHighAddress)
                {
                    record = new Record();
                    record.Address = 0x00;
                    record.Type = RecordType.ExtendedLinearAddress;
                    record.Data = BitConverter.GetBytes(highAddress).Reverse().ToArray();
                    result.WriteLine(record.ToString());
                    currentHighAddress = highAddress;
                }

                record = new Record();
                record.Address = lowAddress;
                record.Data = buf;
                record.Type = RecordType.Data;
                result.WriteLine(record.ToString());
            }

            record = new Record();
            record.Address = 0;
            record.Type = RecordType.EndOfFile;
            result.WriteLine(record.ToString());
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

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
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

