using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MindMesh.UI.Controls;

/// <summary>
/// The grid drawn behind the node canvas, as lines or as dots. It follows the zoom and pan of the canvas.
/// </summary>
public sealed class CanvasGridBackground : Control
{
	/// <summary>Lines closer together than this (in pixels) are not drawn.</summary>
	private const double MinimumVisibleStep = 6;

	public static readonly StyledProperty<bool> UseDotsProperty =
		AvaloniaProperty.Register<CanvasGridBackground, bool>(nameof(UseDots));

	public static readonly StyledProperty<double> SpacingProperty =
		AvaloniaProperty.Register<CanvasGridBackground, double>(nameof(Spacing), 24);

	public static readonly StyledProperty<int> MajorEveryProperty =
		AvaloniaProperty.Register<CanvasGridBackground, int>(nameof(MajorEvery), 5);

	public static readonly StyledProperty<double> ZoomProperty =
		AvaloniaProperty.Register<CanvasGridBackground, double>(nameof(Zoom), 1);

	public static readonly StyledProperty<Point> OffsetProperty =
		AvaloniaProperty.Register<CanvasGridBackground, Point>(nameof(Offset));

	public static readonly StyledProperty<IBrush?> MinorBrushProperty =
		AvaloniaProperty.Register<CanvasGridBackground, IBrush?>(nameof(MinorBrush));

	public static readonly StyledProperty<IBrush?> MajorBrushProperty =
		AvaloniaProperty.Register<CanvasGridBackground, IBrush?>(nameof(MajorBrush));

	static CanvasGridBackground()
	{
		AffectsRender<CanvasGridBackground>(
			UseDotsProperty,
			SpacingProperty,
			MajorEveryProperty,
			ZoomProperty,
			OffsetProperty,
			MinorBrushProperty,
			MajorBrushProperty);

		ClipToBoundsProperty.OverrideDefaultValue<CanvasGridBackground>(true);
		IsHitTestVisibleProperty.OverrideDefaultValue<CanvasGridBackground>(false);
	}

	/// <summary>Draws a dot at every crossing instead of lines.</summary>
	public bool UseDots
	{
		get => GetValue(UseDotsProperty);
		set => SetValue(UseDotsProperty, value);
	}

	/// <summary>Distance between minor lines at zoom 1, in pixels.</summary>
	public double Spacing
	{
		get => GetValue(SpacingProperty);
		set => SetValue(SpacingProperty, value);
	}

	/// <summary>Every n-th line is a major line.</summary>
	public int MajorEvery
	{
		get => GetValue(MajorEveryProperty);
		set => SetValue(MajorEveryProperty, value);
	}

	public double Zoom
	{
		get => GetValue(ZoomProperty);
		set => SetValue(ZoomProperty, value);
	}

	/// <summary>Position of the grid origin in control coordinates (the canvas pan).</summary>
	public Point Offset
	{
		get => GetValue(OffsetProperty);
		set => SetValue(OffsetProperty, value);
	}

	public IBrush? MinorBrush
	{
		get => GetValue(MinorBrushProperty);
		set => SetValue(MinorBrushProperty, value);
	}

	public IBrush? MajorBrush
	{
		get => GetValue(MajorBrushProperty);
		set => SetValue(MajorBrushProperty, value);
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		var step = Spacing * Zoom;
		var majorEvery = Math.Max(1, MajorEvery);

		if (step < MinimumVisibleStep || Bounds.Width <= 0 || Bounds.Height <= 0)
		{
			return;
		}

		if (UseDots)
		{
			RenderDots(context, step, majorEvery);
			return;
		}

		var minorPen = MinorBrush is null ? null : new Pen(MinorBrush, 1);
		var majorPen = MajorBrush is null ? null : new Pen(MajorBrush, 1);

		for (var x = Mod(Offset.X, step); x <= Bounds.Width; x += step)
		{
			var index = (int)Math.Round((x - Offset.X) / step);
			var pen = IsMajor(index, majorEvery) ? majorPen : minorPen;
			var snapped = Math.Floor(x) + 0.5;

			if (pen is not null)
			{
				context.DrawLine(pen, new Point(snapped, 0), new Point(snapped, Bounds.Height));
			}
		}

		for (var y = Mod(Offset.Y, step); y <= Bounds.Height; y += step)
		{
			var index = (int)Math.Round((y - Offset.Y) / step);
			var pen = IsMajor(index, majorEvery) ? majorPen : minorPen;
			var snapped = Math.Floor(y) + 0.5;

			if (pen is not null)
			{
				context.DrawLine(pen, new Point(0, snapped), new Point(Bounds.Width, snapped));
			}
		}
	}

	private void RenderDots(DrawingContext context, double step, int majorEvery)
	{
		var minor = MinorBrush;
		var major = MajorBrush ?? minor;

		if (minor is null)
		{
			return;
		}

		// Dots keep their size when zooming out, but never get smaller than one pixel.
		var radius = Math.Clamp(step / 16, 0.9, 1.6);

		for (var x = Mod(Offset.X, step); x <= Bounds.Width; x += step)
		{
			var column = (int)Math.Round((x - Offset.X) / step);

			for (var y = Mod(Offset.Y, step); y <= Bounds.Height; y += step)
			{
				var row = (int)Math.Round((y - Offset.Y) / step);
				var isMajor = IsMajor(column, majorEvery) && IsMajor(row, majorEvery);

				context.DrawEllipse(isMajor ? major : minor, null, new Point(x, y), radius, radius);
			}
		}
	}

	private static bool IsMajor(int index, int majorEvery) =>
		((index % majorEvery) + majorEvery) % majorEvery == 0;

	private static double Mod(double value, double modulus)
	{
		var result = value % modulus;
		return result < 0 ? result + modulus : result;
	}
}
