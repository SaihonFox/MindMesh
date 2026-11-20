using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.Extensions;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Validation;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Hotkeys;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// The graph editor: keeps view models in step with a <see cref="GraphDocument"/> and carries out everything the user
/// does on the canvas. The view only forwards pointer and keyboard input, in world coordinates, to the methods here.
/// </summary>
public sealed partial class GraphEditorViewModel : ViewModelBase
{
	/// <summary>Nodes snap to a grid of this size (in world units) while they are dragged or added.</summary>
	public const double GridSize = 8;

	private const double CullingMargin = 240;

	private readonly NodeText _text;
	private readonly EditorWorkspace _workspace;
	private readonly GraphClipboard _clipboard;
	private bool _isRebuildingNode;
	private readonly Dictionary<NodeId, NodeViewModel> _nodesById = [];
	private readonly Dictionary<ConnectionId, ConnectionViewModel> _connectionsById = [];
	private readonly Dictionary<NodeId, List<ConnectionViewModel>> _connectionsByNode = [];
	private int _zCounter;

	[ObservableProperty]
	private bool _snapToGrid = true;

	[ObservableProperty]
	private bool _isMinimapVisible = true;

	public GraphEditorViewModel(
		ILocalizer localizer,
		IExtensionCatalog catalog,
		NodeText text,
		EditorWorkspace workspace,
		GraphClipboard clipboard,
		HotkeyService hotkeys)
		: base(localizer)
	{
		Hotkeys = hotkeys;
		_text = text;
		_workspace = workspace;
		_clipboard = clipboard;

		Viewport = new ViewportViewModel();
		Minimap = new MinimapViewModel(this);
		QuickAdd = new QuickAddViewModel(this, catalog, text, workspace);
		Find = new CanvasFindViewModel(this, text);

		Viewport.Changed += (_, _) =>
		{
			UpdateCulling();
			Minimap.Refresh();
		};

		Document = new GraphDocument();
		Attach(Document);
	}

	/// <summary>Raised after the document's graph changed (also by undo and redo).</summary>
	public event EventHandler<GraphChangedEventArgs>? DocumentChanged;

	public GraphDocument Document { get; private set; }

	public ObservableCollection<NodeViewModel> Nodes { get; } = [];

	public ObservableCollection<ConnectionViewModel> Connections { get; } = [];

	public HotkeyService Hotkeys { get; }

	public ViewportViewModel Viewport { get; }

	public MinimapViewModel Minimap { get; }

	public QuickAddViewModel QuickAdd { get; }

	public CanvasFindViewModel Find { get; }

	public PendingConnectionViewModel Pending { get; } = new();

	public RubberBandViewModel RubberBand { get; } = new();

	public bool HasNodes => Nodes.Count > 0;

	public bool CanUndo => Document.CanUndo;

	public bool CanRedo => Document.CanRedo;

	public bool CanPaste => _clipboard.HasContent;

	/// <summary>The zoom levels offered when the percentage on the canvas is clicked.</summary>
	public IReadOnlyList<ZoomPreset> ZoomPresets { get; } =
		[.. ViewportViewModel.ZoomPresets.Select(zoom => new ZoomPreset(zoom, $"{Math.Round(zoom * 100):0}%"))];

	public NodeViewModel? FindNode(NodeId id) => _nodesById.GetValueOrDefault(id);

	public NodeViewModel GetNode(NodeId id) => _nodesById[id];

	/// <summary>Opens another document, for example a loaded project. The history of the old one is dropped.</summary>
	public void OpenDocument(GraphDocument document)
	{
		ArgumentNullException.ThrowIfNull(document);

		Detach(Document);
		Document = document;
		Attach(document);
		OnPropertyChanged(nameof(Document));
	}

	[RelayCommand(CanExecute = nameof(CanUndo))]
	private void Undo() => Document.Undo();

	[RelayCommand(CanExecute = nameof(CanRedo))]
	private void Redo() => Document.Redo();

	[RelayCommand]
	private void ZoomIn() => Viewport.ZoomIn();

	[RelayCommand]
	private void ZoomOut() => Viewport.ZoomOut();

	[RelayCommand]
	private void ResetZoom() => Viewport.ResetZoom();

	[RelayCommand]
	private void SetZoom(double zoom) => Viewport.SetZoom(zoom);

	[RelayCommand]
	private void FitToView() => Viewport.Fit(GetContentBounds());

	[RelayCommand]
	private void OpenQuickAdd() => QuickAdd.Open(Viewport.ViewCenterWorld);

	[RelayCommand]
	private void OpenFind() => Find.Open();

