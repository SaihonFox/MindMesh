using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;
using MindMesh.Graph.Validation;
using static MindMesh.Graph.Tests.Support.TestNodes;

namespace MindMesh.Graph.Tests.Validation;

public class GraphValidatorTests
{
	private static NodeDefinition Trigger { get; } = NodeDefinition.Create("test.trigger", "Trigger", new NodeCategoryId("Triggers"))
		.Out("done", "Done", PortType.Execution)
		.Build();

	private static NodeDefinition Step { get; } = NodeDefinition.Create("test.step", "Step", Category)
		.In("in", "In", PortType.Execution)
		.Out("out", "Out", PortType.Execution)
		.Build();

	[Fact]
	public void AnEmptyGraph_HasNoIssues() => Assert.Empty(GraphValidator.Validate(new NodeGraph()));

	[Fact]
	public void AGraphWithoutATrigger_GetsAGraphLevelWarning()
	{
		var document = NewDocument();

		AddNode(document, Step);

		ValidationIssue issue = Assert.Single(GraphValidator.Validate(document.Graph));

		Assert.Equal(ValidationCode.GraphHasNoTrigger, issue.Code);
		Assert.Equal(ValidationSeverity.Warning, issue.Severity);
		Assert.True(issue.IsGraphLevel);
	}

	[Fact]
	public void ADisabledTrigger_DoesNotCount()
	{
		var document = NewDocument();
		NodeInstance trigger = AddNode(document, Trigger);

		document.Execute(new SetDisabledCommand([trigger.Id], true));

		Assert.Contains(GraphValidator.Validate(document.Graph), issue => issue.Code == ValidationCode.GraphHasNoTrigger);
	}

	[Fact]
	public void ATriggerConnectedToAStep_IsClean()
	{
		var document = NewDocument();
		NodeInstance trigger = AddNode(document, Trigger);
		NodeInstance step = AddNode(document, Step);

		Connect(document, Port(trigger, "done"), Port(step, "in"));

		Assert.Empty(GraphValidator.Validate(document.Graph));
	}

	[Fact]
	public void ANodeWithoutAnyConnection_IsReported_ButNotWhenItIsAlone()
	{
		var document = NewDocument();
		NodeInstance trigger = AddNode(document, Trigger);

		Assert.Empty(GraphValidator.Validate(document.Graph));

		NodeInstance lonely = AddNode(document, Step);

		ValidationIssue issue = Assert.Single(GraphValidator.Validate(document.Graph));

		Assert.Equal(ValidationCode.NodeNotConnected, issue.Code);
		Assert.Equal(lonely.Id, issue.Node);
		Assert.NotEqual(trigger.Id, issue.Node);
	}

	[Fact]
	public void TheModelRefusesLoops_SoAValidGraphNeverReportsOne()
	{
		NodeGraph graph = new();
		NodeInstance first = NodeInstance.Create(Step);
		NodeInstance second = NodeInstance.Create(Step);

		graph.AddNode(first);
		graph.AddNode(second);
		graph.AddConnection(new Connection(ConnectionId.New(), Port(first, "out"), Port(second, "in")));

		Assert.Throws<InvalidOperationException>(() =>
			graph.AddConnection(new Connection(ConnectionId.New(), Port(second, "out"), Port(first, "in"))));
		Assert.DoesNotContain(GraphValidator.Validate(graph), issue => issue.Code == ValidationCode.GraphHasCycle);
	}

	[Fact]
	public void IsRunnable_IgnoresWarnings_ButNotErrors()
	{
		Assert.True(GraphValidator.IsRunnable([new ValidationIssue(ValidationCode.GraphHasNoTrigger, ValidationSeverity.Warning, NodeId.Empty)]));
		Assert.False(GraphValidator.IsRunnable([new ValidationIssue(ValidationCode.GraphHasCycle, ValidationSeverity.Error, NodeId.New())]));
	}
}
