using System.Text.Json.Nodes;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Serialization;
using MindMesh.Graph.Tests.Support;

namespace MindMesh.Graph.Tests.Serialization;

public class WorkflowSerializerTests
{
	private static readonly Dictionary<string, INodeDefinition> Definitions = new[]
	{
		TestNodes.Producer, TestNodes.Consumer, TestNodes.Configurable,
	}.ToDictionary(definition => definition.Id.Value, definition => (INodeDefinition)definition);

	private static WorkflowSerializer CreateSerializer(WorkflowMigrator? migrator = null) =>
		new(id => Definitions.GetValueOrDefault(id.Value), migrator);

	private static WorkflowContent Sample(out GraphDocument document, out NodeInstance producer, out NodeInstance consumer)
	{
		document = TestNodes.NewDocument();
		producer = TestNodes.AddNode(document, TestNodes.Producer, 40, 80);
		consumer = TestNodes.AddNode(document, TestNodes.Consumer, 400, 120.5);
		TestNodes.Connect(document, TestNodes.Port(producer, "text"), TestNodes.Port(consumer, "text"));
		TestNodes.Connect(document, TestNodes.Port(producer, "exec"), TestNodes.Port(consumer, "exec"));

		return new WorkflowContent(Guid.CreateVersion7(), "Sample", "A workflow for tests", document.Graph, document.Layout);
	}

	[Fact]
	public void ARoundTrip_KeepsNodesConnectionsAndPositions()
	{
		var content = Sample(out _, out var producer, out var consumer);
		var serializer = CreateSerializer();

		var result = serializer.Deserialize(serializer.Serialize(content));

		Assert.True(result.Succeeded);
		Assert.Empty(result.Problems);
		var loaded = result.Content!;
		Assert.Equal(content.Id, loaded.Id);
		Assert.Equal("Sample", loaded.Name);
		Assert.Equal("A workflow for tests", loaded.Description);
		Assert.Equal(2, loaded.Graph.Nodes.Count);
		Assert.Equal(2, loaded.Graph.Connections.Count);
		Assert.Equal(new NodePlacement(40, 80), loaded.Layout.GetPlacement(producer.Id));
		Assert.Equal(new NodePlacement(400, 120.5), loaded.Layout.GetPlacement(consumer.Id));
		Assert.Equal(
			content.Graph.Connections.Select(connection => connection.Id).ToArray(),
			loaded.Graph.Connections.Select(connection => connection.Id).ToArray());
	}

	[Fact]
	public void ARoundTrip_KeepsTitlesFlagsAndSettings()
	{
		var content = Sample(out var document, out _, out _);
		var configurable = TestNodes.AddNode(document, TestNodes.Configurable);
		document.Graph.SetTitle(configurable.Id, "My request");
		document.Graph.SetDisabled(configurable.Id, true);
		document.Graph.SetParameter(configurable.Id, "url", ParameterValue.FromString("https://example.com/Привет"));
		document.Graph.SetParameter(configurable.Id, "count", ParameterValue.FromNumber(7));
		document.Layout.SetCollapsed(configurable.Id, true);
		var serializer = CreateSerializer();

		var loaded = serializer.Deserialize(serializer.Serialize(content)).Content!;
		var copy = loaded.Graph.GetNode(configurable.Id);

		Assert.Equal("My request", copy.Title);
		Assert.True(copy.IsDisabled);
		Assert.Equal("https://example.com/Привет", copy.GetParameter("url").AsString());
		Assert.Equal(7m, copy.GetParameter("count").AsDecimal());
		Assert.True(loaded.Layout.IsCollapsed(configurable.Id));
		Assert.Equal("A", copy.GetParameter("mode").AsString());
	}

	[Fact]
	public void ACustomNodeSize_SurvivesARoundTrip_AndNodesWithoutOneStayDefault()
	{
		var content = Sample(out var document, out var first, out var second);
		document.Layout.SetSize(first.Id, new NodeSize(400, 260));
		var serializer = CreateSerializer();

		var loaded = serializer.Deserialize(serializer.Serialize(content)).Content!;

		Assert.Equal(new NodeSize(400, 260), loaded.Layout.GetSize(first.Id));
		Assert.Null(loaded.Layout.GetSize(second.Id));
	}

