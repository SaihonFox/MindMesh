using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

public sealed partial class GraphEditorViewModel
{
	/// <summary>A drag has to travel this many screen units before it counts as a move and not as a click.</summary>
	private const double DragThreshold = 3;

	/// <summary>How close (in screen units) the pointer has to be to a port for a dragged connection to snap to it.</summary>
	private const double PortSnapRadius = 28;

	private const double PasteStep = 32;

	private DragSession? _drag;
	private ResizeSession? _resize;
	private ConnectionViewModel? _reconnecting;
	private readonly List<PortViewModel> _hintedPorts = [];

	private sealed class DragSession(
		Point2D startWorld,
		List<(NodeViewModel Node, NodePlacement Start)> items,
		NodeViewModel pressed,
		bool additive)
	{
		public Point2D StartWorld { get; } = startWorld;

		public List<(NodeViewModel Node, NodePlacement Start)> Items { get; } = items;

		public NodeViewModel Pressed { get; } = pressed;

		public bool Additive { get; } = additive;

		public bool Moved { get; set; }
	}

	// ---- Moving nodes ------------------------------------------------------------------------------------------

	/// <summary>
	/// Starts dragging from a node. A node that is not selected is selected first; Ctrl-click toggles selection and drags nothing.
	/// </summary>
	public void BeginNodeDrag(NodeViewModel node, Point2D world, bool additive)
	{
		_drag = null;

		if (!node.IsSelected)
		{
			SelectNode(node, additive);
		}
		else if (additive)
		{
			SelectNode(node, additive: true);
			return;
		}

		BringToFront(node);

		_drag = new DragSession(
			world,
			[.. _selected.Select(item => (item, Document.Layout.GetPlacement(item.Id)))],
			node,
			additive);
	}

	public void UpdateNodeDrag(Point2D world)
	{
		if (_drag is not { } drag)
			return;

		var delta = world - drag.StartWorld;

		if (!drag.Moved)
		{
			if (Math.Abs(delta.X) * Viewport.Zoom < DragThreshold && Math.Abs(delta.Y) * Viewport.Zoom < DragThreshold)
				return;

			drag.Moved = true;
		}

		var dx = Snap(delta.X);
		var dy = Snap(delta.Y);

		foreach (var (node, start) in drag.Items)
			Document.Layout.SetPlacement(node.Id, new NodePlacement(start.X + dx, start.Y + dy));
	}

	/// <summary>Finishes a drag; moved nodes become one undoable step.</summary>
	public void EndNodeDrag()
	{
		var drag = _drag;
		_drag = null;

		if (drag is null)
			return;

		if (drag.Moved)
		{
			var moves = drag.Items
				.Select(item => new NodeMove(item.Node.Id, item.Start, Document.Layout.GetPlacement(item.Node.Id)))
				.Where(move => move.From != move.To)
				.ToList();

			var command = new MoveNodesCommand(moves);

			if (!command.IsEmpty)
				Document.Execute(command);
		}
		else if (!drag.Additive && _selected.Count > 1)
		{
			// A click without movement on one of several selected nodes selects just that node.
			SetSelection([drag.Pressed]);
		}
	}

	/// <summary>Puts the nodes back where they were when the drag began.</summary>
	public void CancelNodeDrag()
	{
		var drag = _drag;
		_drag = null;

		if (drag is null)
			return;

		foreach (var (node, start) in drag.Items)
			Document.Layout.SetPlacement(node.Id, start);
	}

	public bool IsDraggingNodes => _drag?.Moved == true;

	// ---- Resizing nodes ----------------------------------------------------------------------------------------

	private sealed record ResizeSession(NodeViewModel Node, Point2D StartWorld, double StartWidth, double StartHeight, NodeSize? OriginalSize);

	/// <summary>The node whose bottom-right resize handle is under a world point.</summary>
	public NodeViewModel? HitTestResizeHandle(Point2D world)
	{
		double reach = NodeMetrics.ResizeHandleSize / Viewport.Zoom;

		return Nodes
			.Where(node => node.IsInViewport && !node.IsCollapsed)
			.OrderByDescending(node => node.ZIndex)
			.FirstOrDefault(node =>
				world.X <= node.X + node.Width && world.X >= node.X + node.Width - reach
				&& world.Y <= node.Y + node.Height && world.Y >= node.Y + node.Height - reach);
	}

	public void BeginNodeResize(NodeViewModel node, Point2D world)
	{
		SetSelection([node]);
		BringToFront(node);
		_resize = new ResizeSession(node, world, node.Width, node.Height, Document.Layout.GetSize(node.Id));
	}

	public void UpdateNodeResize(Point2D world)
	{
		if (_resize is not { } resize)
			return;

		double width = Math.Clamp(Snap(resize.StartWidth + (world.X - resize.StartWorld.X)), NodeMetrics.MinimumWidth, NodeMetrics.MaximumWidth);
		double height = Math.Clamp(Snap(resize.StartHeight + (world.Y - resize.StartWorld.Y)), resize.Node.MinimumHeight, Math.Max(resize.Node.MinimumHeight, NodeMetrics.MaximumHeight));

		Document.Layout.SetSize(resize.Node.Id, new NodeSize(width, height));
	}

