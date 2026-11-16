namespace MindMesh.Presentation.Geometry;

/// <summary>
/// An axis-aligned rectangle. Width and height are never negative.
/// </summary>
public readonly record struct Rect2D(double X, double Y, double Width, double Height)
{
	public double Right => X + Width;

	public double Bottom => Y + Height;

	public Point2D Center => new(X + (Width / 2), Y + (Height / 2));

	public bool IsEmpty => Width <= 0 || Height <= 0;

	/// <summary>The rectangle with the two points as opposite corners.</summary>
	public static Rect2D FromPoints(Point2D first, Point2D second) => new(
		Math.Min(first.X, second.X),
		Math.Min(first.Y, second.Y),
		Math.Abs(first.X - second.X),
		Math.Abs(first.Y - second.Y));

	public bool Contains(Point2D point) =>
		point.X >= X && point.X <= Right && point.Y >= Y && point.Y <= Bottom;

	public bool Intersects(Rect2D other) =>
		X <= other.Right && Right >= other.X && Y <= other.Bottom && Bottom >= other.Y;

	public Rect2D Inflate(double amount) =>
		new(X - amount, Y - amount, Math.Max(0, Width + (2 * amount)), Math.Max(0, Height + (2 * amount)));

	public Rect2D Union(Rect2D other)
	{
		var left = Math.Min(X, other.X);
		var top = Math.Min(Y, other.Y);
		var right = Math.Max(Right, other.Right);
		var bottom = Math.Max(Bottom, other.Bottom);

		return new Rect2D(left, top, right - left, bottom - top);
	}
}
