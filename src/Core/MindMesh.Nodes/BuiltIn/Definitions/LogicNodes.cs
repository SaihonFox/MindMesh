using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

internal static class LogicNodes
{
	/// <summary>How many cases a switch can have.</summary>
	private const int MaximumSwitchCases = 20;

	/// <summary>One "Case n equals" setting per possible case; case n is shown while the node has at least n cases.</summary>
	private static IEnumerable<ParameterDefinition> CaseMatchParameters() =>
		Enumerable.Range(1, MaximumSwitchCases).Select(number =>
			Param.Text($"match{number}", $"Case {number} equals")
				.When("caseCount", [.. Enumerable.Range(number, MaximumSwitchCases - number + 1).Select(count => count.ToString(System.Globalization.CultureInfo.InvariantCulture))]));

	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("logic.if", "If", BuiltInCategories.Logic, BuiltInCategories.Conditions)
			.Describe("Continues along the True or False branch depending on a condition.")
			.Icon("branch")
			.Keywords("condition", "else", "branch", "compare")
			.In("in", "In", PortType.Execution)
			.In("value", "Value", PortType.Any, description: "Available to the condition as value.")
			.Out("true", "True", PortType.Execution)
			.Out("false", "False", PortType.Execution)
			.Param(Param.Expression("condition", "Condition", "value > 0", required: true))
			.Build();

		yield return NodeDefinition.Create("logic.switch", "Switch", BuiltInCategories.Logic, BuiltInCategories.Switch)
			.Describe("Routes execution to one of several branches by the value of an expression.")
			.Icon("switch")
			.Keywords("case", "route", "match")
			.In("in", "In", PortType.Execution)
			.In("value", "Value", PortType.Any)
			.DynamicOutputs(new DynamicPortGroup("caseCount", "case{0}", "Case {0}", PortType.Execution, 2, 1, MaximumSwitchCases))
			.Out("default", "Default", PortType.Execution)
			.Param(Param.Expression("expression", "Value to match", "value", required: true))
			.Param(Param.Number("caseCount", "Number of cases", 2, required: true, minimum: 1, maximum: MaximumSwitchCases))
			.Params(CaseMatchParameters())
			.Build();

		yield return NodeDefinition.Create("logic.loop", "Loop", BuiltInCategories.Logic, BuiltInCategories.Loops)
			.Describe("Repeats the branch a fixed number of times, then continues.")
			.Icon("repeat")
			.Keywords("repeat", "times", "while")
			.In("in", "In", PortType.Execution)
			.Out("body", "Each iteration", PortType.Execution)
			.Out("index", "Index", PortType.Integer)
			.Out("done", "Done", PortType.Execution)
			.Param(Param.Number("count", "Repeat (times)", 10, required: true, minimum: 1, maximum: 100000))
			.Param(Param.Boolean("stopOnError", "Stop on error", true))
			.Build();

		yield return NodeDefinition.Create("logic.foreach", "For Each", BuiltInCategories.Logic, BuiltInCategories.Loops)
			.Describe("Runs the branch once for every item of an array.")
			.Icon("loop")
			.Keywords("iterate", "items", "array", "map")
			.In("in", "In", PortType.Execution)
			.In("items", "Items", PortType.Array, required: true)
			.Out("item", "Each item", PortType.Execution)
			.Out("current", "Item", PortType.Any)
			.Out("index", "Index", PortType.Integer)
			.Out("done", "Done", PortType.Execution)
			.Param(Param.Number("batchSize", "Batch size", 1, minimum: 1, maximum: 10000))
			.Param(Param.Boolean("continueOnError", "Continue on error"))
			.Build();

		yield return NodeDefinition.Create("logic.merge", "Merge", BuiltInCategories.Logic, BuiltInCategories.Merge)
			.Describe("Joins several branches back into one.")
			.Icon("merge")
			.Keywords("join", "combine", "wait")
			.In("a", "Branch A", PortType.Execution)
			.In("b", "Branch B", PortType.Execution)
			.Out("out", "Out", PortType.Execution)
			.Param(Param.Select("mode", "Mode", ["Wait for all", "First to arrive", "Append"], "Wait for all"))
			.Build();

		yield return NodeDefinition.Create("logic.delay", "Delay", BuiltInCategories.Logic, BuiltInCategories.Functions)
			.Describe("Waits for a while before continuing.")
			.Icon("hourglass")
			.Keywords("wait", "sleep", "pause")
			.In("in", "In", PortType.Execution)
			.Out("out", "Out", PortType.Execution)
			.Param(Param.Number("amount", "Wait for", 1, required: true, minimum: 0, maximum: 86400))
			.Param(Param.Select("unit", "Unit", ["Milliseconds", "Seconds", "Minutes"], "Seconds"))
			.Build();

		yield return NodeDefinition.Create("logic.try-catch", "Try / Catch", BuiltInCategories.Logic, BuiltInCategories.ErrorHandling)
			.Describe("Runs a branch and handles its errors in another.")
			.Icon("shield")
			.Keywords("error", "exception", "finally", "recover")
			.In("in", "In", PortType.Execution)
			.Out("try", "Try", PortType.Execution)
			.Out("catch", "Catch", PortType.Execution)
			.Out("finally", "Finally", PortType.Execution)
			.Out("error", "Error", PortType.Object)
			.Out("onError", "On error", PortType.Event)
			.Param(Param.Boolean("rethrow", "Rethrow after catch"))
			.Build();

		yield return NodeDefinition.Create("logic.stop", "Stop Workflow", BuiltInCategories.Logic, BuiltInCategories.ErrorHandling)
			.Describe("Ends the workflow with a status and a message.")
			.Icon("stop")
			.Keywords("end", "terminate", "abort", "exit")
			.In("in", "In", PortType.Execution)
			.In("event", "Stop on event", PortType.Event)
			.Param(Param.Select("status", "Status", ["Success", "Error"], "Success"))
			.Param(Param.Text("message", "Message"))
			.Build();
	}
}
