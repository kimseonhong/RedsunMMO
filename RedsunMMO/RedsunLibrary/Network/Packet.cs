using RedsunLibrary.Utils;
using System;

namespace RedsunLibrary.Network
{
	public class Packet : IDisposable
	{
		/*
		 * [ PACKET HEADER ]
		 * [ NO Encrypt | NO Compression ]
		 * Packet Total Size			(2/Int16)
		 * Packet Protocol type			(4/Int32)
		 * Packet Sequence				(2/Int16)
		 * Packet Header Checksum		(1/Bytes)
		 * Packet Original Body Size	(2/Int16)
		 * Packet Body Checksum			(1/Bytes)
		 * ------------------------------- Total: 12 Bytes
		 * [ Packet BODY ]
		 * [ DO Encrypt | DO Compression ]
		 * ERROR TYPE	(2/Int16)
		 * ERROR CODE	(4/Int32)
		 * ------------------------------- Total: 18 Bytes
		 * DATA (1460 - 18 Bytes)
		 * -------------------------------
		 */

		public class PacketHeader : IDisposable
		{
			private Packet _packet;
			private byte[] PacketBuffer => _packet._dataBuffer;

			public Int16 PacketTotalSize
			{
				get { return NetworkBitConverter.ToInt16(PacketBuffer, PacketConst.PACKET_TOTAL_SIZE_OFFSET); }
				set { Buffer.BlockCopy(NetworkBitConverter.GetBytes(value), 0, PacketBuffer, PacketConst.PACKET_TOTAL_SIZE_OFFSET, PacketConst.PACKET_TOTAL_SIZE); }
			}

			public Int32 PacketProtocolType
			{
				get { return NetworkBitConverter.ToInt32(PacketBuffer, PacketConst.PACKET_PROTOCOL_TYPE_OFFSET); }
				set { Buffer.BlockCopy(NetworkBitConverter.GetBytes(value), 0, PacketBuffer, PacketConst.PACKET_PROTOCOL_TYPE_OFFSET, PacketConst.PACKET_PROTOCOL_TYPE); }
			}

			public Int16 PacketSequence
			{
				get { return NetworkBitConverter.ToInt16(PacketBuffer, PacketConst.PACKET_SEQUENCE_OFFSET); }
				set { Buffer.BlockCopy(NetworkBitConverter.GetBytes(value), 0, PacketBuffer, PacketConst.PACKET_SEQUENCE_OFFSET, PacketConst.PACKET_SEQUENCE); }
			}

			public Byte PacketHeaderChecksum
			{
				get { return PacketBuffer[PacketConst.PACKET_HEADER_CHECKSUM_OFFSET]; }
				set { PacketBuffer[PacketConst.PACKET_HEADER_CHECKSUM_OFFSET] = value; }
			}

			public Int16 PacketOriginalBodySize
			{
				get { return NetworkBitConverter.ToInt16(PacketBuffer, PacketConst.PACKET_ORIGINAL_BODY_SIZE_OFFSET); }
				set { Buffer.BlockCopy(NetworkBitConverter.GetBytes(value), 0, PacketBuffer, PacketConst.PACKET_ORIGINAL_BODY_SIZE_OFFSET, PacketConst.PACKET_ORIGINAL_BODY_SIZE); }
			}

			public Byte PacketBodyChecksum
			{
				get { return PacketBuffer[PacketConst.PACKET_BODY_CHECKSUM_OFFSET]; }
				set { PacketBuffer[PacketConst.PACKET_BODY_CHECKSUM_OFFSET] = value; }
			}

			public PacketHeader(Packet packet)
			{
				_packet = packet;
			}

			public void Dispose()
			{
				_packet = null;
			}
		}

		private PacketHeader _packetHeader;
		private byte[] _dataBuffer;

		private int TotalPacketSize => PacketConst.PACKET_HEADER_SIZE + _bodySize;
		private int _bodySize;

		public Packet()
		{
			_dataBuffer = new byte[PacketConst.MAX_PACKET_SIZE];

			_packetHeader = new PacketHeader(this);
		}

		public Packet(int packetId)
		{
			_dataBuffer = new byte[PacketConst.MAX_PACKET_SIZE];

			_packetHeader = new PacketHeader(this);
			_packetHeader.PacketProtocolType = packetId;
		}

		public Packet(int packetId, byte[] body)
		{
			_dataBuffer = new byte[PacketConst.MAX_PACKET_SIZE];

			_packetHeader = new PacketHeader(this);
			_packetHeader.PacketProtocolType = packetId;
			SetBody(body, 0, body.Length);
		}

		public Packet(int packetId, byte[] body, int offset, int size)
		{
			_dataBuffer = new byte[PacketConst.MAX_PACKET_SIZE];

			_packetHeader = new PacketHeader(this);
			_packetHeader.PacketProtocolType = packetId;
			SetBody(body, offset, size);
		}

		public Packet(byte[] data)
		{
			_dataBuffer = data;

			_packetHeader = new PacketHeader(this);
		}

		public int GetPacketId()
		{
			return _packetHeader.PacketProtocolType;
		}

		public bool SetBody(byte[] body)
			=> SetBody(body, 0, body.Length);

		public bool SetBody(byte[] body, int offset, int size)
		{
			// SetBody 할때 모든걸 세팅할까.. 아니면 진짜 Body 넣을까...
			// 로직 체크는 다음에

			Buffer.BlockCopy(body, offset, _dataBuffer, PacketConst.PACKET_BODY_OFFSET, size);
			_bodySize = size;
			_packetHeader.PacketTotalSize = (Int16)TotalPacketSize;
			return true;
		}

