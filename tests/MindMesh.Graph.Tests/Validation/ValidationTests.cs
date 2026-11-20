using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Model;
using MindMesh.Graph.Validation;
using static MindMesh.Graph.Tests.Support.TestNodes;

namespace MindMesh.Graph.Tests.Validation;

public class ValidationTests
{
	private static ParameterDefinition Setting(string key) =>
		Configurable.Parameters.Single(parameter => parameter.Key == key);

	[Fact]
	public void ARequiredSettingThatIsMissingOrBlank_IsReported()
	{
		var url = Setting("url");

		Assert.Equal(ValidationCode.RequiredParameterMissing, ParameterValidator.Validate(url, ParameterValue.Null)!.Value.Code);
		Assert.Equal(ValidationCode.RequiredParameterMissing, ParameterValidator.Validate(url, ParameterValue.FromString("  "))!.Value.Code);
		Assert.Null(ParameterValidator.Validate(url, ParameterValue.FromString("https://a")));
	}

	[Fact]
	public void AnOptionalEmptySetting_IsFine() =>
		Assert.Null(ParameterValidator.Validate(Param.Text("note", "Note"), ParameterValue.Null));

	[Fact]
	public void NumbersAreCheckedAgainstTheirRange()
	{
		var count = Setting("count");

		Assert.Null(ParameterValidator.Validate(count, ParameterValue.FromNumber(5)));
		Assert.Equal(ValidationCode.NumberTooSmall, ParameterValidator.Validate(count, ParameterValue.FromNumber(0))!.Value.Code);
		Assert.Equal("1", ParameterValidator.Validate(count, ParameterValue.FromNumber(0))!.Value.Detail);
		Assert.Equal(ValidationCode.NumberTooLarge, ParameterValidator.Validate(count, ParameterValue.FromNumber(11))!.Value.Code);
		Assert.Equal("10", ParameterValidator.Validate(count, ParameterValue.FromNumber(11))!.Value.Detail);
		Assert.Equal(ValidationCode.InvalidNumber, ParameterValidator.Validate(count, ParameterValue.FromString("many"))!.Value.Code);
	}

	[Fact]
	public void JsonSettings_MustBeValidJson()
	{
		var body = Setting("body");

		Assert.Null(ParameterValidator.Validate(body, ParameterValue.FromString("{\"a\":1}")));
		Assert.Equal(ValidationCode.InvalidJson, ParameterValidator.Validate(body, ParameterValue.FromString("{oops"))!.Value.Code);
	}

	[Fact]
	public void ASelectedOption_MustBeInTheList()
	{
		var mode = Setting("mode");

		Assert.Null(ParameterValidator.Validate(mode, ParameterValue.FromString("B")));
		Assert.Equal(ValidationCode.UnknownOption, ParameterValidator.Validate(mode, ParameterValue.FromString("Z"))!.Value.Code);
	}

	[Fact]
	public void SettingsThatAreNotVisible_AreNotValidated()
	{
		var document = NewDocument();
		var node = AddNode(document, Configurable);
		document.Execute(new SetParameterCommand(node.Id, "url", ParameterValue.FromString("https://a")));
		document.Execute(new SetParameterCommand(node.Id, "token", ParameterValue.FromString("secret:1")));
		document.Execute(new SetParameterCommand(node.Id, "body", ParameterValue.FromString("{broken")));

		// "body" only matters in mode B, and the node is in mode A.
		Assert.Empty(NodeValidator.ValidateParameters(Configurable, node));

		document.Execute(new SetParameterCommand(node.Id, "mode", ParameterValue.FromString("B")));

		var issue = Assert.Single(NodeValidator.ValidateParameters(Configurable, node));
		Assert.Equal(ValidationCode.InvalidJson, issue.Code);
		Assert.Equal("body", issue.Key);
	}

	[Fact]
	public void ANewNode_ReportsItsMissingRequiredSettings()
	{
		var node = NodeInstance.Create(Configurable);

		var keys = NodeValidator.ValidateParameters(Configurable, node).Select(issue => issue.Key).Order().ToArray();

		string[] expected = ["token", "url"];
		Assert.Equal(expected, keys);
	}

	[Fact]
	public void ADisabledNode_IsNotValidated()
	{
		var node = NodeInstance.Create(Configurable);
		node.IsDisabled = true;

		Assert.Empty(NodeValidator.ValidateParameters(Configurable, node));
		Assert.Empty(NodeValidator.Validate(node, new NodeGraph()));
	}

	[Fact]
	public void ARequiredInputThatIsNotConnected_IsAWarning()
	{
		var document = NewDocument();
		var consumer = AddNode(document, Consumer);

		var issues = NodeValidator.Validate(consumer, document.Graph);

		string[] expected = ["exec", "text"];
		Assert.Equal(expected, issues.Select(issue => issue.Key).Order().ToArray());
		Assert.All(issues, issue =>
		{
			Assert.Equal(ValidationCode.RequiredInputNotConnected, issue.Code);
			Assert.Equal(ValidationSeverity.Warning, issue.Severity);
		});
	}

	[Fact]
	public void ConnectingARequiredInput_ClearsItsWarning()
	{
		var document = NewDocument();
		var producer = AddNode(document, Producer);
		var consumer = AddNode(document, Consumer);

		Connect(document, Port(producer, "text"), Port(consumer, "text"));

		var issue = Assert.Single(NodeValidator.Validate(consumer, document.Graph));
		Assert.Equal("exec", issue.Key);
	}

	[Fact]
	public void TheGraphValidator_CollectsTheIssuesOfEveryNode()
	{
		var document = NewDocument();
		AddNode(document, Consumer);
		AddNode(document, Consumer);

		var issues = GraphValidator.Validate(document.Graph);

		// two required inputs per node, two nodes with no connection, and no trigger for the graph
		Assert.Equal(4, issues.Count(issue => issue.Code == ValidationCode.RequiredInputNotConnected));
		Assert.Equal(2, issues.Count(issue => issue.Code == ValidationCode.NodeNotConnected));
		Assert.Single(issues, issue => issue.Code == ValidationCode.GraphHasNoTrigger && issue.IsGraphLevel);
	}
}
