using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace FlowInLib
{
    public class TcpSession
    {
        private Socket _socket = null;
        private bool _bEncode = true;
        private byte _encodeMask = 0x0f;
        private byte _decodeMask = 0x0f;
        private List<DataBuff> _sendBuffList = new List<DataBuff>();
        private List<DataBuff> _recvBuffList = new List<DataBuff>();
        private object _sendLocker = new object();
        private object _recvLocker = new object();
        private FileDataBuff _sendFileBuff = new FileDataBuff();
        private FileDataBuff _recvFileBuff = new FileDataBuff();
        private bool _bSending = false;
        private bool _bRecving = false;

        protected delegate bool RpcCallback(byte[] buff, uint offset, uint buffLen);
        protected RpcCallback[] _rpcCallbackArray = null;
        protected uint _rpcCallbackNum;

        public bool IsConnected()
        {
            return _socket != null && _socket.Connected;
        }

        public void Start(string ip, int port)
        {
            Stop();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (_socket == null)
            {
                LogManager.Debug("TcpSession::Start => fail to create socket");
                return;
            }

            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _socket.BeginConnect(endPoint, OnConnectCallback, this);
            }
            catch
            {
                _socket = null;
                LogManager.Debug("TcpSession::Start => fail to connect");
            }
        }

        public void Stop()
        {
            if (IsConnected())
            {
                bool bSocketClosed = false;
                try { _socket.Shutdown(SocketShutdown.Both); }
                catch (ObjectDisposedException e)
                {
                    bSocketClosed = true;
                    LogManager.Debug(string.Format("TcpSession::Stop => socket disposed (ok) [{0}]", e.Message));
                }
                catch (SocketException e) { LogManager.Debug(string.Format("TcpSession::Stop => error [{0}] [{1}]", e.ErrorCode, e.Message)); }
                catch { LogManager.Debug("TcpSession::Stop => unknown error"); }

                if (!bSocketClosed)
                {
                    try { _socket.Close(); }
                    catch { LogManager.Debug("TcpSession::Stop => close socket error"); }
                }
            }

            _socket = null;
        }

        public void Tick()
        {
            FileDataBuff buff = PopRecvData();
            while (buff != null)
            {
                if (!Invoke(buff))
                {
                    LogManager.Debug("TcpSession::Tick => invoke error");
                    Stop();
                    return;
                }
                buff.Clear();
                buff = PopRecvData();
            }
        }

        public void PushSendData(ushort msgType, ISerializableObj data)
        {
            if (!IsConnected())
                return;

            uint dataLen = data == null ? 0 : data.CalcDataLen();
            uint needLen = dataLen + 4;
            uint copyLen = 0;
            DataBuff buff = null;
            List<DataBuff> sendBuffList = new List<DataBuff>();

            if (needLen > FileDataBuff.MAX_FILE_SIZE)
                return;

            if (needLen > DataBuff.MAX_BUFF_LEN)
            {
                buff = ObjectPool<DataBuff>.Pop();
                if (buff == null) return;

                sendBuffList.Add(buff);

                copyLen = 2;
                SerializeUtil.LEMemcpy(buff._data, buff.GetWritePos(), BitConverter.GetBytes(msgType), 0, copyLen);
                if (_bEncode) Encode(buff._data, buff.GetWritePos(), copyLen);
                buff._dataLen += copyLen;

                copyLen = 2;
                SerializeUtil.LEMemcpy(buff._data, buff.GetWritePos(), BitConverter.GetBytes((ushort)dataLen), 0, copyLen);
                if (_bEncode) Encode(buff._data, buff.GetWritePos(), copyLen);
                buff._dataLen += copyLen;

                if (data != null)
                {
                    _sendFileBuff.Clear();
                    data.Serialize(_sendFileBuff._data, _sendFileBuff.GetWritePos(), FileDataBuff.MAX_FILE_SIZE);

                    while (_sendFileBuff.GetUnreadLen() > 0)
                    {
                        if (buff.GetUnwriteLen() <= 0)
                        {
                            buff = ObjectPool<DataBuff>.Pop();
                            if (buff == null) return;
                            _sendBuffList.Add(buff);
                        }

                        copyLen = Math.Min(buff.GetUnwriteLen(), _sendFileBuff.GetUnreadLen());
                        SerializeUtil.Memcpy(buff._data, buff.GetWritePos(), _sendFileBuff._data, _sendFileBuff.GetReadPos(), copyLen);
                        if (_bEncode) Encode(buff._data, buff.GetWritePos(), copyLen);
                        buff._dataLen += copyLen;
                        _sendFileBuff._readPos += copyLen;
                    }
                }
            }
            else
            {
                lock (_sendLocker)
                {
                    if (_sendBuffList.Count > 0)
                    {
                        int tailIndex = _sendBuffList.Count - 1;
                        sendBuffList.Add(_sendBuffList[tailIndex]);
                        _sendBuffList.RemoveAt(tailIndex);
                    }
                }

                bool needNew = sendBuffList.Count <= 0 || (DataBuff.MAX_BUFF_LEN - sendBuffList[sendBuffList.Count - 1]._dataLen < needLen);
                if (needNew)
                    buff = ObjectPool<DataBuff>.Pop();
                else
                    buff = sendBuffList[sendBuffList.Count - 1];

                if (buff != null)
                {
                    sendBuffList.Add(buff);

                    copyLen = 2;
                    SerializeUtil.LEMemcpy(buff._data, buff.GetWritePos(), BitConverter.GetBytes(msgType), 0, copyLen);
                    if (_bEncode) Encode(buff._data, buff.GetWritePos(), copyLen);
                    buff._dataLen += copyLen;

                    copyLen = 2;
                    SerializeUtil.LEMemcpy(buff._data, buff.GetWritePos(), BitConverter.GetBytes((ushort)dataLen), 0, copyLen);
                    if (_bEncode) Encode(buff._data, buff.GetWritePos(), copyLen);
                    buff._dataLen += copyLen;

                    if (data != null)
                    {
                        copyLen = data.Serialize(buff._data, buff.GetWritePos(), DataBuff.MAX_BUFF_LEN - buff._dataLen);
                        if (_bEncode) Encode(buff._data, buff.GetWritePos(), copyLen);
                        buff._dataLen += copyLen;
                    }
                }
            }

            if (sendBuffList.Count > 0)
            {
                lock (_sendLocker)
                {
                    _sendBuffList.AddRange(sendBuffList);
                    sendBuffList.Clear();
                }
            }

            if (!IsConnected())
                ClearSendBuffList();
            else
                SendBytes();
        }

        public FileDataBuff PopRecvData()
        {
            if (!IsConnected())
                return null;

            List<DataBuff> recvBuffList = new List<DataBuff>();
            lock (_recvLocker)
            {
                List<DataBuff> tempList = _recvBuffList;
                _recvBuffList = recvBuffList;
                recvBuffList = tempList;
            }

            uint copyLen = 0;
            DataBuff buff = null;

            while (_recvFileBuff._dataLen < 4)
            {
                if (recvBuffList.Count <= 0)
                    return null;

                buff = recvBuffList[0];

                copyLen = Math.Min(4 - _recvFileBuff._dataLen, buff.GetUnreadLen());
                SerializeUtil.Memcpy(_recvFileBuff._data, _recvFileBuff.GetWritePos(), buff._data, buff.GetReadPos(), copyLen);
                buff._readPos += copyLen;
                _recvFileBuff._dataLen += copyLen;

                if (buff.GetUnreadLen() <= 0)
                {
                    buff.Clear();
                    ObjectPool<DataBuff>.Push(buff);
                    recvBuffList.RemoveAt(0);
                }
            }

            byte[] ushortBytes = new byte[2];
            SerializeUtil.LEMemcpy(ushortBytes, 0, _recvFileBuff._data, 2, 2);
            uint msgLen = BitConverter.ToUInt16(ushortBytes, 0);
            if (msgLen + 4 > FileDataBuff.MAX_FILE_SIZE)
                return null;

            uint needLen = msgLen - (_recvFileBuff._dataLen - 4);
            while (needLen > 0)
            {
                if (recvBuffList.Count <= 0)
                    return null;

                buff = recvBuffList[0];

                copyLen = Math.Min(needLen, buff.GetUnreadLen());
                SerializeUtil.Memcpy(_recvFileBuff._data, _recvFileBuff.GetWritePos(), buff._data, buff.GetReadPos(), copyLen);
                buff._readPos += copyLen;
                _recvFileBuff._dataLen += copyLen;

                if (buff.GetUnreadLen() <= 0)
                {
                    buff.Clear();
                    ObjectPool<DataBuff>.Push(buff);
                    recvBuffList.RemoveAt(0);
                }

                needLen -= copyLen;
            }

            if (recvBuffList.Count > 0)
            {
                lock (_recvLocker)
                {
                    recvBuffList.AddRange(_recvBuffList);
                    _recvBuffList.Clear();
                    _recvBuffList = recvBuffList;
                }
            }

            if (!IsConnected())
                ClearRecvBuffList();

            return _recvFileBuff;
        }

        protected bool Invoke(FileDataBuff buff)
        {
            if (buff == null || _rpcCallbackArray == null)
                return false;

            if (buff._dataLen < 4)
                return false;

            uint readLen = 0;
            uint msgType = 0;
            uint msgLen = 0;
            byte[] ushortBytes = new byte[2];

            readLen = 2;
            SerializeUtil.LEMemcpy(ushortBytes, 0, buff._data, buff.GetReadPos(), readLen);
            buff._readPos += readLen;
            msgType = BitConverter.ToUInt16(ushortBytes, 0);

            if (msgType >= _rpcCallbackNum)
                return false;

            readLen = 2;
            SerializeUtil.LEMemcpy(ushortBytes, 0, buff._data, buff.GetReadPos(), readLen);
            buff._readPos += readLen;
            msgLen = BitConverter.ToUInt16(ushortBytes, 0);

            if (buff.GetUnreadLen() != msgLen)
                return false;

            return _rpcCallbackArray[msgType](buff._data, buff.GetReadPos(), msgLen);
        }

        protected void OnConnectCallback(IAsyncResult res)
        {
            TcpSession client = res.AsyncState as TcpSession;
            try
            {
                client._socket.EndConnect(res);
                RecvBytes();
            }
            catch (SocketException e) { LogManager.Debug(string.Format("TcpSession::OnConnectCallback => error [{0}] [{1}]", e.ErrorCode, e.Message)); }
            catch { LogManager.Debug("TcpSession::OnConnectCallback => unknown error"); }
        }

        protected void SendBytes()
        {
            if (!IsConnected())
            {
                LogManager.Debug("TcpSession::SendBytes => not connected");
                return;
            }

            if (_bSending)
                return;

            DataBuff buff = null;
            lock (_sendLocker)
            {
                if (_sendBuffList.Count <= 0)
                    return;

                buff = _sendBuffList[0];
                _sendBuffList.RemoveAt(0);
            }

            if (buff == null)
                return;

            try
            {
                _socket.BeginSend(buff._data, 0, (int)buff._dataLen, SocketFlags.None, OnSendBytesCallback, buff);
                _bSending = true;
            }
            catch (SocketException e) { LogManager.Debug(string.Format("TcpSession::SendBytes => error [{0}] [{1}]", e.ErrorCode, e.Message)); }
            catch { LogManager.Debug("TcpSession::SendBytes => unknown error"); }
        }

        protected void OnSendBytesCallback(IAsyncResult res)
        {
            try
            {
                int len = _socket.EndSend(res);
                _bSending = false;

                DataBuff buff = res.AsyncState as DataBuff;
                if (buff == null || len != buff._dataLen)
                {
                    LogManager.Debug("TcpSession::OnSendBytesCallback => send error");
                    Stop();
                    return;
                }

                buff.Clear();
                ObjectPool<DataBuff>.Push(buff);
                SendBytes();
            }
            catch (SocketException e) { LogManager.Debug(string.Format("TcpSession::OnSendBytesCallback => error [{0}] [{1}]", e.ErrorCode, e.Message)); }
            catch { LogManager.Debug("TcpSession::OnSendBytesCallback => unknown error"); }
        }

        protected void RecvBytes()
        {
            if (!IsConnected())
            {
                LogManager.Debug("TcpSession::SendBytes => not connected");
                return;
            }

            if (_bRecving)
                return;

            try
            {
                DataBuff buff = null;
                lock (_recvLocker)
                {
                    if (_recvBuffList.Count > 0)
                    {
                        int index = _recvBuffList.Count - 1;
                        if (_recvBuffList[index] != null && _recvBuffList[index].GetUnwriteLen() > 0)
                        {
                            buff = _recvBuffList[index];
                            _recvBuffList.RemoveAt(index);
                        }
                    }
                }

                if (buff == null)
                    buff = ObjectPool<DataBuff>.Pop();

                _socket.BeginReceive(buff._data, 0, (int)buff.GetUnwriteLen(), SocketFlags.None, OnRecvBytesCallback, buff);
                _bRecving = true;
            }
            catch (SocketException e) { LogManager.Debug(string.Format("TcpSession::RecvBytes => error [{0}] [{1}]", e.ErrorCode, e.Message)); }
            catch { LogManager.Debug("TcpSession::RecvBytes => unknown error"); }
        }

        protected void OnRecvBytesCallback(IAsyncResult res)
        {
            try
            {
                int len = _socket.EndReceive(res);
                _bRecving = false;

                if (len <= 0)
                {
                    LogManager.Debug("TcpSession::OnRecvBytesCallback => disconnected by remote");
                    Stop();
                    return;
                }

                DataBuff buff = res.AsyncState as DataBuff;
                if (buff == null)
                {
                    LogManager.Debug("TcpSession::OnRecvBytesCallback => invalid recv buffer");
                    return;
                }

                buff._dataLen += (uint)len;
                lock (_recvLocker)
                {
                    _recvBuffList.Add(buff);
                }

                RecvBytes();
            }
            catch (SocketException e) { LogManager.Debug(string.Format("TcpSession::OnRecvBytesCallback => error [{0}] [{1}]", e.ErrorCode, e.Message)); }
            catch { LogManager.Debug("TcpSession::OnRecvBytesCallback => unknown error"); }
        }

        protected void ClearSendBuffList()
        {
            lock (_sendLocker)
            {
                for (int i = 0; i < _sendBuffList.Count; ++i)
                {
                    _sendBuffList[i].Clear();
                    ObjectPool<DataBuff>.Push(_sendBuffList[i]);
                }
                _sendBuffList.Clear();
            }
        }

        protected void ClearRecvBuffList()
        {
            lock (_recvLocker)
            {
                for (int i = 0; i < _recvBuffList.Count; ++i)
                {
                    _recvBuffList[i].Clear();
                    ObjectPool<DataBuff>.Push(_recvBuffList[i]);
                }
                _recvBuffList.Clear();
            }
        }

        protected void Encode(byte[] buff, uint offset, uint len)
        {
            byte temp;
            for (uint i = 0; i < len; ++i)
            {
                uint index = i + offset;
                temp = (byte)((buff[index] << 4) | (_encodeMask >> 4));
                buff[index] ^= _encodeMask;
                _encodeMask = temp;
            }
        }

        protected void Decode(byte[] buff, uint offset, uint len)
        {
            for (uint i = 0; i < len; ++i)
            {
                uint index = i + offset;
                buff[index] ^= _decodeMask;
                _decodeMask = (byte)((buff[i] << 4) | (_decodeMask >> 4));
            }
        }
    }
}