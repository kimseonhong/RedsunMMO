using RedsunLibrary.Utils;
using System;

namespace RedsunLibrary.Network
{
	public class PacketProcessor
	{
		private object _lockObj;
		private Packet _lastPacket;

		private byte[] _buffer;
		private Int32 _offset = 0;
		private Int32 _consumedOffset = 0;

		public PacketProcessor()
		{
			_lockObj = new object();

			_buffer = new byte[PacketConst.MAX_PACKET_BUFFER_SIZE];
			_offset = 0;
			_consumedOffset = 0;
		}
		public Int32 GetReceivedLength()
		{
			return _offset - _consumedOffset;
		}

		public void ReceiveProcess(byte[] buffer, Int32 recvCount)
		{
			lock (_lockObj)
			{
				try
				{
					if (_offset + recvCount >= PacketConst.MAX_PACKET_BUFFER_SIZE)
					{
						Int32 receivedLength = GetReceivedLength();
						Buffer.BlockCopy(_buffer, _consumedOffset, _buffer, 0, receivedLength);

						_offset = receivedLength;
						_consumedOffset = 0;
					}

					Buffer.BlockCopy(buffer, 0, _buffer, _offset, recvCount);
					_offset += recvCount;
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return;
				}
			}
		}

		public Packet TakePacket()
		{
			lock (_lockObj)
			{
				try
				{
					Int32 receivedLength = GetReceivedLength();

					// 현재까지 받은 패킷의 사이즈가 헤더 사이즈보다 작으면 아직 뭔가 부족한거야..
					// 헤더사이즈 까지 받아야 뭔가 할 수 있지
					if (receivedLength < PacketConst.PACKET_HEADER_SIZE)
					{
						return null;
					}

					// 패킷의 첫 2바이트는 무조건 패킷 토탈사이즈임
					var totalPacketSize = NetworkBitConverter.ToInt16(_buffer, _consumedOffset);
					// 패킷 토탈사이즈로 기록된게 0이하 (음수) 이거나, packet header size 보다 작다면 문제 ㅇ있는거임
					if (totalPacketSize < 0
						|| totalPacketSize < PacketConst.PACKET_HEADER_SIZE)
					{
						throw new Exception();
					}

					// 실제 패킷의 전체 사이즈보다 현재 기록된 received size 가 작다면 아직 덜 받은거
					if (totalPacketSize > receivedLength)
					{
						return null;
					}

					// 실제 패킷을 만들어보자고
					Packet packet = new Packet();
					if (false == packet.ByteArrayToPacket(_buffer, _consumedOffset, totalPacketSize))
					{
						return null;
					}
					_consumedOffset += totalPacketSize;
					return packet;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return null;
				}
			}
		}
	}
}
