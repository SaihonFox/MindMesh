using MindMesh.Application.Extensions;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.Presentation.Tests.Support;

/// <summary>The graph editor of a test app, with helpers to add built-in nodes and find their ports.</summary>
internal sealed class EditorFixture : IDisposable
{
	public EditorFixture(Application.Settings.AppSettings? settings = null)
	{
		App = new TestApp(settings);
		Editor = App.Get<GraphEditorViewModel>();
		Catalog = App.Get<IExtensionCatalog>();
	}

	public TestApp App { get; }

	public GraphEditorViewModel Editor { get; }

	public IExtensionCatalog Catalog { get; }

	public INodeDefinition Definition(string id) =>
		Catalog.FindNode(new NodeDefinitionId(id)) ?? throw new InvalidOperationException($"No node '{id}'.");

	/// <summary>Adds a node whose top-left corner is at the given grid-aligned position.</summary>
	public NodeViewModel Add(string id, double x, double y) =>
		Editor.AddNode(Definition(id), new Point2D(x + (NodeMetrics.Width / 2), y + (NodeMetrics.HeaderHeight / 2)));

	public static PortViewModel Output(NodeViewModel node, PortType type) =>
		node.Outputs.First(port => port.Type == type);

	public static PortViewModel Input(NodeViewModel node, PortType type) =>
		node.Inputs.First(port => port.Type == type);

	/// <summary>Connects two ports the way the mouse does: press on one, release on the other.</summary>
	public void Drag(PortViewModel from, PortViewModel to)
	{
		Editor.BeginConnection(from, from.Anchor);
		Editor.UpdateConnection(to.Anchor);
		Editor.CompleteConnection(to.Anchor);
	}

	public void Dispose() => App.Dispose();
}
