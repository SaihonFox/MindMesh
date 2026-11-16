using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MindMesh.UI.Icons;

namespace MindMesh.UI.Controls;

/// <summary>
/// Draws an icon from <see cref="IconCatalog"/> as strokes. The color is the inherited foreground,
/// so an icon inside a button follows the button state automatically.
/// </summary>
public sealed class Icon : Control
{
	public static readonly StyledProperty<string?> KeyProperty =
		AvaloniaProperty.Register<Icon, string?>(nameof(Key));

	public static readonly StyledProperty<IBrush?> ForegroundProperty =
		TextBlock.ForegroundProperty.AddOwner<Icon>();

	public static readonly StyledProperty<double> StrokeThicknessProperty =
		AvaloniaProperty.Register<Icon, double>(nameof(StrokeThickness), 1.75);

	static Icon()
	{
		AffectsRender<Icon>(KeyProperty, ForegroundProperty, StrokeThicknessProperty);
		WidthProperty.OverrideDefaultValue<Icon>(16d);
		HeightProperty.OverrideDefaultValue<Icon>(16d);
	}

	public string? Key
	{
		get => GetValue(KeyProperty);
		set => SetValue(KeyProperty, value);
	}

	public IBrush? Foreground
	{
		get => GetValue(ForegroundProperty);
		set => SetValue(ForegroundProperty, value);
	}

	/// <summary>Stroke width in icon units (the icon box is 24 units wide).</summary>
	public double StrokeThickness
	{
		get => GetValue(StrokeThicknessProperty);
		set => SetValue(StrokeThicknessProperty, value);
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		var geometry = IconCatalog.Get(Key);
		var brush = Foreground;

		if (geometry is null || brush is null)
		{
			return;
		}

		var size = Math.Min(Bounds.Width, Bounds.Height);

		if (size <= 0)
		{
			return;
		}

		var scale = size / 24d;
		var offsetX = (Bounds.Width - size) / 2;
		var offsetY = (Bounds.Height - size) / 2;
		var pen = new Pen(brush, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

		using (context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offsetX, offsetY)))
		{
			context.DrawGeometry(null, pen, geometry);
		}
	}
}
