using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System;
using System.IO;

namespace RedsunLibrary.Utils
{
	public class LZ4Compress
	{
		public static byte[] LZ4CodecEncode(byte[] in_data, int in_offset, int in_size)
		{
			var target = new byte[LZ4Codec.MaximumOutputSize(in_size)];
			var encoded = LZ4Codec.Encode(in_data, in_offset, in_size
											, target, 0, target.Length);
			if (-1 == encoded)
			{
				return in_data;
			}
			return target.AsSpan().Slice(0, encoded).ToArray();
		}

		public static byte[] LZ4CodecDecode(byte[] in_data, int in_offset, int in_size, int in_originalSize)
		{
			var target = new byte[in_originalSize];
			var decoded = LZ4Codec.Decode(in_data, in_offset, in_size
											, target, 0, target.Length);

			if (-1 == decoded)
			{
				return null;
			}
			return target;
		}

		public static byte[] LZ4StreamEncode(byte[] in_data, int in_offset, int in_size)
		{
			using (var output = new MemoryStream())
			{
				using (var lz4stream = LZ4Stream.Encode(output))
				{
					lz4stream.Write(in_data, in_offset, in_size);
				}
				return output.ToArray();
			}
		}

		public static byte[] LZ4StreamDecode(byte[] in_data, int in_offset, int in_size)
		{
			using (var output = new MemoryStream())
			{
				using (var lz4stream = LZ4Stream.Decode(new MemoryStream(in_data, in_offset, in_size)))
				{
					lz4stream.CopyTo(output);
				}
				return output.ToArray();
			}
		}
	}
}
