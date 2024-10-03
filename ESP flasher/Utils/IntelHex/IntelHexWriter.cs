namespace ESP_Flasher.Utils.IntelHex
{
    public class IntelHexWriter : IAsyncDisposable
    {
        private readonly StreamWriter _hexFileWriter;
        private readonly int _recordLength;
        private ushort _currentHighAddress = 0xFFFF;
        private bool _disposed = false;

        public IntelHexWriter(string hexFilePath, int recordLength = 16)
        {
            _hexFileWriter = new StreamWriter(File.Open(hexFilePath, FileMode.Create, FileAccess.Write));
            _recordLength = recordLength;
        }

        public IntelHexWriter(Stream hexFileStream, int recordLength = 16)
        {
            _hexFileWriter = new StreamWriter(hexFileStream);
            _recordLength = recordLength;
        }

        public async Task WriteHexRecordsAsync(Stream srcStream, long baseAddress, CancellationToken token)
        {
            byte[] buffer = new byte[_recordLength];

            while (srcStream.Position < srcStream.Length)
            {
                long address = baseAddress + srcStream.Position;
                ushort lowAddress = (ushort)(address & 0xFFFF);
                ushort highAddress = (ushort)((address >> 16) & 0xFFFF);
                int length = await srcStream.ReadAsync(buffer.AsMemory(0, Math.Min(_recordLength, (int)(srcStream.Length - srcStream.Position))), token);

                if (highAddress != _currentHighAddress)
                {
                    var extendedAddressRecord = new IntelHexRecord
                    {
                        Address = 0x0000,
                        RecordType = IntelHexRecordType.ExtendedLinearAddress,
                        Data = BitConverter.GetBytes(highAddress).Reverse().ToArray()
                    };
                    await _hexFileWriter.WriteLineAsync(SerializeRecord(extendedAddressRecord).AsMemory(), token);
                    _currentHighAddress = highAddress;
                }

                var dataRecord = new IntelHexRecord
                {
                    Address = lowAddress,
                    RecordType = IntelHexRecordType.Data,
                    Data = buffer.AsSpan(0, length).ToArray()
                };
                await _hexFileWriter.WriteLineAsync(SerializeRecord(dataRecord).AsMemory(), token);
            }
        }

        public async Task WriteEndOfFileAsync(CancellationToken token)
        {
            var endOfFileRecord = new IntelHexRecord
            {
                Address = 0x0000,
                RecordType = IntelHexRecordType.EndOfFile,
                Data = Array.Empty<byte>()
            };
            await _hexFileWriter.WriteLineAsync(SerializeRecord(endOfFileRecord).AsMemory(), token);
        }

        public static string SerializeRecord(IntelHexRecord record)
        {
            var dataString = string.Concat(record.Data.Select(b => b.ToString("X2")));
            var byteCount = (byte)record.Data.Length;
            var addressHigh = (byte)(record.Address >> 8);
            var addressLow = (byte)(record.Address & 0xFF);
            var checksum = CalculateChecksum(byteCount, addressHigh, addressLow, (byte)record.RecordType, record.Data);

            return $":{byteCount:X2}{record.Address:X4}{(byte)record.RecordType:X2}{dataString}{checksum:X2}";
        }

        private static byte CalculateChecksum(byte byteCount, byte addressHigh, byte addressLow, byte recordType, byte[] data)
        {
            int checksum = byteCount + addressHigh + addressLow + recordType;
            foreach (var b in data)
            {
                checksum += b;
            }

            checksum = (~checksum + 1) & 0xFF;
            return (byte)checksum;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _hexFileWriter.DisposeAsync();
                _disposed = true;
            }
        }
    }
}
