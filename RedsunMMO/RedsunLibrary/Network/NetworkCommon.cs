using System;

namespace RedsunLibrary.Network
{
	public static class PacketConst
	{
		public const Int32 MAX_PACKET_SIZE = 1460;  // TCP MTU : 1500 - TCP header
		public const Int32 MAX_PACKET_BODY_SIZE = MAX_PACKET_SIZE - PACKET_HEADER_SIZE;

		public const Int32 TCP_SEND_BUFFER_SIZE = MAX_PACKET_SIZE * 5;      // 7.3KB (7300 Bytes)
		public const Int32 TCP_RECV_BUFFER_SIZE = MAX_PACKET_SIZE * 5;      // 7.3KB (7300 Bytes)
		public const Int32 MAX_PACKET_BUFFER_SIZE = MAX_PACKET_SIZE * 20;   // 73KB (73000 Bytes)


		// PACKET HEADER OFFSET
		public const Int32 PACKET_TOTAL_SIZE_OFFSET = 0;                                                                        // Offset: 0 / [0,1]
		public const Int32 PACKET_TOTAL_SIZE = sizeof(Int16);                                                                   // Size: 2 bytes

		public const Int32 PACKET_PROTOCOL_TYPE_OFFSET = PACKET_TOTAL_SIZE_OFFSET + PACKET_TOTAL_SIZE;                          // Offset: 2 / [2,3,4,5]
		public const Int32 PACKET_PROTOCOL_TYPE = sizeof(Int32);                                                                // Size: 4 bytes

		public const Int32 PACKET_SEQUENCE_OFFSET = PACKET_PROTOCOL_TYPE_OFFSET + PACKET_PROTOCOL_TYPE;                         // Offset: 6 / [6,7]
		public const Int32 PACKET_SEQUENCE = sizeof(Int16);                                                                     // Size: 2 bytes

		public const Int32 PACKET_HEADER_CHECKSUM_OFFSET = PACKET_SEQUENCE_OFFSET + PACKET_SEQUENCE;                            // Offset: 8 / [8]
		public const Int32 PACKET_HEADER_CHECKSUM = sizeof(Byte);                                                               // Size: 1 bytes

		public const Int32 PACKET_ORIGINAL_BODY_SIZE_OFFSET = PACKET_HEADER_CHECKSUM_OFFSET + PACKET_HEADER_CHECKSUM;           // Offset: 9 / [9,10]
		public const Int32 PACKET_ORIGINAL_BODY_SIZE = sizeof(Int16);                                                           // Size: 2 bytes

		public const Int32 PACKET_BODY_CHECKSUM_OFFSET = PACKET_ORIGINAL_BODY_SIZE_OFFSET + PACKET_ORIGINAL_BODY_SIZE;          // Offset: 11 / [11]
		public const Int32 PACKET_BODY_CHECKSUM = sizeof(Byte);                                                                 // Size: 1 bytes


		// PACKET HEADER SIZE: 12 Bytes
		public const Int32 PACKET_HEADER_SIZE = PACKET_TOTAL_SIZE + PACKET_PROTOCOL_TYPE + PACKET_SEQUENCE
											+ PACKET_HEADER_CHECKSUM + PACKET_ORIGINAL_BODY_SIZE + PACKET_BODY_CHECKSUM;

		// PACKET BODY OFFSET
		public const Int32 PACKET_BODY_OFFSET = PACKET_BODY_CHECKSUM + PACKET_BODY_CHECKSUM_OFFSET;     // Offset: 12 / [12~x]
	}

	public enum SocketState_e
	{
		_BEGIN,
		NOT_CONNECTED,
		CONNECTING,
		CONNECTED,
		DISCONNECTING,
		DISCONNECTED,
		_END
	}
	public enum SocketAcceptState_e
	{
		_BEGIN,
		NOT_BIND,
		BIND,
		NOT_LISTEN,
		LISTEN,
		_END
	}

	public enum SocketSendState_e
	{
		_BEGIN,
		NOT_SENDING,
		SENDING,
		_END
	}

	public enum SocketRecvState_e
	{
		_BEGIN,
		NOT_RECEIVING,
		RECEIVING,
		_END
	}

	public enum DisposeState_e
	{
		_BEGIN,
		NOT_DISPOSED,
		DISPOSED,
		_END
	}
}