	/// <summary>Finishes resizing; a changed size becomes one undoable step.</summary>
	public void EndNodeResize()
	{
		ResizeSession? resize = _resize;
		_resize = null;

		if (resize is null)
			return;

		ResizeNodeCommand command = new(resize.Node.Id, resize.OriginalSize, Document.Layout.GetSize(resize.Node.Id));

		if (!command.IsEmpty)
			Document.Execute(command);
	}

	public void CancelNodeResize()
	{
		ResizeSession? resize = _resize;
		_resize = null;

		if (resize is not null)
			Document.Layout.SetSize(resize.Node.Id, resize.OriginalSize);
	}

	// ---- Connecting --------------------------------------------------------------------------------------------

	/// <summary>
	/// Starts a connection from a port. Dragging from an input that already has a connection picks that connection up,
	/// so it can be moved to another input or dropped on empty space to delete it.
	/// </summary>
	public void BeginConnection(PortViewModel port, Point2D world)
	{
		CancelConnection();

		var from = port;

		if (port.IsInput
			&& !port.Definition.AllowMultiple
			&& Document.Graph.GetConnections(port.Reference).FirstOrDefault() is { } existing
			&& _connectionsById.TryGetValue(existing.Id, out var existingViewModel))
		{
			_reconnecting = existingViewModel;
			existingViewModel.IsDimmed = true;
			from = existingViewModel.Source;
		}

		ShowHints(from);
		Pending.Begin(from, world);
	}

	/// <summary>Moves the loose end of the connection; it snaps to the nearest port that accepts it.</summary>
	public void UpdateConnection(Point2D world)
	{
		if (!Pending.IsActive || Pending.From is not { } from)
			return;

		var target = FindSnapTarget(from, world);
		Pending.MoveTo(target?.Anchor ?? world, target);
	}

	/// <summary>
	/// Drops the connection. On a port it connects (one undoable step, also when moving an existing connection);
	/// on empty space a moved connection is deleted and a new one opens the quick-add box.
	/// </summary>
	public void CompleteConnection(Point2D world)
	{
		if (!Pending.IsActive || Pending.From is not { } from)
			return;

		var target = Pending.Target;
		var reconnecting = _reconnecting;

		EndConnectionState();

		if (target is not null)
		{
			if (reconnecting is not null && reconnecting.Target == target)
				return;

			var commands = new List<IGraphCommand>();

			if (reconnecting is not null)
				commands.Add(new DisconnectCommand([reconnecting.Connection.Id]));

			commands.Add(new ConnectCommand(from.Reference, target.Reference));
			Document.Execute(new CompositeCommand("Connect", commands));
			return;
		}

		if (reconnecting is not null)
		{
			Document.Execute(new DisconnectCommand([reconnecting.Connection.Id]));
		}
		else
		{
			QuickAdd.Open(world, from);
		}
	}

	/// <summary>Abandons the connection without changing the graph.</summary>
	public void CancelConnection()
	{
		if (Pending.IsActive)
			EndConnectionState();
	}

	private void EndConnectionState()
	{
		if (_reconnecting is not null)
		{
			_reconnecting.IsDimmed = false;
			_reconnecting = null;
		}

		foreach (var port in _hintedPorts)
			port.Hint = PortHint.None;

		_hintedPorts.Clear();
		Pending.End();
	}

	private void ShowHints(PortViewModel from)
	{
		foreach (var node in Nodes.Where(node => node.IsInViewport && !node.IsCollapsed))
		{
			foreach (var port in node.AllPorts)
			{
				PortHint hint;

				if (port == from)
				{
					hint = PortHint.Source;
				}
				else if (port.Direction == from.Direction)
				{
					continue;
				}
				else if (_reconnecting is not null && port == _reconnecting.Target)
				{
					// Dropping the picked-up connection back where it came from changes nothing.
					hint = PortHint.Compatible;
				}
				else
				{
					hint = Document.Graph.CheckConnection(from.Reference, port.Reference).IsAllowed
						? PortHint.Compatible
						: PortHint.Incompatible;
				}

				port.Hint = hint;
				_hintedPorts.Add(port);
			}
		}
	}

	private PortViewModel? FindSnapTarget(PortViewModel from, Point2D world)
	{
		var radius = PortSnapRadius / Viewport.Zoom;
		PortViewModel? best = null;
		var bestDistance = radius;

		foreach (var node in Nodes)
		{
			if (!node.IsInViewport || node.IsCollapsed)
				continue;

			foreach (var port in node.AllPorts)
			{
				if (port.Hint != PortHint.Compatible)
					continue;

				var distance = port.Anchor.DistanceTo(world);

				if (distance <= bestDistance)
				{
					best = port;
					bestDistance = distance;
				}
			}
		}

		return best;
	}

	/// <summary>The port whose dot is under a world point, if any.</summary>
	public PortViewModel? HitTestPort(Point2D world, double screenRadius = 10)
	{
		var radius = screenRadius / Viewport.Zoom;

		return Nodes
			.Where(node => node.IsInViewport && !node.IsCollapsed)
			.SelectMany(node => node.AllPorts)
			.Where(port => port.Anchor.DistanceTo(world) <= radius)
			.OrderBy(port => port.Anchor.DistanceTo(world))
			.FirstOrDefault();
	}

