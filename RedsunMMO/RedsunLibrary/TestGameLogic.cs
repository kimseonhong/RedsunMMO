using RedsunLibrary.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RedsunLibrary
{
	public class TestGameLogic
	{
		public const double FPS = 120.0;
		public const double SECONDS_PER_FRAME = 1.0 / FPS;

		private int packetFrameCount = 0;
		private int frameCount = 0;
		private double lastFPSCheckTime = 0;

		private ConcurrentQueue<Packet> packetQueue = new ConcurrentQueue<Packet>();

		public void Update()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			double lastUpdateTime = 0;

			while (true)
			{
				double currentTime = sw.Elapsed.TotalSeconds;
				double wait = sw.Elapsed.TotalMilliseconds + SECONDS_PER_FRAME;
				if (packetQueue.Count <= 0)
				{
					//Thread.Sleep(1);
					SpinWait.SpinUntil(() => sw.Elapsed.TotalMilliseconds > wait);
				}
				else
				{
					var packets = GetPackets(in_size: 60);
					foreach (var packet in packets)
					{
						PacketProcessor(packet);
					}
				}

				packetFrameCount++;
				if (currentTime - lastUpdateTime > SECONDS_PER_FRAME)
				{
					GameLogicUpdate();
					lastUpdateTime = currentTime;

					frameCount++;
					if (currentTime - lastFPSCheckTime >= 1.0)
					{
						Console.Title = $"PacketFrame: {packetFrameCount} | FPS: {frameCount}";
						packetFrameCount = 0;
						frameCount = 0;
						lastFPSCheckTime = currentTime;
					}
				}
			}
		}

		private List<Packet> GetPackets(int in_size)
		{
			int queueCount = packetQueue.Count;
			int count = queueCount > in_size ? in_size : queueCount;

			var packets = new List<Packet>();
			for (int i = 0; i < count; i++)
			{
				Packet packet;
				if (false == packetQueue.TryDequeue(out packet))
				{
					break;
				}
				packets.Add(packet);
			}
			return packets;
		}

		private void PacketProcessor(Packet packet)
		{

		}

		private void GameLogicUpdate()
		{

		}
	}
}
