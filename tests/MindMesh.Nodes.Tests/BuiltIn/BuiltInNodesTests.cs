using MindMesh.Graph.Definitions;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;
using MindMesh.Graph.Validation;
using MindMesh.Nodes.BuiltIn;

namespace MindMesh.Nodes.Tests.BuiltIn;

public class BuiltInNodesTests
{
	private static readonly IReadOnlyList<INodeDefinition> Nodes = BuiltInExtension.CreateNodes();

	[Fact]
	public void TheSpecifiedNodesExist()
	{
		Assert.Equal(50, Nodes.Count);
	}

	[Fact]
	public void NodeIdentifiersAreUniqueAndLookLikeGroupDotName()
	{
		Assert.Equal(Nodes.Count, Nodes.Select(node => node.Id).Distinct().Count());
		Assert.All(Nodes, node => Assert.Matches(@"^[a-z]+\.[a-z0-9]+(-[a-z0-9]+)*$", node.Id.Value));
	}

	[Fact]
	public void EveryNodeIsDescribed()
	{
		foreach (var node in Nodes)
		{
			Assert.False(string.IsNullOrWhiteSpace(node.Name), $"{node.Id} has no name.");
			Assert.False(string.IsNullOrWhiteSpace(node.Description), $"{node.Id} has no description.");
			Assert.False(string.IsNullOrWhiteSpace(node.IconKey), $"{node.Id} has no icon.");
			Assert.NotEmpty(node.Categories);
		}
	}

	[Fact]
	public void EveryNodeUsesAKnownCategory()
	{
		var known = BuiltInCategories.All.Select(category => category.Id).ToHashSet();

		foreach (var node in Nodes)
		{
			Assert.All(node.Categories, category => Assert.True(known.Contains(category), $"{node.Id} uses unknown category {category}."));
		}
	}

	[Fact]
	public void PortKeysAreUniqueWithinANode()
	{
		foreach (var node in Nodes)
		{
			var keys = node.Inputs.Concat(node.Outputs).Select(port => port.Key).ToList();

			Assert.True(keys.Count == keys.Distinct().Count(), $"{node.Id} reuses a port key.");
		}
	}

	[Fact]
	public void SettingKeysAreUniqueWithinANode()
	{
		foreach (var node in Nodes)
		{
			var keys = node.Parameters.Select(parameter => parameter.Key).ToList();

			Assert.True(keys.Count == keys.Distinct().Count(), $"{node.Id} reuses a setting key.");
		}
	}

	[Fact]
	public void PortDirectionsMatchTheListTheyAreIn()
	{
		foreach (var node in Nodes)
		{
			Assert.All(node.Inputs, port => Assert.Equal(PortDirection.Input, port.Direction));
			Assert.All(node.Outputs, port => Assert.Equal(PortDirection.Output, port.Direction));
		}
	}

	[Fact]
	public void SelectSettingsHaveOptionsAndTheDefaultIsOneOfThem()
	{
		foreach (var parameter in Nodes.SelectMany(node => node.Parameters).Where(parameter => parameter.Type == ParameterType.Select))
		{
			Assert.NotEmpty(parameter.Options);
			Assert.Contains(parameter.Options, option => option.Value == parameter.DefaultValue.AsString());
		}
	}

	[Fact]
	public void SettingsThatDependOnAnotherPointAtAnExistingSetting()
	{
		foreach (var node in Nodes)
		{
			foreach (var parameter in node.Parameters.Where(parameter => parameter.VisibleWhen is not null))
			{
				var condition = parameter.VisibleWhen!;
				var driver = node.Parameters.SingleOrDefault(other => other.Key == condition.Key);

				Assert.True(driver is not null, $"{node.Id}.{parameter.Key} depends on missing setting '{condition.Key}'.");
				Assert.True(driver!.Type is ParameterType.Select or ParameterType.Boolean, $"{node.Id}.{parameter.Key} depends on a setting that has no fixed values.");

				if (driver.Type == ParameterType.Select)
				{
					Assert.All(condition.Values, value =>
						Assert.True(driver.Options.Any(option => option.Value == value), $"{node.Id}.{parameter.Key} waits for unknown value '{value}'."));
				}
			}
		}
	}