	[Fact]
	public void TheOutput_IsTheSameEveryTime_AndUsesUnixNewlines()
	{
		var content = Sample(out _, out _, out _);
		var serializer = CreateSerializer();

		var first = serializer.Serialize(content);
		var second = serializer.Serialize(serializer.Deserialize(first).Content!);

		Assert.Equal(first, second);
		Assert.DoesNotContain('\r', first);
		Assert.EndsWith("\n", first);
		Assert.Contains("\t\"nodes\": [", first);
		Assert.Contains("\"formatVersion\": 1", first);
	}

	[Fact]
	public void Settings_AreWrittenInKeyOrder()
	{
		var content = Sample(out var document, out _, out _);
		var configurable = TestNodes.AddNode(document, TestNodes.Configurable);
		document.Graph.SetParameter(configurable.Id, "url", ParameterValue.FromString("x"));
		var serializer = CreateSerializer();

		var text = serializer.Serialize(content);

		var keys = JsonNode.Parse(text)!["nodes"]!.AsArray()
			.Single(node => node!["id"]!.GetValue<string>() == configurable.Id.ToString())!["parameters"]!.AsObject()
			.Select(pair => pair.Key)
			.ToArray();
		Assert.Equal(keys.Order(StringComparer.Ordinal).ToArray(), keys);
	}

	[Fact]
	public void ASecretIsKeptAsAReference_NeverAsAValue()
	{
		var content = Sample(out var document, out _, out _);
		var configurable = TestNodes.AddNode(document, TestNodes.Configurable);
		document.Graph.SetParameter(configurable.Id, "token", ParameterValue.FromString("secret:0f0f0f0f-0000-0000-0000-000000000001"));
		var serializer = CreateSerializer();

		var text = serializer.Serialize(content);
		var loaded = serializer.Deserialize(text).Content!;

		Assert.Contains("secret:0f0f0f0f", text);
		Assert.Equal(
			"secret:0f0f0f0f-0000-0000-0000-000000000001",
			loaded.Graph.GetNode(configurable.Id).GetParameter("token").AsString());
	}

	[Fact]
	public void SettingsThatAreNotValidJson_AreKeptAsText()
	{
		var content = Sample(out var document, out _, out _);
		var configurable = TestNodes.AddNode(document, TestNodes.Configurable);
		document.Graph.SetParameter(configurable.Id, "body", new ParameterValue("{ not json"));
		var serializer = CreateSerializer();

		var loaded = serializer.Deserialize(serializer.Serialize(content)).Content!;

		Assert.Equal("{ not json", loaded.Graph.GetNode(configurable.Id).GetParameter("body").AsString());
	}

	[Fact]
	public void AnEmptyWorkflow_RoundTrips()
	{
		var serializer = CreateSerializer();
		var empty = WorkflowContent.CreateEmpty("Empty");

		var result = serializer.Deserialize(serializer.Serialize(empty));

		Assert.True(result.Succeeded);
		Assert.Empty(result.Content!.Graph.Nodes);
		Assert.Equal("Empty", result.Content.Name);
		Assert.Null(result.Content.Description);
	}

	[Fact]
	public void AFileWithCommentsAndTrailingCommas_IsAccepted()
	{
		var serializer = CreateSerializer();
		const string text = """
			{
				// written by hand
				"format": "mindmesh.workflow",
				"formatVersion": 1,
				"name": "Hand made",
				"nodes": [],
				"connections": [],
			}
			""";

		var result = serializer.Deserialize(text);

		Assert.True(result.Succeeded);
		Assert.Equal("Hand made", result.Content!.Name);
	}

	[Theory]
	[InlineData("this is not json", WorkflowProblemCode.NotJson)]
	[InlineData("[1, 2, 3]", WorkflowProblemCode.NotAWorkflow)]
	[InlineData("{ \"hello\": \"world\" }", WorkflowProblemCode.NotAWorkflow)]
	[InlineData("{ \"format\": \"mindmesh.workflow\" }", WorkflowProblemCode.MissingFormatVersion)]
	[InlineData("{ \"format\": \"mindmesh.workflow\", \"formatVersion\": 0 }", WorkflowProblemCode.MissingFormatVersion)]
	[InlineData("{ \"format\": \"mindmesh.workflow\", \"formatVersion\": \"one\" }", WorkflowProblemCode.MissingFormatVersion)]
	[InlineData("{ \"format\": \"mindmesh.workflow\", \"formatVersion\": 99 }", WorkflowProblemCode.NewerFormat)]
	public void AFileThatCannotBeRead_IsReportedAsFatal(string text, WorkflowProblemCode expected)
	{
		var result = CreateSerializer().Deserialize(text);

		Assert.False(result.Succeeded);
		var problem = Assert.Single(result.Problems);
		Assert.Equal(expected, problem.Code);
		Assert.Equal(WorkflowProblemSeverity.Fatal, problem.Severity);
	}

