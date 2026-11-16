using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// A connection on the canvas, from an output port to an input port.
/// </summary>
public sealed partial class ConnectionViewModel : ObservableObject
{
	[ObservableProperty]
	private ConnectionCurve _curve;

	[ObservableProperty]
	private bool _isSelected;

	/// <summary>True while the connection is being re-attached by the user, so it is drawn faded.</summary>
	[ObservableProperty]
	private bool _isDimmed;

	[ObservableProperty]
	private bool _isInViewport = true;

	public ConnectionViewModel(Connection connection, PortViewModel source, PortViewModel target)
	{
		Connection = connection;
		Source = source;
		Target = target;
		_curve = ConnectionCurve.Between(source.Anchor, target.Anchor);
	}

	public Connection Connection { get; }

	public PortViewModel Source { get; }

	public PortViewModel Target { get; }

	public PortKind Kind => Source.Kind;

	/// <summary>Text on the pill in the middle of the connection. Execution flow needs no label.</summary>
	public string Label => Kind == PortKind.Execution ? string.Empty : Source.Name;

	public bool HasLabel => Label.Length > 0;

	/// <summary>True if the value changes type on its way.</summary>
	public bool RequiresConversion => PortCompatibility.RequiresConversion(Source.Type, Target.Type);

	public Point2D LabelPosition => Curve.Midpoint;

	/// <summary>Recomputes the curve after a connected node moved or collapsed.</summary>
	internal void Update()
	{
		Curve = ConnectionCurve.Between(Source.Anchor, Target.Anchor);
		OnPropertyChanged(nameof(LabelPosition));
	}
}
