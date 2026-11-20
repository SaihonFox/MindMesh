using MindMesh.Graph.Definitions;

namespace MindMesh.Presentation.Geometry;

/// <summary>
/// Fixed dimensions of a node card. The view and the view models share these numbers, so port anchors,
/// hit tests, culling and the minimap do not need the visual tree.
/// </summary>
public static class NodeMetrics
{
	public const double Width = 272;
	public const double MinimumWidth = 200;
	public const double MaximumWidth = 640;
	public const double MaximumHeight = 800;

	/// <summary>Side of the square in the bottom-right corner that resizes a card, in screen units.</summary>
	public const double ResizeHandleSize = 16;
	public const double HeaderHeight = 68;
	public const double BodyPaddingTop = 8;
	public const double BodyPaddingBottom = 12;
	public const double PortRowHeight = 28;
	public const double PortRadius = 6;

	/// <summary>Full height of a card.</summary>
	public static double GetHeight(INodeDefinition definition, bool collapsed)
	{
		if (collapsed)
			return HeaderHeight;

		var rows = Math.Max(definition.Inputs.Count, definition.Outputs.Count);

		return rows == 0
			? HeaderHeight
			: HeaderHeight + BodyPaddingTop + (rows * PortRowHeight) + BodyPaddingBottom;
	}

	/// <summary>
	/// Vertical position of a port relative to the top of the card. A collapsed card has no rows,
	/// so every port sits at the middle of the header.
	/// </summary>
	public static double GetPortY(int index, bool collapsed) => collapsed
		? HeaderHeight / 2
		: HeaderHeight + BodyPaddingTop + (index * PortRowHeight) + (PortRowHeight / 2);
}
