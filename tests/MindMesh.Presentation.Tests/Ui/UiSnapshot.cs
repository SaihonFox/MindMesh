using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace MindMesh.Presentation.Tests.Ui;

/// <summary>Turns a rendered window into text, so a test log can show what is on the screen.</summary>
internal sealed class UiSnapshot
{
	private readonly byte[] _pixels;
	private readonly int _width;
	private readonly int _height;
	private readonly int _stride;
	private readonly Bitmap _bitmap;

	private UiSnapshot(Bitmap bitmap, byte[] pixels, int width, int height, int stride)
	{
		_bitmap = bitmap;
		_pixels = pixels;
		_width = width;
		_height = height;
		_stride = stride;
	}

	public int Width => _width;

	public int Height => _height;

	public static UiSnapshot Capture(Window window)
	{
		var bitmap = window.CaptureRenderedFrame() ?? throw new InvalidOperationException("The window produced no frame.");
		using var locked = bitmap.Lock();
		var bytes = new byte[locked.RowBytes * locked.Size.Height];
		Marshal.Copy(locked.Address, bytes, 0, bytes.Length);

		return new UiSnapshot(bitmap, bytes, locked.Size.Width, locked.Size.Height, locked.RowBytes);
	}

	/// <summary>The frame as a PNG in base64, cut into lines, so a CI log can carry a real picture of the screen.</summary>
	public string PngBase64()
	{
		using var stream = new MemoryStream();

		_bitmap.Save(stream);

		var text = Convert.ToBase64String(stream.ToArray());
		var builder = new StringBuilder();

		builder.AppendLine("<<<PNG");

		for (var index = 0; index < text.Length; index += 160)
		{
			builder.AppendLine(text.Substring(index, Math.Min(160, text.Length - index)));
		}

		builder.AppendLine("PNG>>>");

		return builder.ToString();
	}

	public double Luminance(int x, int y)
	{
		if (x < 0 || y < 0 || x >= _width || y >= _height)
		{
			return 255;
		}

		var index = (y * _stride) + (x * 4);

		return (_pixels[index] + _pixels[index + 1] + _pixels[index + 2]) / 3.0;
	}

	/// <summary>Counts the pixels of a region that are clearly darker or more saturated than paper.</summary>
	public int CountInk(Rect region, double threshold = 150)
	{
		var count = 0;

		for (var y = (int)region.Top; y < (int)region.Bottom; y++)
		{
			for (var x = (int)region.Left; x < (int)region.Right; x++)
			{
				if (Luminance(x, y) < threshold)
				{
					count++;
				}
			}
		}

		return count;
	}

	/// <summary>One character per cell. Blank is white, a dot is light grey, darker colours get heavier characters.</summary>
	public string Ascii(Rect region, int columns, int rows)
	{
		var builder = new StringBuilder();
		var cellWidth = region.Width / columns;
		var cellHeight = region.Height / rows;

		for (var row = 0; row < rows; row++)
		{
			for (var column = 0; column < columns; column++)
			{
				double sum = 0;
				var samples = 0;

				for (var y = (int)(region.Top + (row * cellHeight)); y < (int)(region.Top + ((row + 1) * cellHeight)); y += 2)
				{
					for (var x = (int)(region.Left + (column * cellWidth)); x < (int)(region.Left + ((column + 1) * cellWidth)); x += 2)
					{
						sum += Luminance(x, y);
						samples++;
					}
				}

				builder.Append(ToCharacter(samples == 0 ? 255 : sum / samples));
			}

			builder.AppendLine();
		}

		return builder.ToString();
	}

	public static string Describe(Visual root, params Type[] types)
	{
		var builder = new StringBuilder();

		foreach (var visual in root.GetVisualDescendants().Where(item => types.Any(type => type.IsInstanceOfType(item))))
		{
			var origin = visual.TranslatePoint(default, root);

			builder.Append(visual.GetType().Name);

			if (visual is Control { Name: { Length: > 0 } name })
			{
				builder.Append($" '{name}'");
			}

			builder.AppendLine(
				$" at ({origin?.X:0.#}, {origin?.Y:0.#}) size {visual.Bounds.Width:0.#}x{visual.Bounds.Height:0.#}"
				+ $" visible={visual.IsVisible}/{visual.IsEffectivelyVisible} opacity={visual.Opacity:0.##}");
		}

		return builder.ToString();
	}

	private static char ToCharacter(double luminance) => luminance switch
	{
		>= 252 => ' ',
		>= 248 => '.',
		>= 240 => ':',
		>= 225 => '-',
		>= 200 => '=',
		>= 150 => '+',
		>= 100 => '*',
		>= 50 => '#',
		_ => '@',
	};
}
