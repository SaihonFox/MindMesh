namespace MindMesh.Presentation.Geometry;

/// <summary>
/// The cubic Bezier curve of a connection. Both the canvas and the view models use this one definition,
/// so what is drawn and what is hit-tested always agree.
/// </summary>
public readonly record struct ConnectionCurve(Point2D Start, Point2D Control1, Point2D Control2, Point2D End)
{
	private const double MinimumHandle = 48;
	private const double MaximumHandle = 240;

	/// <summary>
	/// Builds the curve from an output anchor to an input anchor. Handles point away from the card sides,
	/// so a connection that goes backwards still leaves to the right and enters from the left.
	/// </summary>
	public static ConnectionCurve Between(Point2D start, Point2D end)
	{
		var distance = Math.Abs(end.X - start.X);
		var handle = Math.Clamp(distance * 0.5, MinimumHandle, MaximumHandle);

		if (end.X < start.X)
			handle = Math.Clamp(handle + ((start.X - end.X) * 0.25), MinimumHandle, MaximumHandle);

		return new ConnectionCurve(
			start,
			new Point2D(start.X + handle, start.Y),
			new Point2D(end.X - handle, end.Y),
			end);
	}

	/// <summary>The point at parameter <paramref name="t"/> (0 = start, 1 = end).</summary>
	public Point2D PointAt(double t)
	{
		var u = 1 - t;

		return (Start * (u * u * u))
			+ (Control1 * (3 * u * u * t))
			+ (Control2 * (3 * u * t * t))
			+ (End * (t * t * t));
	}

	public Point2D Midpoint => PointAt(0.5);

	/// <summary>A box that contains the whole curve (its control polygon).</summary>
	public Rect2D Bounds
	{
		get
		{
			var left = Math.Min(Math.Min(Start.X, Control1.X), Math.Min(Control2.X, End.X));
			var right = Math.Max(Math.Max(Start.X, Control1.X), Math.Max(Control2.X, End.X));
			var top = Math.Min(Math.Min(Start.Y, Control1.Y), Math.Min(Control2.Y, End.Y));
			var bottom = Math.Max(Math.Max(Start.Y, Control1.Y), Math.Max(Control2.Y, End.Y));

			return new Rect2D(left, top, right - left, bottom - top);
		}
	}

	/// <summary>Distance from <paramref name="point"/> to the curve, found by sampling.</summary>
	public double DistanceTo(Point2D point, int samples = 32)
	{
		var best = double.MaxValue;

		for (int index = 0; index <= samples; index++)
		{
			var distance = PointAt(index / (double)samples).DistanceTo(point);

			if (distance < best)
				best = distance;
		}

		return best;
	}
}
