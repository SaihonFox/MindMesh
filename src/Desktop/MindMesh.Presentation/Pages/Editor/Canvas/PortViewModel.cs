using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// A port of a node on the canvas. Anchors are computed from the node position and fixed card metrics.
/// </summary>
public sealed partial class PortViewModel : ObservableObject
{
	[ObservableProperty]
	private bool _isConnected;

	[ObservableProperty]
	private PortHint _hint;

	public PortViewModel(NodeViewModel owner, PortDefinition definition, int index)
	{
		Owner = owner;
		Definition = definition;
		Index = index;
	}

	public NodeViewModel Owner { get; }

	public PortDefinition Definition { get; }

	/// <summary>Position among the ports of the same side; decides the row.</summary>
	public int Index { get; }

	public string Key => Definition.Key;

	public string Name => Definition.Name;

	public PortDirection Direction => Definition.Direction;

	public PortType Type => Definition.Type;

	public PortKind Kind => Definition.Kind;

	public bool IsInput => Definition.Direction == PortDirection.Input;

	public bool IsRequired => Definition.IsRequired;

	public PortReference Reference => new(Owner.Id, Definition.Key);

	/// <summary>Position relative to the top-left corner of the card: inputs on the left edge, outputs on the right.</summary>
	public Point2D LocalAnchor => new(IsInput ? 0 : Owner.Width, NodeMetrics.GetPortY(Index, Owner.IsCollapsed));

	/// <summary>Diameter of the port dot the view draws.</summary>
	public const double DotSize = 14;

	/// <summary>Left edge of the dot relative to the card, for placing it on a canvas.</summary>
	public double DotX => LocalAnchor.X - (DotSize / 2);

	public double DotY => LocalAnchor.Y - (DotSize / 2);

	/// <summary>Position in the world.</summary>
	public Point2D Anchor => new(Owner.X + LocalAnchor.X, Owner.Y + LocalAnchor.Y);

	/// <summary>Short text for a tooltip, such as "Items (Array)".</summary>
	public string Tooltip => string.IsNullOrEmpty(Definition.Description)
		? $"{Name} ({Type})"
		: $"{Name} ({Type})\n{Definition.Description}";

	/// <summary>Tells the view that the anchor moved.</summary>
	internal void NotifyAnchorChanged()
	{
		OnPropertyChanged(nameof(LocalAnchor));
		OnPropertyChanged(nameof(DotX));
		OnPropertyChanged(nameof(DotY));
		OnPropertyChanged(nameof(Anchor));
	}
}