	[Fact]
	public void ANodeOfAnUnknownType_IsSkipped_AndItsConnectionsGoWithIt()
	{
		var content = Sample(out _, out var producer, out _);
		var serializer = CreateSerializer();
		var root = JsonNode.Parse(serializer.Serialize(content))!.AsObject();
		var ghostId = Guid.NewGuid().ToString();
		root["nodes"]!.AsArray().Add(new JsonObject { ["id"] = ghostId, ["type"] = "plugin.missing", ["x"] = 0, ["y"] = 0 });
		root["connections"]!.AsArray().Add(new JsonObject
		{
			["id"] = Guid.NewGuid().ToString(),
			["from"] = new JsonObject { ["node"] = producer.Id.ToString(), ["port"] = "text" },
			["to"] = new JsonObject { ["node"] = ghostId, ["port"] = "input" },
		});

		var result = serializer.Deserialize(root.ToJsonString());

		Assert.True(result.Succeeded);
		Assert.True(result.IsLossy);
		Assert.Equal(2, result.Content!.Graph.Nodes.Count);
		Assert.Contains(result.Problems, problem => problem.Code == WorkflowProblemCode.UnknownNodeType && problem.Detail == "plugin.missing");
		Assert.Contains(result.Problems, problem => problem.Code == WorkflowProblemCode.ConnectionDropped);
	}

	[Fact]
	public void ADamagedConnection_IsRejectedWithItsReason()
	{
		var content = Sample(out _, out var producer, out var consumer);
		var serializer = CreateSerializer();
		var root = JsonNode.Parse(serializer.Serialize(content))!.AsObject();
		var connections = root["connections"]!.AsArray();
		connections.Clear();
		connections.Add(new JsonObject
		{
			["id"] = Guid.NewGuid().ToString(),
			["from"] = new JsonObject { ["node"] = producer.Id.ToString(), ["port"] = "integer" },
			["to"] = new JsonObject { ["node"] = consumer.Id.ToString(), ["port"] = "event" },
		});
		connections.Add(new JsonObject
		{
			["id"] = Guid.NewGuid().ToString(),
			["from"] = new JsonObject { ["node"] = consumer.Id.ToString(), ["port"] = "text" },
			["to"] = new JsonObject { ["node"] = producer.Id.ToString(), ["port"] = "text" },
		});
		connections.Add(new JsonObject { ["id"] = "not a guid" });

		var result = serializer.Deserialize(root.ToJsonString());

		Assert.Empty(result.Content!.Graph.Connections);
		Assert.Equal(3, result.Problems.Count(problem => problem.Severity == WorkflowProblemSeverity.Error));
		Assert.Contains(result.Problems, problem => problem.Detail == "TypeMismatch");
		Assert.Contains(result.Problems, problem => problem.Code == WorkflowProblemCode.InvalidConnection);
	}

	[Fact]
	public void TwoConnectionsIntoOneSingleInput_KeepTheFirst_AndReportTheSecond()
	{
		var document = TestNodes.NewDocument();
		var first = TestNodes.AddNode(document, TestNodes.Producer);
		var second = TestNodes.AddNode(document, TestNodes.Producer);
		var target = TestNodes.AddNode(document, TestNodes.Consumer);
		TestNodes.Connect(document, TestNodes.Port(first, "text"), TestNodes.Port(target, "text"));
		var content = new WorkflowContent(Guid.NewGuid(), "Conflict", null, document.Graph, document.Layout);
		var serializer = CreateSerializer();
		var root = JsonNode.Parse(serializer.Serialize(content))!.AsObject();
		root["connections"]!.AsArray().Add(new JsonObject
		{
			["id"] = Guid.NewGuid().ToString(),
			["from"] = new JsonObject { ["node"] = second.Id.ToString(), ["port"] = "text" },
			["to"] = new JsonObject { ["node"] = target.Id.ToString(), ["port"] = "text" },
		});

		var result = serializer.Deserialize(root.ToJsonString());

		var kept = Assert.Single(result.Content!.Graph.Connections);
		Assert.Equal(first.Id, kept.Source.Node);
		Assert.Contains(result.Problems, problem => problem.Detail == "PortOccupied");
	}

