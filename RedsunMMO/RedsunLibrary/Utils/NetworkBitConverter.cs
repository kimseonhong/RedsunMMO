using System;

namespace RedsunLibrary.Utils
{
	public static class NetworkBitConverter
	{
		public static bool ToBoolean(byte[] buffer, int startOffset)
		{
			return BitConverter.ToBoolean(buffer, startOffset);
		}
		public static char ToChar(byte[] buffer, int startOffset)
		{
			return BitConverter.ToChar(buffer, startOffset);
		}
		public static short ToInt16(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(short)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(short));
				Array.Reverse(tmp);
				return BitConverter.ToInt16(tmp, 0);
			}
			return BitConverter.ToInt16(buffer, startOffset);
		}
		public static ushort ToUInt16(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(ushort)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(ushort));
				Array.Reverse(tmp);
				return BitConverter.ToUInt16(tmp, 0);
			}
			return BitConverter.ToUInt16(buffer, startOffset);
		}
		public static int ToInt32(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(int)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(int));
				Array.Reverse(tmp);
				return BitConverter.ToInt32(tmp, 0);
			}
			return BitConverter.ToInt32(buffer, startOffset);
		}
		public static uint ToUInt32(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(uint)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(uint));
				Array.Reverse(tmp);
				return BitConverter.ToUInt32(tmp, 0);
			}
			return BitConverter.ToUInt32(buffer, startOffset);
		}
		public static long ToInt64(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(long)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(long));
				Array.Reverse(tmp);
				return BitConverter.ToInt64(tmp, 0);
			}
			return BitConverter.ToInt64(buffer, startOffset);
		}
		public static ulong ToUInt64(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(ulong)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(ulong));
				Array.Reverse(tmp);
				return BitConverter.ToUInt64(tmp, 0);
			}
			return BitConverter.ToUInt64(buffer, startOffset);
		}
		public static float ToSingle(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(float)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(float));
				Array.Reverse(tmp);
				return BitConverter.ToSingle(tmp, 0);
			}
			return BitConverter.ToSingle(buffer, startOffset);
		}
		public static double ToDouble(byte[] buffer, int startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(double)];
				Buffer.BlockCopy(buffer, startOffset, tmp, 0, sizeof(double));
				Array.Reverse(tmp);
				return BitConverter.ToDouble(tmp, 0);
			}
			return BitConverter.ToDouble(buffer, startOffset);
		}

		public static byte[] GetBytes(bool value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(char value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(short value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ushort value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(int value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(uint value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(long value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ulong value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(float value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(double value)
		{
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}

		public static byte[] GetBytes(bool value, out bool out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(char value, out char out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(short value, out short out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ushort value, out ushort out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(int value, out int out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(uint value, out uint out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(long value, out long out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ulong value, out ulong out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(float value, out float out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(double value, out double out_value)
		{
			out_value = value;
			var tmp = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
	}
}
