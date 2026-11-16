using System.Globalization;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Localization;

public class LocalizerTests
{
	private static Localizer Create(string language, out FakeSettingsService settings)
	{
		settings = new FakeSettingsService(new AppSettings { Language = language });
		return new Localizer(settings);
	}

	[Fact]
	public void English_ReturnsEnglishText()
	{
		var localizer = Create("en", out _);

		Assert.Equal("Projects", localizer["Sidebar_Projects"]);
		Assert.Equal("en", localizer.Culture.Name);
	}

	[Fact]
	public void Russian_ReturnsRussianText()
	{
		var localizer = Create("ru", out _);

		Assert.Equal("Проекты", localizer["Sidebar_Projects"]);
		Assert.Equal("ru", localizer.Culture.Name);
	}

	[Fact]
	public void MissingKey_ReturnsTheKeyInBrackets()
	{
		var localizer = Create("en", out _);

		Assert.Equal("[No_Such_Key]", localizer["No_Such_Key"]);
	}

	[Fact]
	public void ChangingTheLanguage_SwitchesTextAndNotifies()
	{
		var localizer = Create("en", out var settings);
		var changed = new List<string?>();
		localizer.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

		settings.Update(current => current with { Language = "ru" });

		Assert.Equal("Настройки", localizer["Sidebar_Settings"]);
		Assert.Contains("Item[]", changed);
		Assert.Contains(nameof(ILocalizer.Culture), changed);
	}

	[Fact]
	public void ChangingTheLanguageBackAndForth_Works()
	{
		var localizer = Create("en", out var settings);

		settings.Update(current => current with { Language = "ru" });
		settings.Update(current => current with { Language = "en" });

		Assert.Equal("Settings", localizer["Sidebar_Settings"]);
	}

	[Fact]
	public void ChangingUnrelatedSettings_DoesNotNotify()
	{
		var localizer = Create("en", out var settings);
		var raised = false;
		localizer.PropertyChanged += (_, _) => raised = true;

		settings.Update(current => current with { ShowWelcomeOnStartup = false });

		Assert.False(raised);
	}

	[Fact]
	public void SwitchingToTheLanguageAlreadyInUse_DoesNotNotify()
	{
		var localizer = Create("auto", out var settings);
		var effective = localizer.Culture.Name;
		var raised = false;
		localizer.PropertyChanged += (_, _) => raised = true;

		settings.Update(current => current with { Language = effective });

		Assert.False(raised);
	}

	[Theory]
	[InlineData("auto", "ru-RU", "ru")]
	[InlineData("auto", "en-US", "en")]
	[InlineData("auto", "de-DE", "en")]
	[InlineData("ru", "en-US", "ru")]
	[InlineData("en", "ru-RU", "en")]
	[InlineData("unknown", "ru-RU", "ru")]
	public void ResolveCulture_FollowsSettingThenSystem(string setting, string system, string expected)
	{
		var culture = Localizer.ResolveCulture(setting, CultureInfo.GetCultureInfo(system));

		Assert.Equal(expected, culture.Name);
	}
}
