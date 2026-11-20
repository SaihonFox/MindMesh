using System.IO.Compression;

namespace MindMesh.Presentation.Projects.Covers;

/// <summary>
/// Draws a random colorful abstract picture in a pixel-art style (large square pixels, a small palette, ordered
/// dithering) and encodes it as PNG.
/// The same seed always gives the same picture. No UI types are needed, so it works in tests too.
/// </summary>
public static class CoverGenerator
{
	public const int Width = 480;

	public const int Height = 288;

	/// <summary>The side of one big pixel of the picture.</summary>
	public const int CellSize = 16;

	/// <summary>How many shades each color channel can have; few shades give the flat, pixel-art look.</summary>
	private const int Levels = 6;

	private static readonly int[,] _bayer =
	{
		{ 0, 8, 2, 10 },
		{ 12, 4, 14, 6 },
		{ 3, 11, 1, 9 },
		{ 15, 7, 13, 5 },
	};

	private static readonly uint[] _crcTable = BuildCrcTable();

	/// <summary>Creates the PNG file bytes of the picture for a seed.</summary>
	public static byte[] CreatePng(int seed, int width = Width, int height = Height)
	{
		byte[] pixels = Render(seed, width, height);

		return Encode(pixels, width, height);
	}

	/// <summary>The RGB pixels (3 bytes each, row by row).</summary>
	public static byte[] Render(int seed, int width, int height)
	{
		Random random = new(seed);
		double baseHue = random.NextDouble() * 360;
		double topHue = baseHue + 20 + random.NextDouble() * 60;
		double bottomHue = baseHue - 20 - random.NextDouble() * 60;

		(double X, double Y, double Radius, double Hue)[] blobs = new (double, double, double, double)[5 + random.Next(3)];

		for (int i = 0; i < blobs.Length; i++)
		{
			blobs[i] = (
				random.NextDouble() * width,
				random.NextDouble() * height,
				height * (0.35 + random.NextDouble() * 0.65),
				baseHue + (random.NextDouble() - 0.5) * 220);
		}

		byte[] pixels = new byte[width * height * 3];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				// every pixel of a cell takes the color of the cell's center
				int cellX = x / CellSize;
				int cellY = y / CellSize;
				double sampleX = (cellX * CellSize) + (CellSize / 2.0);
				double sampleY = (cellY * CellSize) + (CellSize / 2.0);

				double t = (sampleY / height * 0.7) + (sampleX / width * 0.3);
				(double red, double green, double blue) = FromHsv(topHue + ((bottomHue - topHue) * t), 0.55, 0.42 + (0.18 * (1 - t)));

				foreach ((double blobX, double blobY, double radius, double hue) in blobs)
				{
					double dx = sampleX - blobX;
					double dy = sampleY - blobY;
					double distance = ((dx * dx) + (dy * dy)) / (radius * radius);

					if (distance >= 1)
						continue;

					double amount = (1 - distance) * (1 - distance) * 0.85;
					(double blobRed, double blobGreen, double blobBlue) = FromHsv(hue, 0.7, 0.95);

					red += (blobRed - red) * amount;
					green += (blobGreen - green) * amount;
					blue += (blobBlue - blue) * amount;
				}

				int index = ((y * width) + x) * 3;

				double dither = ((_bayer[cellY % 4, cellX % 4] / 16.0) - 0.47) / Levels;

				pixels[index] = Quantize(red + dither);
				pixels[index + 1] = Quantize(green + dither);
				pixels[index + 2] = Quantize(blue + dither);
			}
		}

		return pixels;
	}

	private static byte Quantize(double value)
	{
		int level = (int)Math.Round(Math.Clamp(value, 0, 1) * (Levels - 1));

		return (byte)Math.Round(level * 255.0 / (Levels - 1));
	}

	private static byte ToByte(double value) => (byte)Math.Clamp((int)Math.Round(value * 255), 0, 255);

	private static (double Red, double Green, double Blue) FromHsv(double hue, double saturation, double value)
	{
		hue = ((hue % 360) + 360) % 360;

		double chroma = value * saturation;
		double section = hue / 60;
		double second = chroma * (1 - Math.Abs((section % 2) - 1));
		double match = value - chroma;

		(double red, double green, double blue) = (int)section switch
		{
			0 => (chroma, second, 0.0),
			1 => (second, chroma, 0.0),
			2 => (0.0, chroma, second),
			3 => (0.0, second, chroma),
			4 => (second, 0.0, chroma),
			_ => (chroma, 0.0, second),
		};

		return (red + match, green + match, blue + match);
	}

	private static byte[] Encode(byte[] rgb, int width, int height)
	{
		using MemoryStream output = new();

		output.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

		byte[] header = new byte[13];

		WriteInt(header, 0, width);
		WriteInt(header, 4, height);
		header[8] = 8; // bit depth
		header[9] = 2; // RGB
		WriteChunk(output, "IHDR", header);

		using (MemoryStream raw = new())
		{
			using (ZLibStream zlib = new(raw, CompressionLevel.Optimal, leaveOpen: true))
			{
				for (int y = 0; y < height; y++)
				{
					zlib.WriteByte(0); // filter: none
					zlib.Write(rgb, y * width * 3, width * 3);
				}
			}

			WriteChunk(output, "IDAT", raw.ToArray());
		}

		WriteChunk(output, "IEND", []);

		return output.ToArray();
	}

	private static void WriteChunk(Stream stream, string type, byte[] data)
	{
		byte[] length = new byte[4];
		byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(type);

		WriteInt(length, 0, data.Length);
		stream.Write(length);
		stream.Write(typeBytes);
		stream.Write(data);

		uint crc = 0xFFFFFFFF;

		foreach (byte value in typeBytes)
			crc = _crcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);

		foreach (byte value in data)
			crc = _crcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);

		byte[] checksum = new byte[4];

		WriteInt(checksum, 0, (int)(crc ^ 0xFFFFFFFF));
		stream.Write(checksum);
	}

	private static void WriteInt(byte[] buffer, int offset, int value)
	{
		buffer[offset] = (byte)(value >> 24);
		buffer[offset + 1] = (byte)(value >> 16);
		buffer[offset + 2] = (byte)(value >> 8);
		buffer[offset + 3] = (byte)value;
	}

	private static uint[] BuildCrcTable()
	{
		uint[] table = new uint[256];

		for (uint n = 0; n < 256; n++)
		{
			uint c = n;

			for (int k = 0; k < 8; k++)
				c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;

			table[n] = c;
		}

		return table;
	}
}
