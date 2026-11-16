using MindMesh.Application.Settings;

namespace MindMesh.Application.Tests;

public class AppSettingsTests
{
	[Fact]
	public void Defaults_AreAlreadyNormalized()
	{
		var defaults = new AppSettings();

		Assert.Equal(defaults, defaults.Normalize());
	}

	[Fact]
	public void Defaults_FollowTheSystemLanguageAndShowTheWelcomeScreen()
	{
		var defaults = new AppSettings();

		Assert.Equal("auto", defaults.Language);
		Assert.True(defaults.ShowWelcomeOnStartup);
		Assert.Equal(ThemeMode.Light, defaults.Theme);
		Assert.Equal(AppSettings.CurrentSchemaVersion, defaults.SchemaVersion);
	}

	[Fact]
	public void Normalize_ReplacesUnknownLanguageWithAuto()
	{
		var settings = new AppSettings { Language = "klingon" }.Normalize();

		Assert.Equal("auto", settings.Language);
	}

	[Fact]
	public void Normalize_ReplacesUnknownThemeWithLight()
	{
		var settings = new AppSettings { Theme = (ThemeMode)99 }.Normalize();

		Assert.Equal(ThemeMode.Light, settings.Theme);
	}

	[Theory]
	[InlineData(ThemeMode.Light)]
	[InlineData(ThemeMode.Dark)]
	[InlineData(ThemeMode.System)]
	public void Normalize_KeepsKnownThemes(ThemeMode theme)
	{
		var settings = new AppSettings { Theme = theme }.Normalize();

		Assert.Equal(theme, settings.Theme);
	}

	[Fact]
	public void Normalize_KeepsSupportedLanguage()
	{
		var settings = new AppSettings { Language = "ru" }.Normalize();

		Assert.Equal("ru", settings.Language);
	}

	[Fact]
	public void Normalize_ClampsPanelSizes()
	{
		var settings = new AppSettings
		{
			Layout = new LayoutSettings
			{
				NodeLibraryWidth = 1,
				InspectorWidth = 100_000,
				AssistantWidth = -50,
				ConsoleHeight = 10_000,
			},
		}.Normalize();

		Assert.Equal(LayoutLimits.SidePanelMinWidth, settings.Layout.NodeLibraryWidth);
		Assert.Equal(LayoutLimits.SidePanelMaxWidth, settings.Layout.InspectorWidth);
		Assert.Equal(LayoutLimits.SidePanelMinWidth, settings.Layout.AssistantWidth);
		Assert.Equal(LayoutLimits.ConsoleMaxHeight, settings.Layout.ConsoleHeight);
	}

	[Fact]
	public void Normalize_ReplacesNotANumberWithDefaults()
	{
		var settings = new AppSettings
		{
			Layout = new LayoutSettings
			{
				NodeLibraryWidth = double.NaN,
				InspectorWidth = double.PositiveInfinity,
				ConsoleHeight = double.NaN,
			},
		}.Normalize();

		Assert.Equal(LayoutSettings.DefaultNodeLibraryWidth, settings.Layout.NodeLibraryWidth);
		Assert.Equal(LayoutSettings.DefaultInspectorWidth, settings.Layout.InspectorWidth);
		Assert.Equal(LayoutSettings.DefaultConsoleHeight, settings.Layout.ConsoleHeight);
	}

	[Fact]
	public void Normalize_KeepsValuesInsideTheAllowedRange()
	{
		var settings = new AppSettings
		{
			Layout = new LayoutSettings { NodeLibraryWidth = 300, ConsoleHeight = 250 },
		}.Normalize();

		Assert.Equal(300, settings.Layout.NodeLibraryWidth);
		Assert.Equal(250, settings.Layout.ConsoleHeight);
	}

	[Fact]
	public void Normalize_WritesCurrentSchemaVersion()
	{
		var settings = new AppSettings { SchemaVersion = 0 }.Normalize();

		Assert.Equal(AppSettings.CurrentSchemaVersion, settings.SchemaVersion);
	}

	[Fact]
	public void Records_CompareByValue()
	{
		var first = new AppSettings { Layout = new LayoutSettings { IsSidebarCompact = true } };
		var second = new AppSettings { Layout = new LayoutSettings { IsSidebarCompact = true } };

		Assert.Equal(first, second);
	}
}