	/// <summary>The rectangle that contains every node; an empty rectangle at the origin when there are none.</summary>
	public Rect2D GetContentBounds()
	{
		if (Nodes.Count == 0)
			return default;

		var bounds = Nodes[0].Bounds;

		foreach (var node in Nodes.Skip(1))
			bounds = bounds.Union(node.Bounds);

		return bounds;
	}

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(HasNodes));

	private void Attach(GraphDocument document)
	{
		document.Graph.Changed += OnGraphChanged;
		document.Layout.Changed += OnLayoutChanged;
		document.HistoryChanged += OnHistoryChanged;

		Rebuild();
		NotifyHistory();
	}

	private void Detach(GraphDocument document)
	{
		document.Graph.Changed -= OnGraphChanged;
		document.Layout.Changed -= OnLayoutChanged;
		document.HistoryChanged -= OnHistoryChanged;
	}

	private void Rebuild()
	{
		Nodes.Clear();
		Connections.Clear();
		_nodesById.Clear();
		_connectionsById.Clear();
		_connectionsByNode.Clear();
		ClearSelectionState();

		foreach (var node in Document.Graph.Nodes)
			AddNodeViewModel(node);

		foreach (var connection in Document.Graph.Connections)
			AddConnectionViewModel(connection);

		UpdateCulling();
		Minimap.Refresh();
		OnPropertyChanged(nameof(HasNodes));
	}

	private void OnHistoryChanged(object? sender, EventArgs e)
	{
		NotifyHistory();
		_workspace.SaveStatus = SaveStatus.Unsaved;
	}

	private void NotifyHistory()
	{
		OnPropertyChanged(nameof(CanUndo));
		OnPropertyChanged(nameof(CanRedo));
		UndoCommand.NotifyCanExecuteChanged();
		RedoCommand.NotifyCanExecuteChanged();
	}

	private void OnGraphChanged(object? sender, GraphChangedEventArgs e)
	{
		switch (e.Change)
		{
			case NodeAdded added:
				AddNodeViewModel(added.Node);
				Minimap.Refresh();
				break;

			case NodeRemoved removed:
				RemoveNodeViewModel(removed.Node.Id);
				Minimap.Refresh();
				break;

			case ConnectionAdded connectionAdded:
				AddConnectionViewModel(connectionAdded.Connection);
				break;

			case ConnectionRemoved connectionRemoved:
				RemoveConnectionViewModel(connectionRemoved.Connection);
				break;

			case ParameterChanged parameterChanged:
				if (FindNode(parameterChanged.Node) is { } changedNode
					&& changedNode.Definition.DynamicOutputs.Any(group => group.CountParameter == parameterChanged.Key))
					RebuildNodeViewModel(changedNode);

				ValidateNode(parameterChanged.Node);
				break;

			case TitleChanged titleChanged:
				FindNode(titleChanged.Node)?.RefreshTitle();
				break;

			case DisabledChanged disabledChanged:
				FindNode(disabledChanged.Node)?.RefreshDisabled();
				ValidateNode(disabledChanged.Node);
				break;
		}

		DocumentChanged?.Invoke(this, e);
	}

	private void OnLayoutChanged(object? sender, LayoutChangedEventArgs e)
	{
		if (e.Kind == LayoutChangeKind.Removed || FindNode(e.Node) is not { } node)
			return;

		if (e.Kind == LayoutChangeKind.Moved)
		{
			var placement = Document.Layout.GetPlacement(e.Node);
			node.SetPosition(placement.X, placement.Y);
		}
		else if (e.Kind == LayoutChangeKind.Resized)
		{
			node.SetSize(Document.Layout.GetSize(e.Node));
		}
		else
		{
			node.SetCollapsed(Document.Layout.IsCollapsed(e.Node));
		}

		UpdateConnectionsOf(node);
		UpdateCulling(node);
		Minimap.Refresh();
	}

	private void AddNodeViewModel(NodeInstance instance)
	{
		var node = new NodeViewModel(instance, _text, Loc);
		var placement = Document.Layout.GetPlacement(instance.Id);

		node.SetPosition(placement.X, placement.Y);
		node.SetCollapsed(Document.Layout.IsCollapsed(instance.Id));
		node.SetSize(Document.Layout.GetSize(instance.Id));
		node.ZIndex = ++_zCounter;

		_nodesById[instance.Id] = node;
		_connectionsByNode[instance.Id] = [];
		Nodes.Add(node);

		node.SetIssues(NodeValidator.Validate(instance, Document.Graph));
		UpdateCulling(node);
		OnPropertyChanged(nameof(HasNodes));
	}

	/// <summary>
	/// Builds the card of a node again after its number of ports changed (the cases of a switch). The selection
	/// stays as it was and its change is not announced, so the inspector the user is typing in is left alone.
	/// </summary>
	private void RebuildNodeViewModel(NodeViewModel old)
	{
		NodeInstance instance = old.Node;
		bool wasSelected = old.IsSelected;
		int zIndex = old.ZIndex;
		List<Connection> attached = [.. Document.Graph.GetConnections(instance.Id)];

		_isRebuildingNode = true;

		try
		{
			foreach (Connection connection in attached)
				RemoveConnectionViewModel(connection);

			RemoveNodeViewModel(instance.Id);
			AddNodeViewModel(instance);

			NodeViewModel fresh = _nodesById[instance.Id];

			fresh.ZIndex = zIndex;

			if (wasSelected)
			{
				fresh.IsSelected = true;
				_selected.Add(fresh);
			}

			foreach (Connection connection in attached)
				AddConnectionViewModel(connection);
		}
		finally
		{
			_isRebuildingNode = false;
		}
	}

	private void RemoveNodeViewModel(NodeId id)
	{
		if (!_nodesById.Remove(id, out var node))
			return;

		_connectionsByNode.Remove(id);
		Nodes.Remove(node);

		if (node.IsSelected)
		{
			node.IsSelected = false;
			_selected.Remove(node);
			RaiseSelectionChanged();
		}

		OnPropertyChanged(nameof(HasNodes));
	}

	private void AddConnectionViewModel(Connection connection)
	{
		if (FindNode(connection.Source.Node) is not { } sourceNode
			|| FindNode(connection.Target.Node) is not { } targetNode
			|| sourceNode.FindPort(connection.Source.PortKey) is not { } source
			|| targetNode.FindPort(connection.Target.PortKey) is not { } target)
		{
			return;
		}

		var viewModel = new ConnectionViewModel(connection, source, target);

		_connectionsById[connection.Id] = viewModel;
		_connectionsByNode[sourceNode.Id].Add(viewModel);
		_connectionsByNode[targetNode.Id].Add(viewModel);
		Connections.Add(viewModel);

		source.IsConnected = true;
		target.IsConnected = true;

		ValidateNode(sourceNode.Id);
		ValidateNode(targetNode.Id);
		UpdateCulling(viewModel);
	}

	private void RemoveConnectionViewModel(Connection connection)
	{
		if (!_connectionsById.Remove(connection.Id, out var viewModel))
			return;

		_connectionsByNode.GetValueOrDefault(viewModel.Source.Owner.Id)?.Remove(viewModel);
		_connectionsByNode.GetValueOrDefault(viewModel.Target.Owner.Id)?.Remove(viewModel);
		Connections.Remove(viewModel);

		if (ReferenceEquals(SelectedConnection, viewModel))
			SelectedConnection = null;

		viewModel.Source.IsConnected = Document.Graph.GetConnections(viewModel.Source.Reference).Count > 0;
		viewModel.Target.IsConnected = Document.Graph.GetConnections(viewModel.Target.Reference).Count > 0;

		ValidateNode(viewModel.Source.Owner.Id);
		ValidateNode(viewModel.Target.Owner.Id);
	}

	private void ValidateNode(NodeId id)
	{
		if (FindNode(id) is { } node)
			node.SetIssues(NodeValidator.Validate(node.Node, Document.Graph));
	}

	private void UpdateConnectionsOf(NodeViewModel node)
	{
		if (!_connectionsByNode.TryGetValue(node.Id, out var connections))
			return;

		var visible = Viewport.VisibleWorld.Inflate(CullingMargin);

		foreach (var connection in connections)
		{
			connection.Update();
			connection.IsInViewport = connection.Curve.Bounds.Intersects(visible);
		}
	}

	private void UpdateCulling()
	{
		var visible = Viewport.VisibleWorld.Inflate(CullingMargin);

		foreach (var node in Nodes)
			node.IsInViewport = node.IsSelected || node.Bounds.Intersects(visible);

		foreach (var connection in Connections)
			connection.IsInViewport = connection.Curve.Bounds.Intersects(visible);
	}

	private void UpdateCulling(NodeViewModel node) =>
		node.IsInViewport = node.IsSelected || node.Bounds.Intersects(Viewport.VisibleWorld.Inflate(CullingMargin));

	private void UpdateCulling(ConnectionViewModel connection) =>
		connection.IsInViewport = connection.Curve.Bounds.Intersects(Viewport.VisibleWorld.Inflate(CullingMargin));

	private double Snap(double value) => SnapToGrid ? Math.Round(value / GridSize) * GridSize : value;
}
