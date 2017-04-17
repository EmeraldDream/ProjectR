namespace FlowInLib
{
    public class DataBuff
    {
        public static uint MAX_BUFF_LEN = 2048;

        public uint _readPos = 0;
        public uint _dataLen = 0;
        public byte[] _data = new byte[MAX_BUFF_LEN];

        public uint GetUnreadLen() { return _dataLen - _readPos; }
        public uint GetUnwriteLen() { return MAX_BUFF_LEN - _dataLen; }
        public void Clear() { _readPos = 0; _dataLen = 0; }
        public uint GetReadPos() { return _readPos; }
        public uint GetWritePos() { return _dataLen; }
    }

    public class FileDataBuff
    {
        public static uint MAX_FILE_SIZE = 20480;

        public uint _readPos = 0;
        public uint _dataLen = 0;
        public byte[] _data = new byte[MAX_FILE_SIZE];

        public uint GetUnreadLen() { return _dataLen - _readPos; }
        public uint GetUnwriteLen() { return MAX_FILE_SIZE - _dataLen; }
        public void Clear() { _readPos = 0; _dataLen = 0; }
        public uint GetReadPos() { return _readPos; }
        public uint GetWritePos() { return _dataLen; }
    }
}