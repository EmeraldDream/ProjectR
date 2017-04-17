// This file is Auto Generated by IDLAnalyzer
// Please don't edit manually
using System;
using System.Collections.Generic;

namespace FlowInLib
{
	public class SampleStruct : ISerializableObj
	{
		public List<byte> name = null;
		public int age = 0;
		public int phone = 0;
		
		public void CreateInstance()
		{
			name = new List<byte>();
		}
		
		public void Clear()
		{
			name.Clear();
			age = 0;
			phone = 0;
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes((ushort)name.Count), 0, copyLen);
			buffPos += copyLen;
			
			if (name.Count > 255) return 0;
			if (name.Count > 0)
			{
				for (int i = 0; i < name.Count; ++i)
					buff[buffPos + i] = name[i];
				buffPos += (uint)name.Count;
			}
			
			copyLen = sizeof(int);
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes(age), 0, copyLen);
			buffPos += copyLen;
			
			copyLen = sizeof(int);
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes(phone), 0, copyLen);
			buffPos += copyLen;
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			if (copyLen > buffLen - buffPos) return 0;
			byte[] tempBytes = new byte[8];
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			uint nameLen = (uint)BitConverter.ToUInt16(tempBytes, 0);
			buffPos += copyLen;
			
			if (nameLen > 255) return 0;
			if (nameLen > 0)
			{
				if (nameLen > buffLen - buffPos) return 0;
				for (int i = 0; i < nameLen; ++i)
					name.Add(buff[buffPos + i]);
				buffPos += nameLen;
			}
			
			copyLen = sizeof(int);
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			age = BitConverter.ToInt32(tempBytes, 0);
			buffPos += copyLen;
			
			copyLen = sizeof(int);
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			phone = BitConverter.ToInt32(tempBytes, 0);
			buffPos += copyLen;
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += 2;
			dataLen += (uint)(sizeof(byte) * name.Count);
			dataLen += sizeof(int);
			dataLen += sizeof(int);
			return dataLen;
		}
	}
	public class StructA : ISerializableObj
	{
		public List<SampleStruct> list = null;
		public SampleStruct stru = null;
		public List<int> numList = null;
		
		public void CreateInstance()
		{
			list = new List<SampleStruct>();
			stru = new SampleStruct();
			numList = new List<int>();
		}
		
		public void Clear()
		{
			foreach (var item in list) item.Clear();
			list.Clear();
			stru.Clear();
			numList.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes((ushort)list.Count), 0, copyLen);
			buffPos += copyLen;
			
			if (list.Count > 1000) return 0;
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; ++i)
				{
					copyLen = list[i].Serialize(buff, buffPos, buffLen - (buffPos - offset));
					buffPos += copyLen;
				}
			}
			
			copyLen = stru.Serialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			copyLen = 2;
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes((ushort)numList.Count), 0, copyLen);
			buffPos += copyLen;
			
			if (numList.Count > 1000) return 0;
			if (numList.Count > 0)
			{
				copyLen = sizeof(int);
				for (int i = 0; i < numList.Count; ++i)
				{
					SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes(numList[i]), 0, copyLen);
					buffPos += copyLen;
				}
			}
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			if (copyLen > buffLen - buffPos) return 0;
			byte[] tempBytes = new byte[8];
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			uint listLen = (uint)BitConverter.ToUInt16(tempBytes, 0);
			buffPos += copyLen;
			
			if (listLen > 1000) return 0;
			if (listLen > 0)
			{
				for (int i = 0; i < listLen; ++i)
				{
					SampleStruct item = new SampleStruct();
					copyLen = item.Unserialize(buff, buffPos, buffLen - (buffPos - offset));
					list.Add(item);
					buffPos += copyLen;
				}
			}
			
			copyLen = stru.Unserialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			copyLen = 2;
			if (copyLen > buffLen - buffPos) return 0;
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			uint numListLen = (uint)BitConverter.ToUInt16(tempBytes, 0);
			buffPos += copyLen;
			
			if (numListLen > 1000) return 0;
			if (numListLen > 0)
			{
				copyLen = sizeof(int);
				if (numListLen * copyLen > buffLen - buffPos) return 0;
				for (int i = 0; i < numListLen; ++i)
				{
					SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
					int item = BitConverter.ToInt32(tempBytes, 0);
					numList.Add(item);
					buffPos += copyLen;
				}
			}
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += 2;
			for (int i = 0; i < list.Count; ++i)
				dataLen += list[i].CalcDataLen();
			dataLen += stru.CalcDataLen();
			dataLen += 2;
			dataLen += (uint)(sizeof(int) * numList.Count);
			return dataLen;
		}
	}
	public class AutoGenStructFor_sampleFuncReq : ISerializableObj
	{
		public SampleStruct ss = null;
		
		public void CreateInstance()
		{
			ss = new SampleStruct();
		}
		
		public void Clear()
		{
			ss.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = ss.Serialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = ss.Unserialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += ss.CalcDataLen();
			return dataLen;
		}
	}
	public class AutoGenStructFor_sampleFuncAck : ISerializableObj
	{
		public StructA sa = null;
		
		public void CreateInstance()
		{
			sa = new StructA();
		}
		
		public void Clear()
		{
			sa.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = sa.Serialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = sa.Unserialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += sa.CalcDataLen();
			return dataLen;
		}
	}
	public class AutoGenStructFor_sampleFuncPost : ISerializableObj
	{
		public SampleStruct ss = null;
		
		public void CreateInstance()
		{
			ss = new SampleStruct();
		}
		
		public void Clear()
		{
			ss.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = ss.Serialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = ss.Unserialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += ss.CalcDataLen();
			return dataLen;
		}
	}
	public class AutoGenStructFor_sampleFuncNtf : ISerializableObj
	{
		public StructA sa = null;
		
		public void CreateInstance()
		{
			sa = new StructA();
		}
		
		public void Clear()
		{
			sa.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = sa.Serialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = sa.Unserialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += sa.CalcDataLen();
			return dataLen;
		}
	}
	public class AutoGenStructFor_helloReq : ISerializableObj
	{
		public List<byte> strMsg = null;
		
		public void CreateInstance()
		{
			strMsg = new List<byte>();
		}
		
		public void Clear()
		{
			strMsg.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes((ushort)strMsg.Count), 0, copyLen);
			buffPos += copyLen;
			
			if (strMsg.Count > 1000) return 0;
			if (strMsg.Count > 0)
			{
				for (int i = 0; i < strMsg.Count; ++i)
					buff[buffPos + i] = strMsg[i];
				buffPos += (uint)strMsg.Count;
			}
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			if (copyLen > buffLen - buffPos) return 0;
			byte[] tempBytes = new byte[8];
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			uint strMsgLen = (uint)BitConverter.ToUInt16(tempBytes, 0);
			buffPos += copyLen;
			
			if (strMsgLen > 1000) return 0;
			if (strMsgLen > 0)
			{
				if (strMsgLen > buffLen - buffPos) return 0;
				for (int i = 0; i < strMsgLen; ++i)
					strMsg.Add(buff[buffPos + i]);
				buffPos += strMsgLen;
			}
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += 2;
			dataLen += (uint)(sizeof(byte) * strMsg.Count);
			return dataLen;
		}
	}
	public class AutoGenStructFor_helloAck : ISerializableObj
	{
		public List<byte> strMsg = null;
		
		public void CreateInstance()
		{
			strMsg = new List<byte>();
		}
		
		public void Clear()
		{
			strMsg.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes((ushort)strMsg.Count), 0, copyLen);
			buffPos += copyLen;
			
			if (strMsg.Count > 1000) return 0;
			if (strMsg.Count > 0)
			{
				for (int i = 0; i < strMsg.Count; ++i)
					buff[buffPos + i] = strMsg[i];
				buffPos += (uint)strMsg.Count;
			}
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = 2;
			if (copyLen > buffLen - buffPos) return 0;
			byte[] tempBytes = new byte[8];
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			uint strMsgLen = (uint)BitConverter.ToUInt16(tempBytes, 0);
			buffPos += copyLen;
			
			if (strMsgLen > 1000) return 0;
			if (strMsgLen > 0)
			{
				if (strMsgLen > buffLen - buffPos) return 0;
				for (int i = 0; i < strMsgLen; ++i)
					strMsg.Add(buff[buffPos + i]);
				buffPos += strMsgLen;
			}
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += 2;
			dataLen += (uint)(sizeof(byte) * strMsg.Count);
			return dataLen;
		}
	}
	public class AutoGenStructFor_towParamReq : ISerializableObj
	{
		public SampleStruct ss = null;
		public List<byte> name = null;
		
		public void CreateInstance()
		{
			ss = new SampleStruct();
			name = new List<byte>();
		}
		
		public void Clear()
		{
			ss.Clear();
			name.Clear();
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = ss.Serialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			copyLen = 2;
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes((ushort)name.Count), 0, copyLen);
			buffPos += copyLen;
			
			if (name.Count > 1000) return 0;
			if (name.Count > 0)
			{
				for (int i = 0; i < name.Count; ++i)
					buff[buffPos + i] = name[i];
				buffPos += (uint)name.Count;
			}
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = ss.Unserialize(buff, buffPos, buffLen - (buffPos - offset));
			buffPos += copyLen;
			
			copyLen = 2;
			if (copyLen > buffLen - buffPos) return 0;
			byte[] tempBytes = new byte[8];
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			uint nameLen = (uint)BitConverter.ToUInt16(tempBytes, 0);
			buffPos += copyLen;
			
			if (nameLen > 1000) return 0;
			if (nameLen > 0)
			{
				if (nameLen > buffLen - buffPos) return 0;
				for (int i = 0; i < nameLen; ++i)
					name.Add(buff[buffPos + i]);
				buffPos += nameLen;
			}
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += ss.CalcDataLen();
			dataLen += 2;
			dataLen += (uint)(sizeof(byte) * name.Count);
			return dataLen;
		}
	}
	public class AutoGenStructFor_towParamAck : ISerializableObj
	{
		public int count = 0;
		
		public void CreateInstance()
		{
		}
		
		public void Clear()
		{
			count = 0;
		}
		
		public uint Serialize (byte[] buff, uint offset, uint buffLen)
		{
			uint buffPos = offset;
			uint copyLen = 0;
			
			copyLen = sizeof(int);
			SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes(count), 0, copyLen);
			buffPos += copyLen;
			
			return buffPos - offset;
		}
		
		public uint Unserialize(byte[] buff, uint offset, uint buffLen)
		{
			CreateInstance();
			uint buffPos = offset;
			uint copyLen = 0;
			
			byte[] tempBytes = new byte[8];
			copyLen = sizeof(int);
			SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);
			count = BitConverter.ToInt32(tempBytes, 0);
			buffPos += copyLen;
			
			return buffPos;
		}
		
		public uint CalcDataLen()
		{
			uint dataLen = 0;
			dataLen += sizeof(int);
			return dataLen;
		}
	}
}