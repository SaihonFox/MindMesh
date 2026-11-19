using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;
using MindMesh.Graph.Validation;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// A node card on the canvas.
/// </summary>
public sealed partial class NodeViewModel : ViewModelBase
{
	private readonly NodeText _text;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Bounds))]
	private double _x;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Bounds))]
	private double _y;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(VisualState))]
	private bool _isSelected;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Height))]
	[NotifyPropertyChangedFor(nameof(Bounds))]
	private bool _isCollapsed;

	[ObservableProperty]
	private int _zIndex;

	/// <summary>False when the card is far outside the visible area, so the view can skip it.</summary>
	[ObservableProperty]
	private bool _isInViewport = true;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(VisualState))]
	private NodeRunState _runState;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(VisualState))]
	[NotifyPropertyChangedFor(nameof(HasIssues))]
	private NodeIssueLevel _issueLevel;

	[ObservableProperty]
	private int _issueCount;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(VisualState))]
	private bool _isDisabled;

	/// <summary>True while the node is the current result of the search on the canvas.</summary>
	[ObservableProperty]
	private bool _isFindMatch;

	public NodeViewModel(NodeInstance node, NodeText text, ILocalizer localizer)
		: base(localizer)
	{
		Node = node;
		_text = text;
		_isDisabled = node.IsDisabled;

		Inputs = [.. node.Definition.Inputs.Select((port, index) => new PortViewModel(this, port, index))];
		Outputs = [.. node.Definition.Outputs.Select((port, index) => new PortViewModel(this, port, index))];
		Ports = [.. Inputs, .. Outputs];
	}

	public NodeInstance Node { get; }

	public NodeId Id => Node.Id;

	public INodeDefinition Definition => Node.Definition;

	public IReadOnlyList<PortViewModel> Inputs { get; }

	public IReadOnlyList<PortViewModel> Outputs { get; }

	/// <summary>All ports, inputs first.</summary>
	public IReadOnlyList<PortViewModel> Ports { get; }

	public IReadOnlyList<ValidationIssue> Issues { get; private set; } = [];

	public bool HasIssues => IssueLevel != NodeIssueLevel.None;

	public double Width => NodeMetrics.Width;

	public double Height => NodeMetrics.GetHeight(Definition, IsCollapsed);

	public double HeaderHeight => NodeMetrics.HeaderHeight;

	public Rect2D Bounds => new(X, Y, Width, Height);

	public string Title => string.IsNullOrWhiteSpace(Node.Title) ? _text.GetName(Definition) : Node.Title;

	public string Description => _text.GetDescription(Definition);

	public string CategoryTitle => _text.GetPrimaryCategoryTitle(Definition);

	public string IconKey => Definition.IconKey;

	public AccentTone Tone => _text.GetTone(Definition);

	public bool HasPorts => Inputs.Count > 0 || Outputs.Count > 0;

	/// <summary>The look of the card; disabled, runtime and problem states win over plain selection.</summary>
	public NodeVisualState VisualState
	{
		get
		{
			if (IsDisabled)
				return NodeVisualState.Disabled;

			return RunState switch
			{
				NodeRunState.Failed => NodeVisualState.Failed,
				NodeRunState.Running => NodeVisualState.Running,
				NodeRunState.Completed => NodeVisualState.Completed,
				_ when IssueLevel != NodeIssueLevel.None => NodeVisualState.Warning,
				_ when IsSelected => NodeVisualState.Selected,
				_ => NodeVisualState.Default,
			};
		}
	}

	public IEnumerable<PortViewModel> AllPorts => Ports;

	public PortViewModel? FindPort(string key) => AllPorts.FirstOrDefault(port => port.Key == key);

	internal void SetPosition(double x, double y)
	{
		X = x;
		Y = y;
		NotifyAnchorsChanged();
	}

	internal void SetCollapsed(bool collapsed)
	{
		IsCollapsed = collapsed;
		NotifyAnchorsChanged();
	}

	internal void RefreshTitle() => OnPropertyChanged(nameof(Title));

	internal void RefreshDisabled() => IsDisabled = Node.IsDisabled;

	internal void SetIssues(IReadOnlyList<ValidationIssue> issues)
	{
		Issues = issues;
		IssueCount = issues.Count;
		IssueLevel = issues.Count == 0
			? NodeIssueLevel.None
			: issues.Any(issue => issue.Severity == ValidationSeverity.Error) ? NodeIssueLevel.Error : NodeIssueLevel.Warning;

		OnPropertyChanged(nameof(Issues));
	}

	protected override void OnLanguageChanged()
	{
		OnPropertyChanged(nameof(Title));
		OnPropertyChanged(nameof(Description));
		OnPropertyChanged(nameof(CategoryTitle));
	}

	private void NotifyAnchorsChanged()
	{
		foreach (var port in AllPorts)
			port.NotifyAnchorChanged();
	}
}
