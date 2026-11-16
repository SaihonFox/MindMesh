using MindMesh.Graph.Definitions;

namespace MindMesh.Graph.Tests.Definitions;

public class ParameterValueTests
{
	[Fact]
	public void TheEmptyValue_IsNullAndReadsAsFallbacks()
	{
		var value = ParameterValue.Null;

		Assert.True(value.IsNull);
		Assert.Equal(string.Empty, value.AsString());
		Assert.Equal("x", value.AsString("x"));
		Assert.False(value.AsBoolean());
		Assert.True(value.AsBoolean(true));
		Assert.Null(value.AsDecimal());
		Assert.Empty(value.AsStringList());
		Assert.Empty(value.AsPairs());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("null")]
	public void BlankJson_IsTheEmptyValue(string? json) => Assert.True(new ParameterValue(json).IsNull);

	[Fact]
	public void Strings_RoundTripIncludingQuotesAndNewLines()
	{
		var value = ParameterValue.FromString("a \"quoted\"\nline");

		Assert.Equal("a \"quoted\"\nline", value.AsString());
		Assert.False(value.IsNull);
	}

	[Fact]
	public void AnEmptyString_IsNotTheEmptyValue()
	{
		Assert.False(ParameterValue.FromString(string.Empty).IsNull);
		Assert.True(ParameterValue.FromString(null).IsNull);
	}

	[Fact]
	public void Numbers_UseTheInvariantCulture()
	{
		var value = ParameterValue.FromNumber(12.5m);

		Assert.Equal("12.5", value.Json);
		Assert.Equal(12.5m, value.AsDecimal());
	}

	[Fact]
	public void NumbersCanBeReadFromText()
	{
		Assert.Equal(7m, ParameterValue.FromString("7").AsDecimal());
		Assert.Null(ParameterValue.FromString("seven").AsDecimal());
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Booleans_RoundTrip(bool flag) => Assert.Equal(flag, ParameterValue.FromBoolean(flag).AsBoolean());

	[Fact]
	public void Lists_RoundTripInOrder()
	{
		var value = ParameterValue.FromStringList(["b", "a", "c"]);

		string[] expected = ["b", "a", "c"];
		Assert.Equal(expected, value.AsStringList().ToArray());
	}

	[Fact]
	public void Pairs_RoundTrip()
	{
		var value = ParameterValue.FromPairs([new("Accept", "json"), new("X-Id", "42")]);

		var pairs = value.AsPairs();

		Assert.Equal(2, pairs.Count);
		Assert.Equal("Accept", pairs[0].Key);
		Assert.Equal("json", pairs[0].Value);
		Assert.Equal("42", pairs[1].Value);
	}

	[Fact]
	public void BrokenJson_NeverThrowsWhenRead()
	{
		var value = new ParameterValue("{not json");

		Assert.Equal("{not json", value.AsString());
		Assert.Null(value.AsDecimal());
		Assert.Empty(value.AsStringList());
		Assert.Empty(value.AsPairs());
		Assert.False(value.AsBoolean());
	}

	[Fact]
	public void ValuesWithTheSameJson_AreEqual() =>
		Assert.Equal(ParameterValue.FromString("a"), ParameterValue.FromString("a"));
}
