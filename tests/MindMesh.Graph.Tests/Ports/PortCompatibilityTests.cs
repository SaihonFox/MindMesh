using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Tests.Ports;

public class PortCompatibilityTests
{
	[Theory]
	[InlineData(PortType.String)]
	[InlineData(PortType.Number)]
	[InlineData(PortType.Json)]
	[InlineData(PortType.Execution)]
	[InlineData(PortType.Event)]
	[InlineData(PortType.AIContext)]
	public void EveryTypeIsCompatibleWithItself(PortType type) =>
		Assert.True(PortCompatibility.IsCompatible(type, type));

	[Theory]
	[InlineData(PortType.String)]
	[InlineData(PortType.Object)]
	[InlineData(PortType.Binary)]
	public void AnyConnectsToEveryDataType_InBothDirections(PortType type)
	{
		Assert.True(PortCompatibility.IsCompatible(PortType.Any, type));
		Assert.True(PortCompatibility.IsCompatible(type, PortType.Any));
	}

	[Theory]
	[InlineData(PortType.Integer, PortType.Number)]
	[InlineData(PortType.Number, PortType.Decimal)]
	[InlineData(PortType.Boolean, PortType.String)]
	[InlineData(PortType.Object, PortType.Json)]
	[InlineData(PortType.Json, PortType.Array)]
	public void ListedConversionsAreAllowed(PortType source, PortType target) =>
		Assert.True(PortCompatibility.IsCompatible(source, target));

	[Theory]
	[InlineData(PortType.String, PortType.Number)]
	[InlineData(PortType.Boolean, PortType.Integer)]
	[InlineData(PortType.Binary, PortType.String)]
	[InlineData(PortType.String, PortType.Boolean)]
	public void UnlistedConversionsAreRejected(PortType source, PortType target) =>
		Assert.False(PortCompatibility.IsCompatible(source, target));

	[Theory]
	[InlineData(PortType.Execution, PortType.String)]
	[InlineData(PortType.String, PortType.Execution)]
	[InlineData(PortType.Execution, PortType.Event)]
	[InlineData(PortType.AIContext, PortType.Any)]
	[InlineData(PortType.Any, PortType.Event)]
	[InlineData(PortType.Execution, PortType.Any)]
	public void DifferentKindsNeverConnect(PortType source, PortType target) =>
		Assert.False(PortCompatibility.IsCompatible(source, target));

	[Fact]
	public void ConversionIsRequiredOnlyWhenTheTypesDifferAndNeitherIsAny()
	{
		Assert.True(PortCompatibility.RequiresConversion(PortType.Integer, PortType.String));
		Assert.False(PortCompatibility.RequiresConversion(PortType.String, PortType.String));
		Assert.False(PortCompatibility.RequiresConversion(PortType.Any, PortType.String));
		Assert.False(PortCompatibility.RequiresConversion(PortType.String, PortType.Any));
		Assert.False(PortCompatibility.RequiresConversion(PortType.String, PortType.Number));
	}

	[Theory]
	[InlineData(PortType.Execution, PortKind.Execution)]
	[InlineData(PortType.Event, PortKind.Event)]
	[InlineData(PortType.AIContext, PortKind.AIContext)]
	[InlineData(PortType.String, PortKind.Data)]
	[InlineData(PortType.Any, PortKind.Data)]
	public void PortKindDecidesTheVisualFamily(PortType type, PortKind kind) =>
		Assert.Equal(kind, PortTypes.GetKind(type));

	[Fact]
	public void EveryPortTypeHasAKind()
	{
		foreach (var type in Enum.GetValues<PortType>())
		{
			Assert.True(Enum.IsDefined(PortTypes.GetKind(type)));
		}
	}
}
