namespace MindMesh.Presentation.Common;

/// <summary>
/// Names of icons. The UI layer maps every key to vector data; a test keeps both sides in sync.
/// </summary>
public static class IconKeys
{
	public const string Logo = "logo";
	public const string Projects = "projects";
	public const string Workflows = "workflows";
	public const string Templates = "templates";
	public const string Marketplace = "marketplace";
	public const string Executions = "executions";
	public const string Plugins = "plugins";
	public const string Settings = "settings";
	public const string Undo = "undo";
	public const string Redo = "redo";
	public const string AutoLayout = "auto-layout";
	public const string Validate = "validate";
	public const string Run = "run";
	public const string Debug = "debug";
	public const string Assistant = "assistant";
	public const string PanelLeft = "panel-left";
	public const string PanelRight = "panel-right";
	public const string PanelBottom = "panel-bottom";
	public const string Terminal = "terminal";
	public const string Search = "search";
	public const string Close = "close";
	public const string User = "user";
	public const string Info = "info";
	public const string List = "list";
	public const string Alert = "alert";
	public const string Braces = "braces";
	public const string Database = "database";
	public const string ChevronDown = "chevron-down";
	public const string ChevronRight = "chevron-right";
	public const string Plus = "plus";
	public const string Minus = "minus";
	public const string Fit = "fit";
	public const string Eye = "eye";
	public const string EyeOff = "eye-off";
	public const string Trash = "trash";
	public const string Folder = "folder";
	public const string Map = "map";
	public const string Node = "node";

	public const string Edit = "edit";

	public const string Image = "image";

	public const string Shuffle = "shuffle";

	/// <summary>All keys, for tests and tooling.</summary>
	public static IReadOnlyList<string> All { get; } =
	[
		Logo, Projects, Workflows, Templates, Marketplace, Executions, Plugins, Settings,
		Undo, Redo, AutoLayout, Validate, Run, Debug, Assistant,
		PanelLeft, PanelRight, PanelBottom, Terminal, Search, Close, User, Info,
		List, Alert, Braces, Database,
		ChevronDown, ChevronRight, Plus, Minus, Fit, Eye, EyeOff, Trash, Folder, Map, Node,
	];
}
