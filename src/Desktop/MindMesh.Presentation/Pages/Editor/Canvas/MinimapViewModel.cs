using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// The overview in the corner of the canvas. It only computes the transform from world to minimap and the
/// rectangle of the visible area; the view draws the nodes itself from the editor's node list.
/// </summary>
public sealed class MinimapViewModel : ObservableObject
{
	public const double Width = 200;
	public const double Height = 132;
	private const double Padding = 8;

	/// <summary>The smallest world area the minimap covers, so a few nodes do not blow up to fill it.</summary>
	private const double MinimumWorldWidth = 1800;
	private const double MinimumWorldHeight = 1200;

	private readonly GraphEditorViewModel _editor;
	private double _scale = 1;
	private double _originX;
	private double _originY;
	private bool _isDragging;
	private double _grabX;
	private double _grabY;

	public MinimapViewModel(GraphEditorViewModel editor)
	{
		_editor = editor;
		Refresh();
	}

	public event EventHandler? Changed;

	/// <summary>The visible part of the world, in minimap coordinates.</summary>
	public Rect2D ViewportRect { get; private set; }

	public double Scale => _scale;

	public Point2D WorldToMinimap(Point2D world) => new(((world.X - _originX) * _scale) + Padding, ((world.Y - _originY) * _scale) + Padding);

	public Point2D MinimapToWorld(Point2D point) => new(((point.X - Padding) / _scale) + _originX, ((point.Y - Padding) / _scale) + _originY);

	public Rect2D WorldToMinimap(Rect2D world)
	{
		var topLeft = WorldToMinimap(new Point2D(world.X, world.Y));

		return new Rect2D(topLeft.X, topLeft.Y, world.Width * _scale, world.Height * _scale);
	}

	/// <summary>Centers the canvas on the world point that corresponds to a point of the minimap.</summary>
	public void NavigateTo(Point2D minimapPoint) => _editor.Viewport.CenterOn(MinimapToWorld(minimapPoint));

	/// <summary>
	/// Starts dragging the camera. The map stops rescaling while the drag lasts, so the camera follows
	/// the pointer one to one. Grabbing the frame keeps the grab point; pressing elsewhere centers the camera there.
	/// </summary>
	public void BeginDrag(Point2D minimapPoint)
	{
		_isDragging = true;
		_grabX = 0;
		_grabY = 0;

		Rect2D frame = ViewportRect;
		bool insideFrame = minimapPoint.X >= frame.X && minimapPoint.X <= frame.X + frame.Width
			&& minimapPoint.Y >= frame.Y && minimapPoint.Y <= frame.Y + frame.Height;

		Point2D world = MinimapToWorld(minimapPoint);

		if (insideFrame)
		{
			Point2D center = _editor.Viewport.ViewCenterWorld;

			_grabX = world.X - center.X;
			_grabY = world.Y - center.Y;
		}
		else
		{
			_editor.Viewport.CenterOn(world);
		}
	}

	public void DragTo(Point2D minimapPoint)
	{
		if (!_isDragging)
			return;

		Point2D world = MinimapToWorld(minimapPoint);

		_editor.Viewport.CenterOn(new Point2D(world.X - _grabX, world.Y - _grabY));
	}

	public void EndDrag()
	{
		_isDragging = false;
		Refresh();
	}

	/// <summary>Recomputes the transform. Called when nodes or the viewport change.</summary>
	public void Refresh()
	{
		var visible = _editor.Viewport.VisibleWorld;

		if (_isDragging)
		{
			ViewportRect = WorldToMinimap(visible);
			OnPropertyChanged(nameof(ViewportRect));
			Changed?.Invoke(this, EventArgs.Empty);
			return;
		}

		var bounds = visible;

		foreach (var node in _editor.Nodes)
			bounds = bounds.Union(node.Bounds);

		bounds = bounds.Inflate(32);

		if (bounds.Width < MinimumWorldWidth)
			bounds = new Rect2D(bounds.X - ((MinimumWorldWidth - bounds.Width) / 2), bounds.Y, MinimumWorldWidth, bounds.Height);

		if (bounds.Height < MinimumWorldHeight)
			bounds = new Rect2D(bounds.X, bounds.Y - ((MinimumWorldHeight - bounds.Height) / 2), bounds.Width, MinimumWorldHeight);

		var availableWidth = Width - (2 * Padding);
		var availableHeight = Height - (2 * Padding);

		_scale = Math.Min(availableWidth / Math.Max(bounds.Width, 1), availableHeight / Math.Max(bounds.Height, 1));

		// Center the content in the minimap.
		_originX = bounds.X - (((availableWidth / _scale) - bounds.Width) / 2);
		_originY = bounds.Y - (((availableHeight / _scale) - bounds.Height) / 2);

		ViewportRect = WorldToMinimap(visible);
		OnPropertyChanged(nameof(ViewportRect));
		Changed?.Invoke(this, EventArgs.Empty);
	}
}
