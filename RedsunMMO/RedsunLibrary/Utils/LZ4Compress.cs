using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System;
using System.IO;

namespace RedsunLibrary.Utils
{
	public class LZ4Compress
	{
		public static byte[] LZ4CodecEncode(byte[] data, int offset, int size)
		{
			var target = new byte[LZ4Codec.MaximumOutputSize(size)];
			var encoded = LZ4Codec.Encode(data, offset, size
											, target, 0, target.Length);
			if (-1 == encoded)
			{
				return data;
			}
			return target.AsSpan().Slice(0, encoded).ToArray();
		}

		public static byte[] LZ4CodecDecode(byte[] data, int offset, int size, int originalSize)
		{
			var target = new byte[originalSize];
			var decoded = LZ4Codec.Decode(data, offset, size
											, target, 0, target.Length);

			if (-1 == decoded)
			{
				return null;
			}
			return target;
		}

		public static byte[] LZ4StreamEncode(byte[] data, int offset, int size)
		{
			using (var output = new MemoryStream())
			{
				using (var lz4stream = LZ4Stream.Encode(output))
				{
					lz4stream.Write(data, offset, size);
				}
				return output.ToArray();
			}
		}

		public static byte[] LZ4StreamDecode(byte[] data, int offset, int size)
		{
			using (var output = new MemoryStream())
			{
				using (var lz4stream = LZ4Stream.Decode(new MemoryStream(data, offset, size)))
				{
					lz4stream.CopyTo(output);
				}
				return output.ToArray();
			}
		}
	}
}
