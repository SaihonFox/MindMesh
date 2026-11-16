using MindMesh.Application.Localization;

namespace MindMesh.Application.Tests;

public class AppLanguagesTests
{
	[Theory]
	[InlineData("en", "ru", "en")]
	[InlineData("ru", "en", "ru")]
	[InlineData("auto", "ru", "ru")]
	[InlineData("auto", "en", "en")]
	[InlineData("auto", "de", "en")]
	[InlineData("auto", null, "en")]
	[InlineData("xx", "ru", "ru")]
	[InlineData(null, "ru", "ru")]
	[InlineData("", "de", "en")]
	public void Resolve_ReturnsExpectedLanguage(string? setting, string? system, string expected)
	{
		Assert.Equal(expected, AppLanguages.Resolve(setting, system));
	}

	[Theory]
	[InlineData("auto", true)]
	[InlineData("en", true)]
	[InlineData("ru", true)]
	[InlineData("de", false)]
	[InlineData("AUTO", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	public void IsValidSetting_AcceptsOnlyKnownCodes(string? code, bool expected)
	{
		Assert.Equal(expected, AppLanguages.IsValidSetting(code));
	}

	[Fact]
	public void Supported_ContainsEnglishAndRussianWithNativeNames()
	{
		Assert.Collection(
			AppLanguages.Supported,
			english =>
			{
				Assert.Equal("en", english.Code);
				Assert.Equal("English", english.NativeName);
			},
			russian =>
			{
				Assert.Equal("ru", russian.Code);
				Assert.Equal("Русский", russian.NativeName);
			});
	}
}
