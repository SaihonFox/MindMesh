using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Ports;
using MindMesh.Graph.Tests.Support;
using static MindMesh.Graph.Tests.Support.TestNodes;

namespace MindMesh.Graph.Tests.Model;

public class ConnectionRulesTests
{
	[Fact]
	public void OutputToMatchingInput_IsAllowed()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		var check = document.Graph.CheckConnection(Port(producer, "text"), Port(consumer, "text"));

		Assert.True(check.IsAllowed);
		Assert.False(check.RequiresConversion);
		Assert.Empty(check.ReplacedConnections);
	}

	[Fact]
	public void TheOrderOfThePortsDoesNotMatter()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		var forward = document.Graph.CheckConnection(Port(producer, "text"), Port(consumer, "text"));
		var backward = document.Graph.CheckConnection(Port(consumer, "text"), Port(producer, "text"));

		Assert.True(forward.IsAllowed);
		Assert.True(backward.IsAllowed);
	}

	[Fact]
	public void ConnectingInTheOppositeOrderStillCreatesAnOutputToInputConnection()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		var connection = Connect(document, Port(consumer, "text"), Port(producer, "text"));

		Assert.Equal(Port(producer, "text"), connection.Source);
		Assert.Equal(Port(consumer, "text"), connection.Target);
	}

	[Fact]
	public void ConversionBetweenDifferentTypes_IsFlagged()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		var check = document.Graph.CheckConnection(Port(producer, "integer"), Port(consumer, "number"));

		Assert.True(check.IsAllowed);
		Assert.True(check.RequiresConversion);
	}

	[Fact]
	public void AnyOutput_FitsAnyDataInput_WithoutConversion()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		var check = document.Graph.CheckConnection(Port(producer, "any"), Port(consumer, "json"));

		Assert.True(check.IsAllowed);
		Assert.False(check.RequiresConversion);
	}

	[Fact]
	public void IncompatibleTypes_AreRejected()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		var check = document.Graph.CheckConnection(Port(producer, "text"), Port(consumer, "number"));

		Assert.Equal(ConnectionRejection.TypeMismatch, check.Rejection);
	}

	[Fact]
	public void ExecutionAndDataPorts_NeverConnect()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		Assert.Equal(
			ConnectionRejection.TypeMismatch,
			document.Graph.CheckConnection(Port(producer, "exec"), Port(consumer, "text")).Rejection);
		Assert.Equal(
			ConnectionRejection.TypeMismatch,
			document.Graph.CheckConnection(Port(producer, "event"), Port(consumer, "exec")).Rejection);
	}

	[Fact]
	public void TwoOutputs_AreRejected()
	{
		var document = NewDocument();
		var first = AddNode(document, Producer);
		var second = AddNode(document, Producer);

		var check = document.Graph.CheckConnection(Port(first, "text"), Port(second, "text"));

		Assert.Equal(ConnectionRejection.WrongDirection, check.Rejection);
	}

	[Fact]
	public void TwoInputs_AreRejected()
	{
		var document = NewDocument();
		var first = AddNode(document, Consumer);
		var second = AddNode(document, Consumer);

		var check = document.Graph.CheckConnection(Port(first, "text"), Port(second, "text"));

		Assert.Equal(ConnectionRejection.WrongDirection, check.Rejection);
	}

	[Fact]
	public void ANodeCannotConnectToItself()
	{
		var document = NewDocument();
		var consumer = AddNode(document, Consumer);

		var check = document.Graph.CheckConnection(Port(consumer, "text"), Port(consumer, "text"));

		Assert.Equal(ConnectionRejection.SameNode, check.Rejection);
	}

	[Fact]
	public void UnknownPortsAndNodes_AreRejected()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		Assert.Equal(
			ConnectionRejection.UnknownPort,
			document.Graph.CheckConnection(Port(producer, "nothing"), Port(consumer, "text")).Rejection);
		Assert.Equal(
			ConnectionRejection.UnknownPort,
			document.Graph.CheckConnection(new PortReference(MindMesh.Graph.Identifiers.NodeId.New(), "text"), Port(consumer, "text")).Rejection);
	}

	[Fact]
	public void TheSameConnectionTwice_IsADuplicate()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		Connect(document, Port(producer, "text"), Port(consumer, "many"));

		var check = document.Graph.CheckConnection(Port(producer, "text"), Port(consumer, "many"));

		Assert.Equal(ConnectionRejection.Duplicate, check.Rejection);
	}

	[Fact]
	public void ASingleInput_ReplacesItsOldConnection()
	{
		var document = NewDocument();
		var first = AddNode(document, Producer);
		var second = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		var old = Connect(document, Port(first, "text"), Port(consumer, "text"));

		var check = document.Graph.CheckConnection(Port(second, "text"), Port(consumer, "text"));

		Assert.True(check.IsAllowed);
		Assert.Equal(old.Id, Assert.Single(check.ReplacedConnections));

		Connect(document, Port(second, "text"), Port(consumer, "text"));

		var remaining = Assert.Single(document.Graph.Connections);
		Assert.Equal(Port(second, "text"), remaining.Source);
	}

	[Fact]
	public void AnInputThatAllowsSeveralConnections_KeepsThemAll()
	{
		var document = NewDocument();
		var first = AddNode(document, Producer);
		var second = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		Connect(document, Port(first, "text"), Port(consumer, "many"));
		Connect(document, Port(second, "text"), Port(consumer, "many"));

		Assert.Equal(2, document.Graph.GetConnections(Port(consumer, "many")).Count);
	}

	[Fact]
	public void ExecutionInputs_AcceptSeveralConnectionsByDefault()
	{
		var document = NewDocument();
		var first = AddNode(document, Producer);
		var second = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		Connect(document, Port(first, "exec"), Port(consumer, "exec"));
		Connect(document, Port(second, "exec"), Port(consumer, "exec"));

		Assert.Equal(2, document.Graph.GetConnections(Port(consumer, "exec")).Count);
	}

	[Fact]
	public void ACycleIsRejected()
	{
		var document = NewDocument();
		var first = AddNode(document, Consumer);
		var second = AddNode(document, Consumer);
		Connect(document, Port(first, "done"), Port(second, "exec"));

		var check = document.Graph.CheckConnection(Port(second, "done"), Port(first, "exec"));

		Assert.Equal(ConnectionRejection.Cycle, check.Rejection);
	}

	[Fact]
	public void ALongerCycleIsRejectedToo()
	{
		var document = NewDocument();
		var a = AddNode(document, Consumer);
		var b = AddNode(document, Consumer);
		var c = AddNode(document, Consumer);
		Connect(document, Port(a, "done"), Port(b, "exec"));
		Connect(document, Port(b, "done"), Port(c, "exec"));

		Assert.Equal(ConnectionRejection.Cycle, document.Graph.CheckConnection(Port(c, "done"), Port(a, "exec")).Rejection);
		Assert.True(document.Graph.IsReachable(a.Id, c.Id));
		Assert.False(document.Graph.IsReachable(c.Id, a.Id));
	}

	[Fact]
	public void ConnectCommand_ThrowsWhenTheConnectionIsNotAllowed()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		Assert.Throws<InvalidOperationException>(() =>
			document.Execute(new ConnectCommand(Port(producer, "text"), Port(consumer, "number"))));
	}

	[Fact]
	public void RemovingANode_RemovesItsConnections()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);
		Connect(document, Port(producer, "text"), Port(consumer, "text"));

		document.Execute(new RemoveNodesCommand([producer.Id]));

		Assert.Empty(document.Graph.Connections);
		Assert.Empty(document.Graph.GetConnections(Port(consumer, "text")));
		Assert.Single(document.Graph.Nodes);
	}
}
