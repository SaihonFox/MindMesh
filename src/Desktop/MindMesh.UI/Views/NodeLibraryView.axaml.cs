using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using MindMesh.Presentation.Pages.Editor.Library;
using MindMesh.UI.Controls;

namespace MindMesh.UI.Views;

/// <summary>
/// The node library. A tile is added to the canvas by double-click or by dragging it onto the canvas.
/// The drag uses pointer capture and a small ghost on the overlay layer, so the library never needs to know
/// about the canvas: it only reports where in the window the tile was dropped.
/// </summary>
public partial class NodeLibraryView : UserControl
{
	private const double DragThreshold = 6;

	private LibraryNodeViewModel? _pressedItem;
	private Point _pressPoint;
	private bool _isDragging;
	private Control? _ghost;
	private OverlayLayer? _overlay;

	public NodeLibraryView()
	{
		InitializeComponent();
	}

	/// <summary>Raised when a tile is released; the point is relative to the window.</summary>
	public event EventHandler<NodeDroppedEventArgs>? NodeDropped;

	private void OnTilePressed(object? sender, PointerPressedEventArgs e)
	{
		if (sender is not Border { Tag: LibraryNodeViewModel item } tile
			|| !e.GetCurrentPoint(tile).Properties.IsLeftButtonPressed)
		{
			return;
		}

		_pressedItem = item;
		_pressPoint = e.GetPosition(TopLevel.GetTopLevel(this));
		_isDragging = false;

		e.Pointer.Capture(tile);
	}

	private void OnTileMoved(object? sender, PointerEventArgs e)
	{
		if (_pressedItem is null || sender is not Border tile || !ReferenceEquals(e.Pointer.Captured, tile))
		{
			return;
		}

		var position = e.GetPosition(TopLevel.GetTopLevel(this));

		if (!_isDragging)
		{
			if (Math.Abs(position.X - _pressPoint.X) < DragThreshold && Math.Abs(position.Y - _pressPoint.Y) < DragThreshold)
			{
				return;
			}

			_isDragging = true;
			ShowGhost(_pressedItem);
		}

		MoveGhost(position);
	}

	private void OnTileReleased(object? sender, PointerReleasedEventArgs e)
	{
		var item = _pressedItem;
		var wasDragging = _isDragging;
		var position = e.GetPosition(TopLevel.GetTopLevel(this));

		Reset();
		e.Pointer.Capture(null);

		if (item is not null && wasDragging)
		{
			NodeDropped?.Invoke(this, new NodeDroppedEventArgs(item, position));
		}
	}

	private void OnTileCaptureLost(object? sender, PointerCaptureLostEventArgs e) => Reset();

	private void OnTileDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (sender is Border { Tag: LibraryNodeViewModel item } && DataContext is NodeLibraryViewModel library)
		{
			library.AddToCanvasCommand.Execute(item);
		}
	}

	private void ShowGhost(LibraryNodeViewModel item)
	{
		_overlay = OverlayLayer.GetOverlayLayer(this);

		if (_overlay is null)
		{
			return;
		}

		var pill = new TonePill
		{
			Width = 28,
			Height = 28,
			CornerRadius = new CornerRadius(7),
			Tone = item.Tone,
			Child = new Icon { Key = item.IconKey, Width = 15, Height = 15, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center },
		};

		var label = new TextBlock { Text = item.Title, FontWeight = Avalonia.Media.FontWeight.SemiBold, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(8, 0, 4, 0) };

		var ghost = new Border
		{
			Classes = { "floating" },
			Padding = new Thickness(6),
			IsHitTestVisible = false,
			Opacity = 0.9,
			Child = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { pill, label } },
		};

		_ghost = ghost;
		_overlay.Children.Add(ghost);
	}

	private void MoveGhost(Point windowPoint)
	{
		if (_ghost is null)
		{
			return;
		}

		Canvas.SetLeft(_ghost, windowPoint.X + 12);
		Canvas.SetTop(_ghost, windowPoint.Y + 12);
	}

	private void Reset()
	{
		if (_ghost is not null)
		{
			_overlay?.Children.Remove(_ghost);
		}

		_ghost = null;
		_overlay = null;
		_pressedItem = null;
		_isDragging = false;
	}
}

/// <summary>A library tile was dropped somewhere in the window.</summary>
public sealed class NodeDroppedEventArgs(LibraryNodeViewModel item, Point windowPoint) : EventArgs
{
	public LibraryNodeViewModel Item { get; } = item;

	/// <summary>Where the pointer was released, relative to the top-level window.</summary>
	public Point WindowPoint { get; } = windowPoint;
}
