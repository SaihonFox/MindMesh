using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// The connection the user is dragging out of a port. It follows the pointer and snaps to a port it may connect to.
/// </summary>
public sealed partial class PendingConnectionViewModel : ObservableObject
{
	[ObservableProperty]
	private bool _isActive;

	[ObservableProperty]
	private ConnectionCurve _curve;

	[ObservableProperty]
	private PortKind _kind;

	/// <summary>True while the end snaps to a port that accepts the connection.</summary>
	[ObservableProperty]
	private bool _isValid;

	/// <summary>The port the drag started from (or, when re-attaching, the output of the old connection).</summary>
	public PortViewModel? From { get; private set; }

	/// <summary>The port the end is snapped to, if any.</summary>
	public PortViewModel? Target { get; private set; }

	internal void Begin(PortViewModel from, Point2D pointer)
	{
		From = from;
		Target = null;
		Kind = from.Kind;
		IsValid = false;
		IsActive = true;
		MoveTo(pointer, null);
	}

	internal void MoveTo(Point2D point, PortViewModel? target)
	{
		Target = target;
		IsValid = target is not null;

		if (From is null)
		{
			return;
		}

		var anchor = From.Anchor;
		Curve = From.IsInput
			? ConnectionCurve.Between(point, anchor)
			: ConnectionCurve.Between(anchor, point);
	}

	internal void End()
	{
		IsActive = false;
		IsValid = false;
		From = null;
		Target = null;
	}
}
