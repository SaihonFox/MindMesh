namespace MindMesh.Presentation.Geometry;

/// <summary>
/// A point or vector in the plane. The presentation layer has its own geometry so it does not depend on the UI framework.
/// </summary>
public readonly record struct Point2D(double X, double Y)
{
	public static Point2D Zero => default;

	public static Point2D operator +(Point2D left, Point2D right) => new(left.X + right.X, left.Y + right.Y);

	public static Point2D operator -(Point2D left, Point2D right) => new(left.X - right.X, left.Y - right.Y);

	public static Point2D operator *(Point2D point, double factor) => new(point.X * factor, point.Y * factor);

	public double DistanceTo(Point2D other)
	{
		var dx = X - other.X;
		var dy = Y - other.Y;

		return Math.Sqrt((dx * dx) + (dy * dy));
	}
}
