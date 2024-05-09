using Google.Protobuf;
using RedsunLibrary.Network;
using RedsunLibrary.Network.TCP;
using RedsunLibrary.Utils;

namespace RedsunServer.ServerUtils
{
	public class MessageProcessor<REQ_MSG, ACK_MSG>
		where REQ_MSG : Google.Protobuf.IMessage, new()
		where ACK_MSG : Google.Protobuf.IMessage, new()
	{
		protected ServerResult _result;
		protected TCPSession _session;
		protected Packet _packet;

		protected Int32 _reqPacketId;
		protected Int32 _ackPacketId;

		protected REQ_MSG _reqMsg;
		protected ACK_MSG _ackMsg;

		public REQ_MSG ReqMsg => _reqMsg;
		public ACK_MSG AckMsg => _ackMsg;

		public MessageProcessor(PacketMessage packet, Int32 reqPacketId, Int32 ackPacketId)
		{
			_result = new ServerResult();
			_session = (TCPSession)packet.Session;
			_packet = packet.Packet;

			_reqPacketId = reqPacketId;
			_ackPacketId = ackPacketId;

			_reqMsg = new REQ_MSG();
			_ackMsg = new ACK_MSG();
		}

		public void Initialize()
		{
			var data = _packet.PacketToByteArray();
			_reqMsg.MergeFrom(data, 0, data.Length);
		}

		public void HandleProcess()
		{
			onSafeProcess();
		}

		private void onSafeProcess()
		{
			try
			{
				Initialize();

				_result = onHandlePrepare();
				if (_result.IsFail())
				{
					Console.WriteLine(_result.ToString());
					return;
				}

				_result = onHandleProcess();
				if (_result.IsFail())
				{
					Console.WriteLine(_result.ToString());
					return;
				}
			}
			catch (Exception ex)
			{
				_result = _result.setException(ex.ToString());
				Console.WriteLine(_result.ToString());
			}
			finally
			{
				onHandleCleanup(_result);
			}
		}

		private ServerResult onHandlePrepare() => onPrepare();
		public virtual ServerResult onPrepare() => ServerResult.alloc().setOk();

		private ServerResult onHandleProcess() => onProcess();
		public virtual ServerResult onProcess() => ServerResult.alloc().setOk();

		private void onHandleCleanup(ServerResult result) => onCleanup(result);
		public virtual void onCleanup(ServerResult result) { }


		public Packet MakePacket()
		{
			Packet packet;
			if (_result.IsFail())
			{
				packet = new Packet((int)EPacketProtocol.ScErrorAck);
				var error = new SCErrorAck()
				{
					SourceProtocol = (EPacketProtocol)_reqPacketId,
					ErrorCode = _result.ResultCode,
				};

				packet.SetBody(error.ToByteArray());
			}
			else
			{
				packet = new Packet(_ackPacketId);
				packet.SetBody(AckMsg.ToByteArray());
			}
			return packet;
		}

		public void Send()
		{
			_session.SendAsync(MakePacket());
		}
	}
}
