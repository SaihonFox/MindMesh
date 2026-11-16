using Avalonia;
using Avalonia.Controls;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.UI.Controls;

/// <summary>
/// The card of a node. It turns the visual state of the node into pseudo-classes
/// (<c>:running</c>, <c>:completed</c>, <c>:failed</c>, <c>:disabled</c>, <c>:warning</c>) and
/// <c>:selected</c> for selection and <c>:found</c> for search hits, so the styles of the theme decide how each state looks.
/// </summary>
public sealed class NodeCard : Border
{
	public static readonly StyledProperty<NodeVisualState> VisualStateProperty =
		AvaloniaProperty.Register<NodeCard, NodeVisualState>(nameof(VisualState));

	public static readonly StyledProperty<bool> IsSelectedProperty =
		AvaloniaProperty.Register<NodeCard, bool>(nameof(IsSelected));

	public static readonly StyledProperty<bool> IsFoundProperty =
		AvaloniaProperty.Register<NodeCard, bool>(nameof(IsFound));

	static NodeCard()
	{
		VisualStateProperty.Changed.AddClassHandler<NodeCard>((card, _) => card.UpdatePseudoClasses());
		IsFoundProperty.Changed.AddClassHandler<NodeCard>((card, _) => card.UpdatePseudoClasses());
		IsSelectedProperty.Changed.AddClassHandler<NodeCard>((card, _) => card.UpdatePseudoClasses());
	}

	public NodeCard()
	{
		UpdatePseudoClasses();
	}

	public NodeVisualState VisualState
	{
		get => GetValue(VisualStateProperty);
		set => SetValue(VisualStateProperty, value);
	}

	/// <summary>Selection is shown on top of every other state, so a selected node with a warning keeps its outline.</summary>
	public bool IsSelected
	{
		get => GetValue(IsSelectedProperty);
		set => SetValue(IsSelectedProperty, value);
	}

	public bool IsFound
	{
		get => GetValue(IsFoundProperty);
		set => SetValue(IsFoundProperty, value);
	}

	private void UpdatePseudoClasses()
	{
		var state = VisualState;

		PseudoClasses.Set(":selected", IsSelected);
		PseudoClasses.Set(":running", state == NodeVisualState.Running);
		PseudoClasses.Set(":completed", state == NodeVisualState.Completed);
		PseudoClasses.Set(":failed", state == NodeVisualState.Failed);
		PseudoClasses.Set(":disabled-node", state == NodeVisualState.Disabled);
		PseudoClasses.Set(":warning", state == NodeVisualState.Warning);
		PseudoClasses.Set(":found", IsFound);
	}
}
