using System;

namespace RedsunLibrary.Utils
{
	public static class NetworkBitConverter
	{
		public static bool ToBoolean(byte[] in_buffer, int in_startOffset)
		{
			return BitConverter.ToBoolean(in_buffer, in_startOffset);
		}
		public static char ToChar(byte[] in_buffer, int in_startOffset)
		{
			return BitConverter.ToChar(in_buffer, in_startOffset);
		}
		public static short ToInt16(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(short)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(short));
				Array.Reverse(tmp);
				return BitConverter.ToInt16(tmp, in_startOffset);
			}
			return BitConverter.ToInt16(in_buffer, in_startOffset);
		}
		public static ushort ToUInt16(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(ushort)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(ushort));
				Array.Reverse(tmp);
				return BitConverter.ToUInt16(tmp, in_startOffset);
			}
			return BitConverter.ToUInt16(in_buffer, in_startOffset);
		}
		public static int ToInt32(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(int)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(int));
				Array.Reverse(tmp);
				return BitConverter.ToInt32(tmp, in_startOffset);
			}
			return BitConverter.ToInt32(in_buffer, in_startOffset);
		}
		public static uint ToUInt32(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(uint)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(uint));
				Array.Reverse(tmp);
				return BitConverter.ToUInt32(tmp, in_startOffset);
			}
			return BitConverter.ToUInt32(in_buffer, in_startOffset);
		}
		public static long ToInt64(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(long)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(long));
				Array.Reverse(tmp);
				return BitConverter.ToInt64(tmp, in_startOffset);
			}
			return BitConverter.ToInt64(in_buffer, in_startOffset);
		}
		public static ulong ToUInt64(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(ulong)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(ulong));
				Array.Reverse(tmp);
				return BitConverter.ToUInt64(tmp, in_startOffset);
			}
			return BitConverter.ToUInt64(in_buffer, in_startOffset);
		}
		public static float ToSingle(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(float)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(float));
				Array.Reverse(tmp);
				return BitConverter.ToSingle(tmp, in_startOffset);
			}
			return BitConverter.ToSingle(in_buffer, in_startOffset);
		}
		public static double ToDouble(byte[] in_buffer, int in_startOffset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] tmp = new byte[sizeof(double)];
				Buffer.BlockCopy(in_buffer, in_startOffset, tmp, 0, sizeof(double));
				Array.Reverse(tmp);
				return BitConverter.ToDouble(tmp, in_startOffset);
			}
			return BitConverter.ToDouble(in_buffer, in_startOffset);
		}

		public static byte[] GetBytes(bool in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(char in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(short in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ushort in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(int in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(uint in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(long in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ulong in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(float in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(double in_value)
		{
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}

		public static byte[] GetBytes(bool in_value, out bool out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(char in_value, out char out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(short in_value, out short out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ushort in_value, out ushort out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(int in_value, out int out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(uint in_value, out uint out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(long in_value, out long out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(ulong in_value, out ulong out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(float in_value, out float out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
		public static byte[] GetBytes(double in_value, out double out_value)
		{
			out_value = in_value;
			var tmp = BitConverter.GetBytes(in_value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return tmp;
		}
	}
}
