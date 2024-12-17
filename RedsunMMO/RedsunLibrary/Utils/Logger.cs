using System;

namespace RedsunLibrary.Utils
{
	public class Logger
	{
		public static void Print(string message)
		{
			Console.WriteLine($"[RedsunLibrary / Info] : {message}");
		}

		public static void PrintError(string message)
		{
			Console.WriteLine($"[RedsunLibrary / Error] : {message}");
		}
	}
}