	// ---- Adding nodes ------------------------------------------------------------------------------------------

	/// <summary>Adds a node so that its card is centered on a world point, and selects it.</summary>
	public NodeViewModel AddNode(INodeDefinition definition, Point2D world)
	{
		var node = NodeInstance.Create(definition);
		var placement = new NodePlacement(
			Snap(world.X - (NodeMetrics.Width / 2)),
			Snap(world.Y - (NodeMetrics.HeaderHeight / 2)));

		Document.Execute(new AddNodeCommand(node, placement));

		var viewModel = _nodesById[node.Id];
		SetSelection([viewModel]);
		BringToFront(viewModel);

		return viewModel;
	}

	/// <summary>Adds a node in the middle of the view, shifted so it does not hide a node that is already there.</summary>
	public NodeViewModel AddNodeAtViewCenter(INodeDefinition definition)
	{
		var center = Viewport.ViewCenterWorld;
		var world = center;

		while (Nodes.Any(node => Math.Abs(node.X - Snap(world.X - (NodeMetrics.Width / 2))) < 1
			&& Math.Abs(node.Y - Snap(world.Y - (NodeMetrics.HeaderHeight / 2))) < 1))
		{
			world = new Point2D(world.X + 24, world.Y + 24);
		}

		return AddNode(definition, world);
	}

	/// <summary>
	/// Adds a node next to a point and connects it to <paramref name="from"/>, as one undoable step.
	/// If no port of the node fits, the node is added without a connection.
	/// </summary>
	public NodeViewModel AddNodeConnected(INodeDefinition definition, Point2D world, PortViewModel from)
	{
		var port = NodeGraphPorts.FindCompatiblePort(definition, from.Definition);

		if (port is null)
			return AddNode(definition, world);

		var node = NodeInstance.Create(definition);
		var portList = from.Direction == PortDirection.Output ? definition.Inputs : definition.Outputs;
		var portIndex = portList.ToList().FindIndex(candidate => candidate.Key == port.Key);

		// Place the card so the connecting port lies at the drop point.
		var x = from.Direction == PortDirection.Output ? world.X : world.X - NodeMetrics.Width;
		var y = world.Y - NodeMetrics.GetPortY(portIndex, collapsed: false);
		var placement = new NodePlacement(Snap(x), Snap(y));

		var newPort = new PortReference(node.Id, port.Key);
		var connect = from.Direction == PortDirection.Output
			? new ConnectCommand(from.Reference, newPort)
			: new ConnectCommand(newPort, from.Reference);

		Document.Execute(new CompositeCommand("Add node", [new AddNodeCommand(node, placement), connect]));

		var viewModel = _nodesById[node.Id];
		SetSelection([viewModel]);
		BringToFront(viewModel);

		return viewModel;
	}

	// ---- Clipboard ---------------------------------------------------------------------------------------------

	public void Copy()
	{
		if (_selected.Count == 0)
			return;

		_clipboard.Set(GraphFragment.Capture(Document, _selected.Select(node => node.Id)));
		OnPropertyChanged(nameof(CanPaste));
	}

	/// <summary>Pastes the copied nodes, each paste a little further down and to the right.</summary>
	public void Paste()
	{
		if (_clipboard.Fragment is not { IsEmpty: false } fragment)
			return;

		var step = _clipboard.NextPasteIndex() * PasteStep;
		ApplyPaste(fragment, step, step);
	}

	/// <summary>Pastes the copied nodes so their top-left corner is at a world point.</summary>
	public void PasteAt(Point2D world)
	{
		if (_clipboard.Fragment is not { IsEmpty: false } fragment)
			return;

		var corner = fragment.TopLeft;
		ApplyPaste(fragment, Snap(world.X - corner.X), Snap(world.Y - corner.Y));
	}

	/// <summary>Copies the selected nodes next to the originals without touching the clipboard.</summary>
	public void Duplicate()
	{
		if (_selected.Count == 0)
			return;

		ApplyPaste(GraphFragment.Capture(Document, _selected.Select(node => node.Id)), PasteStep, PasteStep);
	}

	private void ApplyPaste(GraphFragment fragment, double offsetX, double offsetY)
	{
		var command = fragment.CreatePasteCommand(offsetX, offsetY, out var created);
		Document.Execute(command);
		SetSelection(created.Select(id => _nodesById[id]));

		foreach (var node in _selected)
			BringToFront(node);
	}

	// ---- Settings (used by the inspector) ----------------------------------------------------------------------

	public void SetParameter(NodeViewModel node, string key, ParameterValue value) =>
		Document.Execute(new SetParameterCommand(node.Id, key, value));

	public void SetTitle(NodeViewModel node, string? title) =>
		Document.Execute(new SetTitleCommand(node.Id, title));

	public void SetDisabled(NodeViewModel node, bool isDisabled) =>
		Document.Execute(new SetDisabledCommand([node.Id], isDisabled));
}
