namespace MindMesh.Presentation.Geometry;

/// <summary>
/// The pan and zoom of the canvas. World coordinates belong to the graph; screen coordinates belong to the view.
/// <c>screen = world * Zoom + Offset</c>.
/// </summary>
public sealed class ViewportState
{
	public const double MinimumZoom = 0.25;
	public const double MaximumZoom = 4.0;

	public double Zoom { get; private set; } = 1;

	public double OffsetX { get; private set; }

	public double OffsetY { get; private set; }

	public Point2D WorldToScreen(Point2D world) => new((world.X * Zoom) + OffsetX, (world.Y * Zoom) + OffsetY);

	public Point2D ScreenToWorld(Point2D screen) => new((screen.X - OffsetX) / Zoom, (screen.Y - OffsetY) / Zoom);

	public void Set(double zoom, double offsetX, double offsetY)
	{
		Zoom = Math.Clamp(zoom, MinimumZoom, MaximumZoom);
		OffsetX = offsetX;
		OffsetY = offsetY;
	}

	public void PanBy(double screenDeltaX, double screenDeltaY) =>
		Set(Zoom, OffsetX + screenDeltaX, OffsetY + screenDeltaY);

	/// <summary>Changes the zoom by a factor while the world point under <paramref name="screenAnchor"/> stays put.</summary>
	public void ZoomAt(Point2D screenAnchor, double factor)
	{
		var world = ScreenToWorld(screenAnchor);
		var zoom = Math.Clamp(Zoom * factor, MinimumZoom, MaximumZoom);

		Set(zoom, screenAnchor.X - (world.X * zoom), screenAnchor.Y - (world.Y * zoom));
	}

	/// <summary>The part of the world that is visible in a view of the given size.</summary>
	public Rect2D VisibleWorldRect(double viewWidth, double viewHeight)
	{
		var topLeft = ScreenToWorld(Point2D.Zero);

		return new Rect2D(topLeft.X, topLeft.Y, viewWidth / Zoom, viewHeight / Zoom);
	}

	/// <summary>Zooms and pans so that the whole <paramref name="world"/> rectangle fits with a margin.</summary>
	public void Fit(Rect2D world, double viewWidth, double viewHeight, double margin = 64)
	{
		if (world.IsEmpty && world.Width <= 0 && world.Height <= 0)
		{
			Set(1, (viewWidth / 2) - world.X, (viewHeight / 2) - world.Y);
			return;
		}

		var availableWidth = Math.Max(1, viewWidth - (2 * margin));
		var availableHeight = Math.Max(1, viewHeight - (2 * margin));
		var zoom = Math.Min(availableWidth / Math.Max(world.Width, 1), availableHeight / Math.Max(world.Height, 1));

		// Fitting should not magnify a small graph beyond its natural size.
		// and it is rounded down to a whole percent, so the number on screen is the real one.
		zoom = Math.Clamp(Math.Floor(Math.Min(zoom, 1) * 100) / 100, MinimumZoom, MaximumZoom);

		var center = world.Center;
		Set(zoom, (viewWidth / 2) - (center.X * zoom), (viewHeight / 2) - (center.Y * zoom));
	}

	/// <summary>Pans so that the world point is in the middle of the view.</summary>
	public void CenterOn(Point2D world, double viewWidth, double viewHeight) =>
		Set(Zoom, (viewWidth / 2) - (world.X * Zoom), (viewHeight / 2) - (world.Y * Zoom));
}
