namespace MindMesh.Application.Settings;

/// <summary>
/// Allowed ranges for resizable panels.
/// </summary>
public static class LayoutLimits
{
	public const double SidePanelMinWidth = 200;

	public const double SidePanelMaxWidth = 640;

	public const double ConsoleMinHeight = 120;

	public const double ConsoleMaxHeight = 600;

	/// <summary>
	/// Clamps <paramref name="value"/> into the range; NaN and infinities fall back to <paramref name="fallback"/>.
	/// </summary>
	public static double Sanitize(double value, double min, double max, double fallback)
	{
		if (!double.IsFinite(value))
		{
			return fallback;
		}

		return Math.Clamp(value, min, max);
	}
}
