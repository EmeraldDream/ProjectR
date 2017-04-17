namespace FlowInLib
{
    public interface ISerializableObj
    {
        uint Serialize(byte[] buff, uint offset, uint buffLen);
        uint Unserialize(byte[] buff, uint offset, uint buffLen);
        uint CalcDataLen();
        void Clear();
    }

    public class SerializeUtil
    {
        public static void LEMemcpy(byte[] dest, uint destOffset, byte[] src, uint srcOffset, uint len)
        {
            int temp = 1;
            bool bLittleEndian = (temp & 0xff) != 0x01;
            if (bLittleEndian)
            {
                for (uint i = 0; i < len; ++i)
                    dest[i + destOffset] = src[i + srcOffset];
            }
            else
            {
                for (uint i = 0; i < len; ++i)
                    dest[i + destOffset] = src[len - 1 - i + srcOffset];
            }
        }

        public static void Memcpy(byte[] dest, uint destOffset, byte[] src, uint srcOffset, uint len)
        {
            for (uint i = 0; i < len; ++i)
                dest[i + destOffset] = src[i + srcOffset];
        }
    }
}