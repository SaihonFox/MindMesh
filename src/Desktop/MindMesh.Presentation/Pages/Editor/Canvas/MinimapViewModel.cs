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

	private readonly GraphEditorViewModel _editor;
	private double _scale = 1;
	private double _originX;
	private double _originY;

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

	/// <summary>Recomputes the transform. Called when nodes or the viewport change.</summary>
	public void Refresh()
	{
		var visible = _editor.Viewport.VisibleWorld;
		var bounds = visible;

		foreach (var node in _editor.Nodes)
		{
			bounds = bounds.Union(node.Bounds);
		}

		bounds = bounds.Inflate(32);

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