	[Fact]
	public void NumbersHaveSaneRangesAndDefaultsInsideThem()
	{
		foreach (var parameter in Nodes.SelectMany(node => node.Parameters).Where(parameter => parameter.Type == ParameterType.Number))
		{
			if (parameter.Minimum is { } minimum && parameter.Maximum is { } maximum)
			{
				Assert.True(minimum <= maximum);
			}

			if (parameter.DefaultValue.AsDecimal() is { } number)
			{
				Assert.True(parameter.Minimum is null || number >= parameter.Minimum);
				Assert.True(parameter.Maximum is null || number <= parameter.Maximum);
			}
		}
	}

	[Fact]
	public void CodeAndExpressionSettingsNameALanguage_OrNone()
	{
		foreach (var parameter in Nodes.SelectMany(node => node.Parameters).Where(parameter => parameter.Type == ParameterType.Code))
		{
			Assert.False(string.IsNullOrWhiteSpace(parameter.Language), $"Code setting '{parameter.Key}' names no language.");
		}
	}

	[Fact]
	public void SecretSettingsNeverHaveADefault()
	{
		foreach (var parameter in Nodes.SelectMany(node => node.Parameters).Where(parameter => parameter.Type == ParameterType.Secret))
		{
			Assert.True(parameter.DefaultValue.IsNull, $"Secret '{parameter.Key}' has a default value.");
		}
	}

	[Fact]
	public void EveryNodeCanBeInstantiatedAndItsDefaultsAreValid()
	{
		foreach (var definition in Nodes)
		{
			var node = NodeInstance.Create(definition);

			// Settings without a default may be reported as missing, but never as malformed.
			var malformed = NodeValidator.ValidateParameters(definition, node)
				.Where(issue => issue.Code != ValidationCode.RequiredParameterMissing)
				.ToList();

			Assert.True(malformed.Count == 0, $"{definition.Id} has invalid defaults: {string.Join(", ", malformed.Select(issue => $"{issue.Key}:{issue.Code}"))}");
		}
	}

	[Fact]
	public void TriggersHaveNoInputsAndSomeOutput()
	{
		foreach (var node in Nodes.Where(node => node.Id.Value.StartsWith("trigger.", StringComparison.Ordinal)))
		{
			Assert.Empty(node.Inputs);
			Assert.NotEmpty(node.Outputs);
		}
	}

	[Fact]
	public void ExecutionPortsAreOnTheNodesThatControlFlow()
	{
		string[] flowNodes = ["logic.if", "logic.switch", "logic.loop", "logic.foreach", "logic.merge", "logic.delay", "logic.try-catch"];

		foreach (var id in flowNodes)
		{
			var node = Nodes.Single(item => item.Id.Value == id);

			Assert.Contains(node.Inputs, port => port.Kind == PortKind.Execution);
			Assert.Contains(node.Outputs, port => port.Kind == PortKind.Execution);
		}
	}

	[Fact]
	public void TheMainPortFamiliesAreUsed()
	{
		var used = Nodes.SelectMany(node => node.Inputs.Concat(node.Outputs)).Select(port => port.Type).ToHashSet();

		// Types that a node of the built-in set has no reason to use may stay unused, but the main families must appear.
		Assert.Contains(PortType.Execution, used);
		Assert.Contains(PortType.AIContext, used);
		Assert.Contains(PortType.String, used);
		Assert.Contains(PortType.Json, used);
		Assert.Contains(PortType.Object, used);
		Assert.Contains(PortType.Array, used);
		Assert.Contains(PortType.Any, used);
	}
}