		private void _MakeHeaderCheckSum()
		{
			_packetHeader.PacketHeaderChecksum = 0;
			for (int i = 0; i < PacketConst.PACKET_HEADER_SIZE; i++)
			{
				if (i == PacketConst.PACKET_HEADER_CHECKSUM_OFFSET)
				{
					continue;
				}
				_packetHeader.PacketHeaderChecksum ^= _dataBuffer[i];
			}
		}

		public bool IsValidHeader()
		{
			int packetTotalSize = _packetHeader.PacketTotalSize;

			if (packetTotalSize < PacketConst.PACKET_HEADER_SIZE
				|| packetTotalSize > PacketConst.MAX_PACKET_SIZE)
			{
				return false;
			}

			byte tmpCheckSum = _packetHeader.PacketHeaderChecksum;
			for (int i = 0; i < PacketConst.PACKET_HEADER_SIZE; i++)
			{
				if (i == PacketConst.PACKET_HEADER_CHECKSUM_OFFSET)
				{
					continue;
				}
				tmpCheckSum ^= _dataBuffer[i];
			}

			if (0 != tmpCheckSum)
			{
				return false;
			}
			return true;
		}

		private void _MakeBodyCheckSum()
		{
			_packetHeader.PacketBodyChecksum = 0;
			for (int i = PacketConst.PACKET_BODY_OFFSET; i < _bodySize; i++)
			{
				_packetHeader.PacketBodyChecksum ^= _dataBuffer[i];
			}
		}

		public bool IsValidBody()
		{
			byte tmpCheckSum = _packetHeader.PacketBodyChecksum;
			for (int i = PacketConst.PACKET_BODY_OFFSET; i < _bodySize; i++)
			{
				tmpCheckSum ^= _dataBuffer[i];
			}

			if (0 != tmpCheckSum)
			{
				return false;
			}
			return true;
		}

		// Packet 데이터 -> byte[] 로 복사
		public bool PacketToByteArrayy(ref byte[] ref_buffer, int startOffset)
		{
			// 원본 사이즈를 넣는다. 
			_packetHeader.PacketOriginalBodySize = (Int16)_bodySize;

			var compress = LZ4Compress.LZ4CodecEncode(_dataBuffer, PacketConst.PACKET_BODY_OFFSET, _bodySize);
			if (false == SetBody(compress, 0, compress.Length))
			{
				return false;
			}

			//Console.WriteLine($"OriginalSize: {_packetHeader.PacketOriginalBodySize} | CompressSize: {compress.Length}");

			if (startOffset + TotalPacketSize > ref_buffer.Length)
			{
				return false;
			}

			// BodyCheckSum 부터
			_MakeBodyCheckSum();
			_MakeHeaderCheckSum();
			Buffer.BlockCopy(_dataBuffer, 0, ref_buffer, startOffset, TotalPacketSize);
			return true;
		}

		public byte[] PacketToByteArray()
		{
			// 원본 사이즈를 넣는다. 
			_packetHeader.PacketOriginalBodySize = (Int16)_bodySize;

			var compress = LZ4Compress.LZ4CodecEncode(_dataBuffer, PacketConst.PACKET_BODY_OFFSET, _bodySize);
			if (false == SetBody(compress, 0, compress.Length))
			{
				return null;
			}

			//Console.WriteLine($"OriginalSize: {_packetHeader.PacketOriginalBodySize} | CompressSize: {compress.Length}");

			// BodyCheckSum 부터
			_MakeBodyCheckSum();
			_MakeHeaderCheckSum();

			byte[] bytes = new byte[TotalPacketSize];
			Buffer.BlockCopy(_dataBuffer, 0, bytes, 0, TotalPacketSize);
			return bytes;
		}

		public bool ByteArrayToPacket(byte[] buffer, int offset, int size)
		{
			// 복사하려는 패킷의 사이즈가 패킷헤더 최소보다 작다면 아직 데이터가 없음
			if (size < PacketConst.PACKET_HEADER_SIZE)
			{
				return false;
			}

			// size 는 토탈 패킷사이즈임, 복사되는 양이랑 이런것들 앞단에서 체크함
			Buffer.BlockCopy(buffer, offset, _dataBuffer, 0, size);

			// 일단 헤더부터 체크
			if (false == IsValidHeader())
			{
				return false;
			}

			int currentBodySize = size - PacketConst.PACKET_HEADER_SIZE;
			int originalBodySize = _packetHeader.PacketOriginalBodySize;

			_bodySize = currentBodySize;
			if (false == IsValidBody())
			{
				return false;
			}

			// 디코딩은 제일 마지막
			var compress = LZ4Compress.LZ4CodecDecode(_dataBuffer, PacketConst.PACKET_BODY_OFFSET, currentBodySize, originalBodySize);
			if (false == SetBody(compress, 0, compress.Length))
			{
				return false;
			}

			return true;
		}

		public byte[] ToBodyArray()
		{
			byte[] bytes = new byte[_bodySize];
			Buffer.BlockCopy(_dataBuffer, PacketConst.PACKET_BODY_OFFSET, bytes, 0, _bodySize);
			return bytes;
		}

		public void Dispose()
		{
			_packetHeader.Dispose();
			_packetHeader = null;
			_dataBuffer = null;
		}

	}
}
