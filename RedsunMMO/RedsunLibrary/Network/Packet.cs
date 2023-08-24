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

		internal class PacketHeader : IDisposable
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
		private byte[] _dataBuffer = new byte[PacketConst.MAX_PACKET_SIZE];

		private int TotalPacketSize => PacketConst.PACKET_HEADER_SIZE + _bodySize;
		private int _bodySize;

		public bool SetBody(byte[] in_body, int in_offset, int in_size)
		{
			// 로직 체크는 다음에

			Buffer.BlockCopy(in_body, in_offset, _dataBuffer, PacketConst.PACKET_BODY_OFFSET, in_size);
			_bodySize = in_size;
			_packetHeader.PacketTotalSize = (Int16)TotalPacketSize;
			return true;
		}

		public void _MakeHeaderCheckSum()
		{
			_dataBuffer[PacketConst.PACKET_HEADER_CHECKSUM_OFFSET] = 0;
			for (int i = 0; i < PacketConst.PACKET_HEADER_SIZE; i++)
			{
				if (i == PacketConst.PACKET_HEADER_CHECKSUM_OFFSET)
				{
					continue;
				}
				_dataBuffer[PacketConst.PACKET_HEADER_CHECKSUM_OFFSET] ^= _dataBuffer[i];
			}
		}

		public bool _IsValidHeader()
		{
			int packetTotalSize = _packetHeader.PacketTotalSize;

			if (packetTotalSize < PacketConst.PACKET_HEADER_SIZE
				|| packetTotalSize > PacketConst.MAX_PACKET_SIZE)
			{
				return false;
			}

			byte tmpCheckSum = _dataBuffer[PacketConst.PACKET_HEADER_CHECKSUM_OFFSET];
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

		public void _MakeBodyCheckSum()
		{
			_dataBuffer[PacketConst.PACKET_BODY_CHECKSUM_OFFSET] = 0;
			for (int i = PacketConst.PACKET_BODY_OFFSET; i < _bodySize; i++)
			{
				_dataBuffer[PacketConst.PACKET_BODY_CHECKSUM_OFFSET] ^= _dataBuffer[i];
			}
		}

		public bool _IsValidBody()
		{
			byte tmpCheckSum = _dataBuffer[PacketConst.PACKET_BODY_CHECKSUM_OFFSET];
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
		public bool PacketToByteArrayy(ref byte[] ref_buffer, int in_startOffset)
		{
			// 원본 사이즈를 넣는다. 
			_packetHeader.PacketOriginalBodySize = (Int16)_bodySize;

			var compress = LZ4Compress.LZ4CodecEncode(_dataBuffer, PacketConst.PACKET_BODY_OFFSET, _bodySize);
			if (false == SetBody(compress, 0, compress.Length))
			{
				return false;
			}

			Console.WriteLine($"OriginalSize: {_packetHeader.PacketOriginalBodySize} | CompressSize: {compress.Length}");

			if (in_startOffset + TotalPacketSize > ref_buffer.Length)
			{
				return false;
			}

			// BodyCheckSum 부터
			_MakeBodyCheckSum();
			_MakeHeaderCheckSum();
			Buffer.BlockCopy(_dataBuffer, 0, ref_buffer, in_startOffset, TotalPacketSize);
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

			Console.WriteLine($"OriginalSize: {_packetHeader.PacketOriginalBodySize} | CompressSize: {compress.Length}");

			// BodyCheckSum 부터
			_MakeBodyCheckSum();
			_MakeHeaderCheckSum();

			byte[] bytes = new byte[TotalPacketSize];
			Buffer.BlockCopy(_dataBuffer, 0, bytes, 0, TotalPacketSize);
			return bytes;
		}

		public bool ByteArrayToPacket(byte[] in_buffer, int in_offset, int in_size)
		{
			Buffer.BlockCopy(in_buffer, in_offset, _dataBuffer, 0, in_size);

			int currentBodySize = in_size - PacketConst.PACKET_HEADER_SIZE;
			int originalBodySize = _packetHeader.PacketOriginalBodySize;
			var compress = LZ4Compress.LZ4CodecDecode(_dataBuffer, PacketConst.PACKET_BODY_OFFSET, currentBodySize, originalBodySize);
			if (false == SetBody(compress, 0, compress.Length))
			{
				return false;
			}

			if (false == _IsValidHeader())
			{
				return false;
			}

			if (false == _IsValidBody())
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
