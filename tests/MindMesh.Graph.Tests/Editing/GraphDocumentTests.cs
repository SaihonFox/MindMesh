using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using static MindMesh.Graph.Tests.Support.TestNodes;

namespace MindMesh.Graph.Tests.Editing;

public class GraphDocumentTests
{
	[Fact]
	public void AFreshDocument_HasNothingToUndoOrRedo()
	{
		var document = NewDocument();

		Assert.False(document.CanUndo);
		Assert.False(document.CanRedo);
		Assert.Null(document.UndoName);
	}

	[Fact]
	public void AddingANode_CanBeUndoneAndRedone()
	{
		var document = NewDocument();
		var node = AddNode(document, Producer, 40, 80);

		Assert.Equal(new NodePlacement(40, 80), document.Layout.GetPlacement(node.Id));

		document.Undo();
		Assert.Empty(document.Graph.Nodes);
		Assert.True(document.CanRedo);

		document.Redo();
		Assert.Same(node, Assert.Single(document.Graph.Nodes));
		Assert.Equal(new NodePlacement(40, 80), document.Layout.GetPlacement(node.Id));
	}

	[Fact]
	public void ANewCommand_ClearsTheRedoStack()
	{
		var document = NewDocument();
		AddNode(document, Producer);
		document.Undo();
		Assert.True(document.CanRedo);

		AddNode(document, Consumer);

		Assert.False(document.CanRedo);
	}

	[Fact]
	public void Connecting_CanBeUndoneAndKeepsTheConnectionIdOnRedo()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		var connection = Connect(document, Port(producer, "text"), Port(consumer, "text"));

		document.Undo();
		Assert.Empty(document.Graph.Connections);

		document.Redo();
		Assert.Equal(connection.Id, Assert.Single(document.Graph.Connections).Id);
	}

	[Fact]
	public void ReplacingAConnection_IsUndoneInOneStep()
	{
		var document = NewDocument();
		var first = AddNode(document, Producer);
		var second = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		Connect(document, Port(first, "text"), Port(consumer, "text"));
		Connect(document, Port(second, "text"), Port(consumer, "text"));

		document.Undo();

		var restored = Assert.Single(document.Graph.Connections);
		Assert.Equal(Port(first, "text"), restored.Source);
	}

	[Fact]
	public void DeletingNodes_RestoresNodesConnectionsPositionsAndCollapsedState()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer, 10, 20);
		var consumer = AddNode(document, Consumer, 300, 20);
		Connect(document, Port(producer, "text"), Port(consumer, "text"));
		document.Layout.SetCollapsed(producer.Id, true);

		document.Execute(new RemoveNodesCommand([producer.Id, consumer.Id]));
		Assert.Empty(document.Graph.Nodes);
		Assert.Empty(document.Graph.Connections);

		document.Undo();

		Assert.Equal(2, document.Graph.Nodes.Count);
		Assert.Single(document.Graph.Connections);
		Assert.Equal(new NodePlacement(10, 20), document.Layout.GetPlacement(producer.Id));
		Assert.True(document.Layout.IsCollapsed(producer.Id));
	}

	[Fact]
	public void MovingNodes_CanBeUndone()
	{
		var document = NewDocument();
		var node = AddNode(document, Producer, 0, 0);

		document.Execute(new MoveNodesCommand([new NodeMove(node.Id, new NodePlacement(0, 0), new NodePlacement(64, 32))]));
		Assert.Equal(new NodePlacement(64, 32), document.Layout.GetPlacement(node.Id));

		document.Undo();
		Assert.Equal(new NodePlacement(0, 0), document.Layout.GetPlacement(node.Id));
	}

	[Fact]
	public void ConsecutiveChangesOfOneSetting_AreOneUndoStep()
	{
		var document = NewDocument();
		var node = AddNode(document, Configurable);
		var before = node.GetParameter("url");

		document.Execute(new SetParameterCommand(node.Id, "url", ParameterValue.FromString("h")));
		document.Execute(new SetParameterCommand(node.Id, "url", ParameterValue.FromString("ht")));
		document.Execute(new SetParameterCommand(node.Id, "url", ParameterValue.FromString("http")));

		Assert.Equal("http", node.GetParameter("url").AsString());

		document.Undo();

		Assert.Equal(before, node.GetParameter("url"));
		Assert.Equal("Add node", document.UndoName);
	}

	[Fact]
	public void ChangesOfDifferentSettings_AreSeparateUndoSteps()
	{
		var document = NewDocument();
		var node = AddNode(document, Configurable);

		document.Execute(new SetParameterCommand(node.Id, "url", ParameterValue.FromString("a")));
		document.Execute(new SetParameterCommand(node.Id, "count", ParameterValue.FromNumber(5)));

		document.Undo();

		Assert.Equal("a", node.GetParameter("url").AsString());
		Assert.Equal(3m, node.GetParameter("count").AsDecimal());
	}

	[Fact]
	public void TitleAndDisabledChanges_CanBeUndone()
	{
		var document = NewDocument();
		var node = AddNode(document, Producer);

		document.Execute(new SetTitleCommand(node.Id, "Mine"));
		document.Execute(new SetDisabledCommand([node.Id], true));

		Assert.Equal("Mine", node.Title);
		Assert.True(node.IsDisabled);

		document.Undo();
		Assert.False(node.IsDisabled);

		document.Undo();
		Assert.Null(node.Title);
	}

	[Fact]
	public void CompositeCommand_IsUndoneAsOneStep()
	{
		var document = NewDocument();
		var producer = NodeInstance.Create(Producer);
		var consumer = NodeInstance.Create(Consumer);

		document.Execute(new CompositeCommand("Both",
		[
			new AddNodeCommand(producer, new NodePlacement(0, 0)),
			new AddNodeCommand(consumer, new NodePlacement(300, 0)),
		]));

		Assert.Equal(2, document.Graph.Nodes.Count);

		document.Undo();

		Assert.Empty(document.Graph.Nodes);
		Assert.False(document.CanUndo);
	}

	[Fact]
	public void HistoryIsLimited()
	{
		var document = new GraphDocument(historyLimit: 3);

		for (int index = 0; index < 5; index++)
			AddNode(document, Producer);

		int undone = 0;

		while (document.CanUndo)
		{
			document.Undo();
			undone++;
		}

		Assert.Equal(3, undone);
		Assert.Equal(2, document.Graph.Nodes.Count);
	}

	[Fact]
	public void HistoryChanged_IsRaisedForEveryStep()
	{
		var document = NewDocument();
		int count = 0;
		document.HistoryChanged += (_, _) => count++;

		AddNode(document, Producer);
		document.Undo();
		document.Redo();
		document.ClearHistory();

		Assert.Equal(4, count);
		Assert.False(document.CanUndo);
	}

	[Fact]
	public void TheGraphRaisesAChangeForEveryEdit()
	{
		var document = NewDocument();
		var changes = new List<GraphChange>();
		document.Graph.Changed += (_, e) => changes.Add(e.Change);

		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		Connect(document, Port(producer, "text"), Port(consumer, "text"));

		Assert.IsType<NodeAdded>(changes[0]);
		Assert.IsType<NodeAdded>(changes[1]);
		Assert.IsType<ConnectionAdded>(changes[2]);
	}

	[Fact]
	public void SettingTheSameValueAgain_RaisesNoChange()
	{
		var document = NewDocument();
		var node = AddNode(document, Configurable);
		int count = 0;
		document.Graph.Changed += (_, _) => count++;

		document.Graph.SetParameter(node.Id, "count", ParameterValue.FromNumber(3));

		Assert.Equal(0, count);
	}
}
