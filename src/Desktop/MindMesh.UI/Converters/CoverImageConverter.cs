using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace MindMesh.UI.Converters;

/// <summary>
/// Loads a cover picture from its file path. The file is read into memory first, so it stays free
/// and can be replaced while the picture is shown.
/// </summary>
public sealed class CoverImageConverter : IValueConverter
{
	public static CoverImageConverter Instance { get; } = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not string { Length: > 0 } path || !File.Exists(path))
			return null;

		try
		{
			using MemoryStream stream = new(File.ReadAllBytes(path));

			return Bitmap.DecodeToWidth(stream, 480);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
		{
			return null;
		}
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
