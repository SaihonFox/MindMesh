namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>What the runtime reports about a node. The runtime arrives in Phase 4; until then nodes stay idle.</summary>
public enum NodeRunState
{
	Idle,
	Running,
	Completed,
	Failed,
}

/// <summary>The look of a node card.</summary>
public enum NodeVisualState
{
	Default,
	Selected,
	Running,
	Completed,
	Failed,
	Disabled,
	Warning,
}

/// <summary>The most serious problem found in a node.</summary>
public enum NodeIssueLevel
{
	None,
	Warning,
	Error,
}

/// <summary>How a port is shown while the user drags a new connection.</summary>
public enum PortHint
{
	None,

	/// <summary>The port the drag started from.</summary>
	Source,

	/// <summary>A connection to this port is possible.</summary>
	Compatible,

	/// <summary>A connection to this port is not possible.</summary>
	Incompatible,
}
