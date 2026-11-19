using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// Pan and zoom of the canvas, observable by the view and by the minimap.
/// </summary>
public sealed class ViewportViewModel : ObservableObject
{
	private const double ButtonZoomStep = 1.2;

	private readonly ViewportState _state = new();

	public event EventHandler? Changed;

	public double Zoom => _state.Zoom;

	public double OffsetX => _state.OffsetX;

	public double OffsetY => _state.OffsetY;

	/// <summary>Size of the canvas control in screen units. The view reports it whenever it changes.</summary>
	public double ViewWidth { get; private set; } = 1280;

	public double ViewHeight { get; private set; } = 720;

	public string ZoomText => $"{Math.Round(Zoom * 100):0}%";

	public Rect2D VisibleWorld => _state.VisibleWorldRect(ViewWidth, ViewHeight);

	public Point2D ViewCenterWorld => _state.ScreenToWorld(new Point2D(ViewWidth / 2, ViewHeight / 2));

	public Point2D ScreenToWorld(Point2D screen) => _state.ScreenToWorld(screen);

	public Point2D WorldToScreen(Point2D world) => _state.WorldToScreen(world);

	public void SetViewSize(double width, double height)
	{
		if (width <= 0 || height <= 0 || (width == ViewWidth && height == ViewHeight))
			return;

		ViewWidth = width;
		ViewHeight = height;
		NotifyChanged();
	}

	public void PanBy(double screenDeltaX, double screenDeltaY)
	{
		_state.PanBy(screenDeltaX, screenDeltaY);
		NotifyChanged();
	}

	/// <summary>Zooms by a factor around a screen point (the pointer position for the mouse wheel).</summary>
	public void ZoomAt(Point2D screenAnchor, double factor)
	{
		_state.ZoomAt(screenAnchor, factor);
		NotifyChanged();
	}

	public void ZoomIn() => ZoomAt(new Point2D(ViewWidth / 2, ViewHeight / 2), ButtonZoomStep);

	public void ZoomOut() => ZoomAt(new Point2D(ViewWidth / 2, ViewHeight / 2), 1 / ButtonZoomStep);

	/// <summary>Back to 100% around the middle of the view.</summary>
	public void ResetZoom()
	{
		var center = ViewCenterWorld;
		_state.Set(1, (ViewWidth / 2) - center.X, (ViewHeight / 2) - center.Y);
		NotifyChanged();
	}

	public void Fit(Rect2D world)
	{
		_state.Fit(world, ViewWidth, ViewHeight);
		NotifyChanged();
	}

	public void CenterOn(Point2D world)
	{
		_state.CenterOn(world, ViewWidth, ViewHeight);
		NotifyChanged();
	}

	private void NotifyChanged()
	{
		OnPropertyChanged(nameof(Zoom));
		OnPropertyChanged(nameof(OffsetX));
		OnPropertyChanged(nameof(OffsetY));
		OnPropertyChanged(nameof(ZoomText));
		Changed?.Invoke(this, EventArgs.Empty);
	}
}
