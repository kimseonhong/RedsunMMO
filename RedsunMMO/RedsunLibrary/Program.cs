using System;
using System.Threading;

namespace RedsunLibrary
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Thread t = new Thread(new TestGameLogic().Update);
			t.Start();
			Console.ReadKey();
		}
	}
}