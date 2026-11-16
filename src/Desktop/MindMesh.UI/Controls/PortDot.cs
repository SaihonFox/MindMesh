using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.UI.Controls;

/// <summary>
/// The dot of a port on the edge of a node card. The shape tells the kind of port apart:
/// a circle for data, a triangle for execution flow, a diamond for events and a rounded square for AI context.
/// A connected port is filled; a free one is a ring.
/// </summary>
public sealed class PortDot : Control
{
	public static readonly StyledProperty<PortKind> KindProperty =
		AvaloniaProperty.Register<PortDot, PortKind>(nameof(Kind));

	public static readonly StyledProperty<bool> IsConnectedProperty =
		AvaloniaProperty.Register<PortDot, bool>(nameof(IsConnected));

	public static readonly StyledProperty<PortHint> HintProperty =
		AvaloniaProperty.Register<PortDot, PortHint>(nameof(Hint));

	static PortDot()
	{
		AffectsRender<PortDot>(KindProperty, IsConnectedProperty, HintProperty);
		WidthProperty.OverrideDefaultValue<PortDot>(PortViewModel.DotSize);
		HeightProperty.OverrideDefaultValue<PortDot>(PortViewModel.DotSize);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property.Name == "ActualThemeVariant")
		{
			InvalidateVisual();
		}
	}

	public PortKind Kind
	{
		get => GetValue(KindProperty);
		set => SetValue(KindProperty, value);
	}

	public bool IsConnected
	{
		get => GetValue(IsConnectedProperty);
		set => SetValue(IsConnectedProperty, value);
	}

	public PortHint Hint
	{
		get => GetValue(HintProperty);
		set => SetValue(HintProperty, value);
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		var color = Find(Kind switch
		{
			PortKind.Execution => "MM.Brush.PortExecution",
			PortKind.Event => "MM.Brush.PortEvent",
			PortKind.AIContext => "MM.Brush.PortAIContext",
			_ => "MM.Brush.PortData",
		});

		var ring = Find("MM.Brush.NodeRing");
		var hint = Hint;
		var opacity = hint == PortHint.Incompatible ? 0.3 : 1;
		var scale = hint is PortHint.Compatible or PortHint.Source ? 1.25 : 1;

		var size = Math.Min(Bounds.Width, Bounds.Height) * scale;
		var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
		var half = (size / 2) - 1;

		using (context.PushOpacity(opacity))
		{
			var fill = IsConnected || hint == PortHint.Source ? color : ring;
			var pen = new Pen(color, 2);
			var geometry = CreateShape(center, half);

			context.DrawGeometry(fill, pen, geometry);
		}

		if (hint == PortHint.Compatible && Find("MM.Brush.Accent") is { } accent)
		{
			context.DrawEllipse(null, new Pen(accent, 1.5), center, (size / 2) + 3, (size / 2) + 3);
		}
	}

	private Geometry CreateShape(Point center, double half)
	{
		switch (Kind)
		{
			case PortKind.Execution:
				return Polygon(
					new Point(center.X - half, center.Y - half),
					new Point(center.X + half, center.Y),
					new Point(center.X - half, center.Y + half));

			case PortKind.Event:
				return Polygon(
					new Point(center.X, center.Y - half),
					new Point(center.X + half, center.Y),
					new Point(center.X, center.Y + half),
					new Point(center.X - half, center.Y));

			case PortKind.AIContext:
				return new RectangleGeometry(new Rect(center.X - half, center.Y - half, half * 2, half * 2), 3, 3);

			default:
				return new EllipseGeometry(new Rect(center.X - half, center.Y - half, half * 2, half * 2));
		}
	}

	private static StreamGeometry Polygon(params Point[] points)
	{
		var geometry = new StreamGeometry();

		using (var context = geometry.Open())
		{
			context.BeginFigure(points[0], true);

			for (var index = 1; index < points.Length; index++)
			{
				context.LineTo(points[index]);
			}

			context.EndFigure(true);
		}

		return geometry;
	}

	private IBrush? Find(string key) =>
		this.TryFindResource(key, ActualThemeVariant, out var resource) ? resource as IBrush : null;
}
