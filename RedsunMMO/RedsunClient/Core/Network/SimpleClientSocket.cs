using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace RedsunClient.Core.Network
{
    // Need to Network Common & Packet structure

    internal class NetworkDefineTemp
    {
        public const int MAX_PACKET_BINARY_SIZE = 8000;
    }



    public class SimpleClientSocket
    {
        public Socket? ClientSocket => mSocket;

        public bool IsConnected => mSocket?.Connected ?? false;


        private Socket? mSocket = null;
        private bool mIsConnecting = false;

        private SocketAsyncEventArgs mRecvEvent = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs mSendEvent = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs mConnectEvent = new SocketAsyncEventArgs();

        private byte[] mRecvBuffer = new byte[NetworkDefineTemp.MAX_PACKET_BINARY_SIZE];
        private byte[] mSendBuffer = new byte[NetworkDefineTemp.MAX_PACKET_BINARY_SIZE];

        private object mSendLock = new object();
        private Queue<byte[]> mSendList = new Queue<byte[]>();


        public SimpleClientSocket()
        {
            _ResetBuffer();

            mRecvEvent.Completed += _RecvCompleted;
            mSendEvent.Completed += _SendCompleted;



            mConnectEvent.Completed += _ConnectCompleted;
        }


        public void Reset()
        {
            Disconnect();

        }

        public void Disconnect()
        {
            if (null == mSocket)
                return;

            if (!IsConnected)
                return;

            // Todo OnDisconnected()

            try
            {
                mSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {

            }
            finally
            {
                mSocket.Close();
                mSocket = null;
            }
            mIsConnecting = false;

            lock (mSendLock)
                mSendList.Clear();
        }

        public void Send(byte[] data)
        {
            // Todo : need to change byte[] to Packet

            lock (mSendLock)
            {
                bool bNeedToStart = mSendList.Count == 0;
                mSendList.Enqueue(data);
                if (bNeedToStart)
                    _StartSend();
            }
        }



        public bool Connect(string host, int port)
        {
            if (IsConnected)
                return false;   // 이미 접속됨

            if (mIsConnecting)
                return false;   // 이미 접속중

            if (port <= 0
                || port > ushort.MaxValue)
                return false;

            if (!IPAddress.TryParse(host, out var address))
            {
                try
                {
                    var ip = Dns.GetHostAddresses(host);
                    if (ip.Length == 0)
                        return false;

                    address = ip[0];
                }
                catch (Exception)
                {
                    return false;
                }
            }

            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mConnectEvent.RemoteEndPoint = new IPEndPoint(address, port);
            _ResetBuffer();

            mIsConnecting = true;
            try
            {
                if (!mSocket.ConnectAsync(mConnectEvent))
                    _ConnectCompleted(null, mConnectEvent);
            }
            catch (Exception)
            {
                Disconnect();
                return false;
            }

            return true;
        }


        private void _ResetBuffer()
        {
            mRecvEvent.SetBuffer(mRecvBuffer, 0, mRecvBuffer.Length);
            mSendEvent.SetBuffer(mSendBuffer, 0, mSendBuffer.Length);
        }




        //private TcpClient mSocket = new TcpClient();

        //public bool IsConnected => mSocket?.Connected ?? false;
        ////public bool IsConnecting => mSocket?.

        //public SimpleClientSocket()
        //{
        //    mSocket.NoDelay = true;
        //    mSocket.ReceiveBufferSize = 8192;   // need to Max Packet Binary Size
        //    mSocket.SendBufferSize = 8192;
        //    //mSocket.
        //}



        //private void _Run()
        //{

        //    while(IsConnected)
        //    {

        //    }


        //}


        private void _RecvProcess(SocketAsyncEventArgs e, int pendingCount = 0)
        {
            if (e.BytesTransferred == 0
                || e.SocketError != SocketError.Success)
            {
                Disconnect();
                return;
            }

            _OnReceive(e.Buffer, e.Offset, e.BytesTransferred);

            if (null == mSocket)
                return;

            try
            {
                if (false == mSocket.ReceiveAsync(e))
                    _RecvProcess(e, ++pendingCount);
            }
            catch (ObjectDisposedException)
            {
                // already socket closed
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void _SendProcess(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred <= 0
                || e.SocketError != SocketError.Success)
                return;

            lock (mSendLock)
            {
                if (mSendList.Count == 0)
                    return;
                if (e.BytesTransferred != mSendList.Peek().Length)
                    return;

                mSendList.Dequeue();
                if (mSendList.Count > 0)
                    _StartSend();

            }
        }

        private void _StartSend()
        {
            if (mSocket == null)
                return;

            if (!IsConnected)
                return;


            lock (mSendLock)
            {
                if (mSendList.Count == 0)
                    return;

                byte[] sendPacket = mSendList.Peek();
                mSendEvent.SetBuffer(mSendEvent.Offset, sendPacket.Length);
                if (null == mSendEvent.Buffer)
                    return; // 흠...?

                Buffer.BlockCopy(sendPacket, 0, mSendEvent.Buffer, mSendEvent.Offset, sendPacket.Length);
            }

            if (!mSocket.SendAsync(mSendEvent))
                _SendProcess(mSendEvent);
        }

        private void _OnReceive(byte[]? buffer, int offset, int bytesTransferred)
        {
            // Todo : Make Packet Message
        }






        #region SocketAsyncEvent

        private void _SendCompleted(object? sender, SocketAsyncEventArgs e)
        {
            _SendProcess(e);
        }

        private void _RecvCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
                _RecvProcess(e);
        }
        private void _ConnectCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                // Todo : OnConnectFailed
                Disconnect();
                return;
            }

            // Todo OnConnected
            mIsConnecting = false;  // 접속됨
            if (null != mSocket)
            {
                if (!mSocket.ReceiveAsync(mRecvEvent))
                    _RecvCompleted(null, mRecvEvent);
            }
        }

        #endregion
    }
}
