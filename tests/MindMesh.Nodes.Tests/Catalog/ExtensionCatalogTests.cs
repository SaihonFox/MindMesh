using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Application.Extensions;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Ports;
using MindMesh.Nodes.BuiltIn;
using MindMesh.Plugins.Abstractions;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Plugins.Abstractions.Projects;

namespace MindMesh.Nodes.Tests.Catalog;

public class ExtensionCatalogTests
{
	private static ExtensionCatalog Build(params IMindMeshExtension[] extensions) =>
		new(extensions, NullLogger<ExtensionCatalog>.Instance);

	private static NodeDefinition Node(string id, NodeCategoryId category) =>
		NodeDefinition.Create(id, id, category).Out("out", "Out", PortType.Any).Build();

	[Fact]
	public void TheBuiltInExtension_RegistersWithoutProblems()
	{
		var catalog = Build(new BuiltInExtension());

		Assert.Empty(catalog.Problems);
		Assert.Equal(50, catalog.Nodes.Count);
		Assert.Equal(BuiltInCategories.All.Count, catalog.Categories.Count);
		Assert.Single(catalog.Extensions);
	}

	[Fact]
	public void ThereIsExactlyOneBuiltInProjectKind_WithTheFourModes()
	{
		var catalog = Build(new BuiltInExtension());

		var kind = Assert.Single(catalog.ProjectKinds);

		Assert.Equal(BuiltInExtension.WorkflowKind, kind.Id);
		EditorModeId[] expected =
		[
			BuiltInExtension.WorkflowMode,
			BuiltInExtension.AiAgentMode,
			BuiltInExtension.DataFlowMode,
			BuiltInExtension.LogicMode,
		];

		Assert.Equal(expected, kind.Modes.Select(mode => mode.Id).ToArray());
	}

	[Fact]
	public void EveryCategoryIsUsedByAModeAndHasNodes()
	{
		var catalog = Build(new BuiltInExtension());
		var inModes = catalog.ProjectKinds.SelectMany(kind => kind.Modes).SelectMany(mode => mode.Categories).ToHashSet();

		foreach (var category in catalog.Categories)
		{
			Assert.Contains(category.Id, inModes);
			Assert.NotEmpty(catalog.GetNodes(category.Id));
		}
	}

	[Fact]
	public void EveryNodeAppearsInAtLeastOneMode()
	{
		var catalog = Build(new BuiltInExtension());
		var inModes = catalog.ProjectKinds.SelectMany(kind => kind.Modes).SelectMany(mode => mode.Categories).ToHashSet();

		foreach (var node in catalog.Nodes)
		{
			Assert.True(node.Categories.Any(inModes.Contains), $"{node.Id} is in no mode.");
		}
	}

	[Fact]
	public void Lookups_FindWhatWasRegistered()
	{
		var catalog = Build(new BuiltInExtension());

		Assert.NotNull(catalog.FindNode(new NodeDefinitionId("logic.if")));
		Assert.Null(catalog.FindNode(new NodeDefinitionId("nope")));
		Assert.NotNull(catalog.FindCategory(BuiltInCategories.Triggers));
		Assert.NotNull(catalog.FindMode(BuiltInExtension.LogicMode));
		Assert.NotNull(catalog.FindProjectKind(BuiltInExtension.WorkflowKind));
	}

	[Fact]
	public void ANodeInSeveralCategories_IsListedInEachOfThem()
	{
		var catalog = Build(new BuiltInExtension());
		var setVariable = catalog.FindNode(new NodeDefinitionId("data.set-variable"))!;

		Assert.True(setVariable.Categories.Count > 1);
		Assert.All(setVariable.Categories, category => Assert.Contains(setVariable, catalog.GetNodes(category)));
	}

	[Fact]
	public void ADuplicateExtension_IsReportedAndIgnored()
	{
		var catalog = Build(new BuiltInExtension(), new BuiltInExtension());

		Assert.Single(catalog.Extensions);
		Assert.Single(catalog.Problems);
		Assert.Equal(50, catalog.Nodes.Count);
	}

	[Fact]
	public void ANodeInAnUnknownCategory_IsRejectedAndTheRestStillWorks()
	{
		var catalog = Build(new BuiltInExtension(), new TestExtension("plugin.a", registrar =>
		{
			registrar.AddNode(Node("plugin.lost", new NodeCategoryId("Missing")));
			registrar.AddNode(Node("plugin.fine", BuiltInCategories.Triggers));
		}));

		var problem = Assert.Single(catalog.Problems);
		Assert.Equal("plugin.a", problem.ExtensionId);
		Assert.Null(catalog.FindNode(new NodeDefinitionId("plugin.lost")));
		Assert.NotNull(catalog.FindNode(new NodeDefinitionId("plugin.fine")));
	}

	[Fact]
	public void ADuplicateNodeId_IsRejected()
	{
		var catalog = Build(new BuiltInExtension(), new TestExtension("plugin.a", registrar =>
			registrar.AddNode(Node("logic.if", BuiltInCategories.Logic))));

		Assert.Single(catalog.Problems);
		Assert.Equal(50, catalog.Nodes.Count);
	}

	[Fact]
	public void APluginCanAddItsOwnCategoryNodesAndProjectKind()
	{
		var category = new NodeCategoryDefinition(new NodeCategoryId("Robots"), "Robots", "bolt", AccentTone.Red);
		var mode = new EditorModeDefinition(new EditorModeId("Robot"), "Robot", [category.Id]);
		var kind = new ProjectKindDefinition(new ProjectKindId("RobotProject"), "Robot project", "Robots.", "workflow", [mode]);

		var catalog = Build(new BuiltInExtension(), new TestExtension("plugin.robots", registrar =>
		{
			// The node is registered before its category: the order must not matter.
			registrar.AddNode(Node("robots.walk", category.Id));
			registrar.AddNodeCategory(category);
			registrar.AddProjectKind(kind);
		}));

		Assert.Empty(catalog.Problems);
		Assert.Equal(2, catalog.ProjectKinds.Count);
		Assert.NotNull(catalog.FindMode(mode.Id));
		Assert.Single(catalog.GetNodes(category.Id));
	}

	[Fact]
	public void AProjectKindWithAnUnknownCategoryOrNoModes_IsRejected()
	{
		var unknown = new EditorModeDefinition(new EditorModeId("Ghost"), "Ghost", [new NodeCategoryId("Missing")]);

		var catalog = Build(new BuiltInExtension(), new TestExtension("plugin.bad", registrar =>
		{
			registrar.AddProjectKind(new ProjectKindDefinition(new ProjectKindId("Ghosts"), "Ghosts", "", "workflow", [unknown]));
			registrar.AddProjectKind(new ProjectKindDefinition(new ProjectKindId("Empty"), "Empty", "", "workflow", []));
		}));

		Assert.Equal(2, catalog.Problems.Count);
		Assert.Single(catalog.ProjectKinds);
	}

	[Fact]
	public void AnExtensionThatThrows_DoesNotBreakTheOthers()
	{
		var catalog = Build(new TestExtension("plugin.broken", _ => throw new InvalidOperationException("boom")), new BuiltInExtension());

		var problem = Assert.Single(catalog.Problems);
		Assert.Contains("boom", problem.Message);
		Assert.Equal(50, catalog.Nodes.Count);
	}

	private sealed class TestExtension(string id, Action<IExtensionRegistrar> register) : IMindMeshExtension
	{
		public ExtensionInfo Info { get; } = new(id, id, new Version(1, 0), "tests");

		public void Register(IExtensionRegistrar registrar) => register(registrar);
	}
}
