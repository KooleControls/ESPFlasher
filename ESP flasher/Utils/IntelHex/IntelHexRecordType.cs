namespace ESP_Flasher.Utils.IntelHex
{
    public enum IntelHexRecordType : byte
    {
        Data = 0,
        EndOfFile = 1,
        ExtendedLinearAddress = 4
    }
}
