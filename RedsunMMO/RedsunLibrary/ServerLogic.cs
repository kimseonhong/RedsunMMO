using RedsunLibrary.Network;
using RedsunLibrary.Network.Server;
using RedsunLibrary.Network.TCP;
using RedsunLibrary.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RedsunLibrary
{
	public abstract class ServerLogic
	{
		private const double DEFAULT_FPS = 60.0;
		public readonly double FPS = 60.0;
		public readonly double SECONDS_PER_FRAME = 1.0 / DEFAULT_FPS;

		private bool _isRunning = false;

		private int _packetFrameCount = 0;
		private int _frameCount = 0;
		private double _lastFPSCheckTime = 0;

		private ConcurrentQueue<PacketMessage> _packetQueue;
		private Thread _runningThread;

		private bool _useThreadSleep = false;

		public ServerLogic(double fps = DEFAULT_FPS, bool useThreadSleep = false)
		{
			FPS = fps;
			SECONDS_PER_FRAME = 1.0 / FPS;

			_useThreadSleep = useThreadSleep;

			_packetQueue = new ConcurrentQueue<PacketMessage>();
			_runningThread = new Thread(_Update)
			{
				IsBackground = true,
				Priority = ThreadPriority.AboveNormal,
			};
		}

		public void Start()
		{
			_isRunning = true;
			_runningThread.Start();
		}

		public void Stop()
		{
			_isRunning = false;
			_runningThread.Join();
			_runningThread.DisableComObjectEagerCleanup();

			_packetQueue = null;
		}

		//private void _Update()
		//{
		//	Stopwatch sw = new Stopwatch();
		//	sw.Start();
		//
		//	double lastUpdateTime = 0;
		//
		//	while (_isRunning)
		//	{
		//		double currentTime = sw.Elapsed.TotalSeconds;
		//		double wait = sw.Elapsed.TotalMilliseconds + SECONDS_PER_FRAME;
		//		if (_packetQueue.Count <= 0)
		//		{
		//			//Thread.Sleep(1);
		//			SpinWait.SpinUntil(() => sw.Elapsed.TotalMilliseconds > wait);
		//		}
		//		else
		//		{
		//			var packets = GetPackets(in_size: 60);
		//			foreach (var packet in packets)
		//			{
		//				PacketProcessor(packet);
		//			}
		//		}
		//
		//		_packetFrameCount++;
		//		if (currentTime - lastUpdateTime > SECONDS_PER_FRAME)
		//		{
		//			GameLogicUpdate();
		//			lastUpdateTime = currentTime;
		//
		//			_frameCount++;
		//			if (currentTime - _lastFPSCheckTime >= 1.0)
		//			{
		//				Console.Title = $"PacketFrame: {_packetFrameCount} | FPS: {_frameCount}";
		//				_packetFrameCount = 0;
		//				_frameCount = 0;
		//				_lastFPSCheckTime = currentTime;
		//			}
		//		}
		//	}
		//}

		private void _Update()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			double nextGameLogicTime = 0;  // 다음 게임 로직 업데이트 시간

			while (_isRunning)
			{
				double currentTime = sw.Elapsed.TotalSeconds;

				// 게임 로직 업데이트 (60 FPS)
				if (currentTime >= nextGameLogicTime)
				{
					GameLogicUpdate();
					nextGameLogicTime = currentTime + SECONDS_PER_FRAME;
					_frameCount++;
				}

				// 패킷 처리 (가능한 한 자주)
				if (_packetQueue.Count > 0)
				{
					ProcessPackets();
				}
				else
				{
					double waitTime = nextGameLogicTime - currentTime;
					if (waitTime > 0)
					{
						PreciseWait(waitTime);
					}
				}
				_packetFrameCount++;

				if (currentTime - _lastFPSCheckTime >= 1.0)
				{
					Console.Title = $"PacketFrame: {_packetFrameCount} | FPS: {_frameCount}";
					_frameCount = 0;
					_packetFrameCount = 0;
					_lastFPSCheckTime = currentTime;
				}
			}
		}
		private void PreciseWait(double waitTimeInSeconds)
		{
			//if (waitTimeInSeconds >= 0.015)
			//{
			//	//int w = (int)(waitTimeInSeconds % 0.015 * 1000);
			//	int w = (int)(waitTimeInSeconds * 1000 - 12);
			//	Thread.Sleep(w);
			//	return;
			//}

			if (_useThreadSleep)
			{
				int w = (int)(waitTimeInSeconds * 1000);
				Thread.Sleep(w);
				return;
			}

			var until = Stopwatch.GetTimestamp() + Stopwatch.Frequency * waitTimeInSeconds;
			while (Stopwatch.GetTimestamp() < until) ;
		}

		private void ProcessPackets()
		{
			var packets = GetPackets(in_size: 60);
			foreach (var packet in packets)
			{
				PacketProcessor(packet);
			}
		}

		private List<PacketMessage> GetPackets(int in_size)
		{
			int queueCount = _packetQueue.Count;
			int count = queueCount > in_size ? in_size : queueCount;

			var packets = new List<PacketMessage>();
			for (int i = 0; i < count; i++)
			{
				PacketMessage packet;
				if (false == _packetQueue.TryDequeue(out packet))
				{
					break;
				}
				packets.Add(packet);
			}
			return packets;
		}

		public void AddPacket(ISession session, Packet packet)
		{
			if (false == _isRunning)
				return;

			_packetQueue.Enqueue(new PacketMessage(session, packet));
		}

		public abstract void PacketProcessor(PacketMessage packetMessage);
		public abstract void GameLogicUpdate();
	}
}
