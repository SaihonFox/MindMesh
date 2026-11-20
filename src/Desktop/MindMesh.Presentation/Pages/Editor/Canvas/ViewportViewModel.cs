using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// Pan and zoom of the canvas, observable by the view and by the minimap.
/// </summary>
public sealed class ViewportViewModel : ObservableObject
{
	/// <summary>The zoom levels the buttons and the wheel step through: round numbers a person can name.</summary>
	public static readonly IReadOnlyList<double> ZoomPresets = [0.05, 0.1, 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4];

	private const double PresetTolerance = 0.005;

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

	public void ZoomIn() => StepAt(new Point2D(ViewWidth / 2, ViewHeight / 2), zoomIn: true);

	public void ZoomOut() => StepAt(new Point2D(ViewWidth / 2, ViewHeight / 2), zoomIn: false);

	/// <summary>Moves to the next round zoom level above or below the current one, around a screen point.</summary>
	public void StepAt(Point2D screenAnchor, bool zoomIn)
	{
		double target = zoomIn
			? ZoomPresets.FirstOrDefault(preset => preset > Zoom + PresetTolerance, ViewportState.MaximumZoom)
			: ZoomPresets.LastOrDefault(preset => preset < Zoom - PresetTolerance, ViewportState.MinimumZoom);

		ZoomAt(screenAnchor, target / Zoom);
	}

	/// <summary>Sets an exact zoom around the middle of the view.</summary>
	public void SetZoom(double zoom) => ZoomAt(new Point2D(ViewWidth / 2, ViewHeight / 2), zoom / Zoom);

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