	[Fact]
	public void DuplicateAndBrokenIds_AreReported()
	{
		var content = Sample(out _, out var producer, out _);
		var serializer = CreateSerializer();
		var root = JsonNode.Parse(serializer.Serialize(content))!.AsObject();
		var nodes = root["nodes"]!.AsArray();
		nodes.Add(new JsonObject { ["id"] = producer.Id.ToString(), ["type"] = "test.producer" });
		nodes.Add(new JsonObject { ["id"] = "nonsense", ["type"] = "test.producer" });
		nodes.Add(JsonValue.Create(5));
		root["connections"]!.AsArray().Add(root["connections"]![0]!.DeepClone());

		var result = serializer.Deserialize(root.ToJsonString());

		Assert.Equal(2, result.Content!.Graph.Nodes.Count);
		Assert.Single(result.Problems, problem => problem.Code == WorkflowProblemCode.DuplicateNodeId);
		Assert.Equal(2, result.Problems.Count(problem => problem.Code == WorkflowProblemCode.InvalidNodeId));
		Assert.Single(result.Problems, problem => problem.Code == WorkflowProblemCode.DuplicateConnectionId);
	}

	[Fact]
	public void ASettingTheNodeDoesNotKnow_IsKeptWithAWarning()
	{
		var content = Sample(out _, out _, out var consumer);
		var serializer = CreateSerializer();
		var root = JsonNode.Parse(serializer.Serialize(content))!.AsObject();
		var node = root["nodes"]!.AsArray().Single(item => item!["id"]!.GetValue<string>() == consumer.Id.ToString())!.AsObject();
		node["parameters"] = new JsonObject { ["retired"] = "still here" };

		var result = serializer.Deserialize(root.ToJsonString());

		Assert.False(result.IsLossy);
		Assert.Contains(result.Problems, problem => problem.Code == WorkflowProblemCode.UnknownParameter && problem.Detail == "retired");
		Assert.Equal("still here", result.Content!.Graph.GetNode(consumer.Id).GetParameter("retired").AsString());
	}

	[Fact]
	public void ANodeWrittenByANewerDefinition_IsLoadedWithAWarning()
	{
		var content = Sample(out _, out var producer, out _);
		var serializer = CreateSerializer();
		var root = JsonNode.Parse(serializer.Serialize(content))!.AsObject();
		root["nodes"]!.AsArray().Single(item => item!["id"]!.GetValue<string>() == producer.Id.ToString())!["typeVersion"] = "9.0";

		var result = serializer.Deserialize(root.ToJsonString());

		Assert.Equal(2, result.Content!.Graph.Nodes.Count);
		Assert.Contains(result.Problems, problem => problem.Code == WorkflowProblemCode.NodeVersionNewer);
		Assert.False(result.IsLossy);
	}

	[Fact]
	public void AMissingWorkflowId_GetsANewOne()
	{
		var serializer = CreateSerializer();
		const string text = "{ \"format\": \"mindmesh.workflow\", \"formatVersion\": 1, \"nodes\": [], \"connections\": [] }";

		var result = serializer.Deserialize(text);

		Assert.NotEqual(Guid.Empty, result.Content!.Id);
		Assert.Contains(result.Problems, problem => problem.Code == WorkflowProblemCode.InvalidWorkflowId);
		Assert.Equal(string.Empty, result.Content.Name);
	}

	[Fact]
	public void ALoadedGraph_CanBeEditedAndUndoneLikeAnyOther()
	{
		var content = Sample(out _, out _, out _);
		var serializer = CreateSerializer();
		var loaded = serializer.Deserialize(serializer.Serialize(content)).Content!;
		var document = new GraphDocument(loaded.Graph, loaded.Layout);

		var target = loaded.Graph.Nodes[1];
		document.Execute(new RemoveNodesCommand([target.Id]));
		document.Undo();

		Assert.Equal(2, loaded.Graph.Nodes.Count);
		Assert.Equal(2, loaded.Graph.Connections.Count);
	}
}
