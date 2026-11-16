using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Layout;
using static MindMesh.Graph.Tests.Support.TestNodes;

namespace MindMesh.Graph.Tests.Editing;

public class GraphFragmentTests
{
	[Fact]
	public void Capture_KeepsOnlyTheRequestedNodesAndTheConnectionsBetweenThem()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		var outsider = AddNode(document, Consumer);
		Connect(document, Port(producer, "text"), Port(consumer, "text"));
		Connect(document, Port(producer, "exec"), Port(outsider, "exec"));

		var fragment = GraphFragment.Capture(document, [producer.Id, consumer.Id]);
		document.Execute(fragment.CreatePasteCommand(0, 0, out _));

		// 3 originals + 2 copies; 2 original connections + the 1 connection between the copied nodes.
		Assert.Equal(5, document.Graph.Nodes.Count);
		Assert.Equal(3, document.Graph.Connections.Count);
	}

	[Fact]
	public void Paste_CreatesNewIdentifiersAndShiftsThePositions()
	{
		var document = NewDocument();
		var node = AddNode(document, Producer, 100, 50);

		var fragment = GraphFragment.Capture(document, [node.Id]);
		document.Execute(fragment.CreatePasteCommand(32, 16, out var created));

		var copy = document.Graph.GetNode(Assert.Single(created));
		Assert.NotEqual(node.Id, copy.Id);
		Assert.Equal(new NodePlacement(132, 66), document.Layout.GetPlacement(copy.Id));
	}

	[Fact]
	public void Paste_CanBeRepeatedAndIsUndoneInOneStep()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		Connect(document, Port(producer, "text"), Port(consumer, "text"));
		var fragment = GraphFragment.Capture(document, [producer.Id, consumer.Id]);

		document.Execute(fragment.CreatePasteCommand(10, 10, out _));
		document.Execute(fragment.CreatePasteCommand(20, 20, out _));
		Assert.Equal(6, document.Graph.Nodes.Count);

		document.Undo();

		Assert.Equal(4, document.Graph.Nodes.Count);
		Assert.Equal(2, document.Graph.Connections.Count);
	}

	[Fact]
	public void CopiesDoNotShareTheirSettingsWithTheOriginal()
	{
		var document = NewDocument();
		var node = AddNode(document, Configurable);
		document.Execute(new SetParameterCommand(node.Id, "url", ParameterValue.FromString("https://a")));

		var fragment = GraphFragment.Capture(document, [node.Id]);
		document.Execute(fragment.CreatePasteCommand(0, 0, out var created));
		var copy = document.Graph.GetNode(created[0]);
		document.Execute(new SetParameterCommand(copy.Id, "url", ParameterValue.FromString("https://b")));

		Assert.Equal("https://a", node.GetParameter("url").AsString());
		Assert.Equal("https://b", copy.GetParameter("url").AsString());
	}

	[Fact]
	public void SecretSettings_AreNotCopied()
	{
		var document = NewDocument();
		var node = AddNode(document, Configurable);
		document.Execute(new SetParameterCommand(node.Id, "token", ParameterValue.FromString("secret:123")));
		document.Execute(new SetParameterCommand(node.Id, "url", ParameterValue.FromString("https://a")));

		var fragment = GraphFragment.Capture(document, [node.Id]);
		document.Execute(fragment.CreatePasteCommand(0, 0, out var created));
		var copy = document.Graph.GetNode(created[0]);

		Assert.True(copy.GetParameter("token").IsNull);
		Assert.Equal("https://a", copy.GetParameter("url").AsString());
		Assert.False(node.GetParameter("token").IsNull);
	}

	[Fact]
	public void TheTitleAndTheCollapsedStateAreCopied()
	{
		var document = NewDocument();
		var node = AddNode(document, Producer);
		document.Execute(new SetTitleCommand(node.Id, "Mine"));
		document.Layout.SetCollapsed(node.Id, true);

		var fragment = GraphFragment.Capture(document, [node.Id]);
		document.Execute(fragment.CreatePasteCommand(0, 0, out var created));

		Assert.Equal("Mine", document.Graph.GetNode(created[0]).Title);
		Assert.True(document.Layout.IsCollapsed(created[0]));
	}

	[Fact]
	public void UnknownNodes_AreIgnoredAndAnEmptyFragmentIsEmpty()
	{
		var document = NewDocument();

		var fragment = GraphFragment.Capture(document, [MindMesh.Graph.Identifiers.NodeId.New()]);

		Assert.True(fragment.IsEmpty);
		Assert.Equal(0, fragment.NodeCount);
	}

	[Fact]
	public void TopLeft_IsTheCornerOfTheCopiedNodes()
	{
		var document = NewDocument();
		var first = AddNode(document, Producer, 120, 300);
		var second = AddNode(document, Consumer, 400, 80);

		var fragment = GraphFragment.Capture(document, [first.Id, second.Id]);

		Assert.Equal(new NodePlacement(120, 80), fragment.TopLeft);
	}
}
