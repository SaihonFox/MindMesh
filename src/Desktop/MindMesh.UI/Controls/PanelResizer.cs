using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace MindMesh.UI.Controls;

/// <summary>
/// The edge of a panel on which the resize handle sits.
/// </summary>
public enum ResizeEdge
{
	/// <summary>Right edge of a panel on the left side: dragging right makes the panel wider.</summary>
	Right,

	/// <summary>Left edge of a panel on the right side: dragging left makes the panel wider.</summary>
	Left,

	/// <summary>Top edge of a panel at the bottom: dragging up makes the panel taller.</summary>
	Top,
}

/// <summary>
/// A thin handle that resizes a panel by dragging. It only changes <see cref="Size"/>;
/// bind that two-way to the width or height of the panel.
/// </summary>
public sealed class PanelResizer : Border
{
	public static readonly StyledProperty<double> SizeProperty =
		AvaloniaProperty.Register<PanelResizer, double>(
			nameof(Size),
			defaultBindingMode: BindingMode.TwoWay);

	public static readonly StyledProperty<double> MinSizeProperty =
		AvaloniaProperty.Register<PanelResizer, double>(nameof(MinSize), 0);

	public static readonly StyledProperty<double> MaxSizeProperty =
		AvaloniaProperty.Register<PanelResizer, double>(nameof(MaxSize), double.PositiveInfinity);

	public static readonly StyledProperty<ResizeEdge> EdgeProperty =
		AvaloniaProperty.Register<PanelResizer, ResizeEdge>(nameof(Edge), ResizeEdge.Right);

	private Point _startPoint;
	private double _startSize;
	private bool _isDragging;

	static PanelResizer()
	{
		BackgroundProperty.OverrideDefaultValue<PanelResizer>(Brushes.Transparent);
	}

	public PanelResizer()
	{
		Classes.Add("resizer");
		UpdateCursor();
	}

	/// <summary>Current width or height of the resized panel.</summary>
	public double Size
	{
		get => GetValue(SizeProperty);
		set => SetValue(SizeProperty, value);
	}

	public double MinSize
	{
		get => GetValue(MinSizeProperty);
		set => SetValue(MinSizeProperty, value);
	}

	public double MaxSize
	{
		get => GetValue(MaxSizeProperty);
		set => SetValue(MaxSizeProperty, value);
	}

	public ResizeEdge Edge
	{
		get => GetValue(EdgeProperty);
		set => SetValue(EdgeProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == EdgeProperty)
		{
			UpdateCursor();
		}
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		base.OnPointerPressed(e);

		var root = TopLevel.GetTopLevel(this);

		if (root is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
		{
			return;
		}

		_startPoint = e.GetPosition(root);
		_startSize = Size;
		_isDragging = true;

		e.Pointer.Capture(this);
		e.Handled = true;
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		base.OnPointerMoved(e);

		var root = TopLevel.GetTopLevel(this);

		if (!_isDragging || root is null)
		{
			return;
		}

		var current = e.GetPosition(root);

		var delta = Edge switch
		{
			ResizeEdge.Right => current.X - _startPoint.X,
			ResizeEdge.Left => _startPoint.X - current.X,
			_ => _startPoint.Y - current.Y,
		};

		var min = MinSize;
		var max = Math.Max(min, MaxSize);

		Size = Math.Clamp(_startSize + delta, min, max);
		e.Handled = true;
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		base.OnPointerReleased(e);

		if (!_isDragging)
		{
			return;
		}

		_isDragging = false;
		e.Pointer.Capture(null);
		e.Handled = true;
	}

	protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
	{
		base.OnPointerCaptureLost(e);
		_isDragging = false;
	}

	private void UpdateCursor()
	{
		var type = Edge == ResizeEdge.Top ? StandardCursorType.SizeNorthSouth : StandardCursorType.SizeWestEast;
		Cursor = new Cursor(type);
	}
}
