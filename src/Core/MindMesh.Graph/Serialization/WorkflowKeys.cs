namespace MindMesh.Graph.Serialization;

/// <summary>Names of the fields of the file. Migrations refer to these.</summary>
public static class WorkflowKeys
{
	public const string Format = "format";
	public const string FormatVersion = "formatVersion";
	public const string Id = "id";
	public const string Name = "name";
	public const string Description = "description";
	public const string Nodes = "nodes";
	public const string Connections = "connections";

	public const string NodeType = "type";
	public const string NodeTypeVersion = "typeVersion";
	public const string NodeTitle = "title";
	public const string NodeDisabled = "disabled";
	public const string NodeX = "x";
	public const string NodeY = "y";
	public const string NodeCollapsed = "collapsed";
	public const string NodeParameters = "parameters";

	public const string From = "from";
	public const string To = "to";
	public const string Node = "node";
	public const string Port = "port";
}
