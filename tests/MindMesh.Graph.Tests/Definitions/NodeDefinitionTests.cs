using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;
using static MindMesh.Graph.Tests.Support.TestNodes;

namespace MindMesh.Graph.Tests.Definitions;

public class NodeDefinitionTests
{
	[Fact]
	public void TheBuilder_RecordsEverythingItIsGiven()
	{
		var definition = NodeDefinition.Create("test.full", "Full", Category)
			.Describe("A node.")
			.Icon("bolt")
			.Keywords("one", "two")
			.In("in", "In", PortType.String, required: true)
			.Out("out", "Out", PortType.Json)
			.Param(Param.Text("name", "Name"))
			.Build();

		Assert.Equal("test.full", definition.Id.Value);
		Assert.Equal("Full", definition.Name);
		Assert.Equal("A node.", definition.Description);
		Assert.Equal("bolt", definition.IconKey);
		string[] keywords = ["one", "two"];
		Assert.Equal(keywords, definition.Keywords.ToArray());
		Assert.Single(definition.Inputs);
		Assert.Single(definition.Outputs);
		Assert.Single(definition.Parameters);
		Assert.Equal(Category, Assert.Single(definition.Categories));
	}

	[Fact]
	public void ANodeCanLiveInSeveralCategories()
	{
		var other = new MindMesh.Graph.Identifiers.NodeCategoryId("Other");

		var definition = NodeDefinition.Create("test.two", "Two", Category, other).Build();

		MindMesh.Graph.Identifiers.NodeCategoryId[] categories = [Category, other];
		Assert.Equal(categories, definition.Categories.ToArray());
	}

	[Fact]
	public void DuplicatePortKeys_AreRejected() =>
		Assert.Throws<InvalidOperationException>(() => NodeDefinition.Create("test.dup", "Dup", Category)
			.In("same", "A")
			.Out("same", "B")
			.Build());

	[Fact]
	public void DuplicateSettingKeys_AreRejected() =>
		Assert.Throws<InvalidOperationException>(() => NodeDefinition.Create("test.dup", "Dup", Category)
			.Param(Param.Text("same", "A"))
			.Param(Param.Text("same", "B"))
			.Build());

	[Fact]
	public void AnInputAndAnOutputCannotShareAKey()
	{
		// A PortReference holds only the key, so a key must name one port of the node.
		Assert.Throws<InvalidOperationException>(() => NodeDefinition.Create("test.same", "Same", Category)
			.In("x", "In")
			.Out("x", "Out")
			.Build());
	}

	[Fact]
	public void DataInputsAcceptOneConnection_ExecutionAndEventInputsSeveral()
	{
		Assert.False(Consumer.FindPort("text", PortDirection.Input)!.AllowMultiple);
		Assert.True(Consumer.FindPort("exec", PortDirection.Input)!.AllowMultiple);
		Assert.True(Consumer.FindPort("event", PortDirection.Input)!.AllowMultiple);
		Assert.True(Consumer.FindPort("many", PortDirection.Input)!.AllowMultiple);
	}

	[Fact]
	public void FindPort_LooksOnlyInTheGivenDirection()
	{
		Assert.NotNull(Consumer.FindPort("text", PortDirection.Input));
		Assert.Null(Consumer.FindPort("text", PortDirection.Output));
		Assert.NotNull(Consumer.FindPort("result", PortDirection.Output));
		Assert.Null(Consumer.FindPort("number", PortDirection.Output));
		Assert.Null(Consumer.FindPort("missing", PortDirection.Input));
	}

	[Fact]
	public void ASettingCanDependOnAnother()
	{
		var body = Configurable.Parameters.Single(parameter => parameter.Key == "body");
		var modeA = new Dictionary<string, ParameterValue> { ["mode"] = ParameterValue.FromString("A") };
		var modeB = new Dictionary<string, ParameterValue> { ["mode"] = ParameterValue.FromString("B") };

		Assert.False(body.IsVisible(modeA));
		Assert.True(body.IsVisible(modeB));
		Assert.False(body.IsVisible(new Dictionary<string, ParameterValue>()));
	}

	[Fact]
	public void ANewInstance_StartsWithTheDefaults()
	{
		var node = MindMesh.Graph.Model.NodeInstance.Create(Configurable);

		Assert.Equal(3m, node.GetParameter("count").AsDecimal());
		Assert.Equal("A", node.GetParameter("mode").AsString());
		Assert.True(node.GetParameter("url").IsNull);
	}
}
