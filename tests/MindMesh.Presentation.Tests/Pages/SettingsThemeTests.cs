using MindMesh.Application.Settings;
using MindMesh.Presentation.Pages.Settings;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Pages;

public class SettingsThemeTests
{
	[Fact]
	public void TheDefaultThemeIsLight()
	{
		using var app = new TestApp();

		var page = app.Get<SettingsPageViewModel>();

		Assert.Equal(ThemeMode.Light, page.SelectedTheme!.Mode);
		Assert.Equal(ThemeMode.Light, new AppSettings().Theme);
	}

	[Fact]
	public void OffersEveryTheme_WithReadableTitles()
	{
		using var app = new TestApp(new AppSettings { Language = "en" });

		var page = app.Get<SettingsPageViewModel>();

		Assert.Equal(Enum.GetValues<ThemeMode>(), page.Themes.Select(theme => theme.Mode).ToArray());
		Assert.Equal("Light", page.Themes.Single(theme => theme.Mode == ThemeMode.Light).Title);
		Assert.Equal("Dark", page.Themes.Single(theme => theme.Mode == ThemeMode.Dark).Title);
	}

	[Fact]
	public void ChoosingATheme_IsStored()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();

		page.SelectedTheme = page.Themes.Single(theme => theme.Mode == ThemeMode.Dark);

		Assert.Equal(ThemeMode.Dark, app.Settings.Current.Theme);
	}

	[Fact]
	public void TheStoredTheme_IsShownWhenThePageOpens()
	{
		using var app = new TestApp(new AppSettings { Theme = ThemeMode.System });

		Assert.Equal(ThemeMode.System, app.Get<SettingsPageViewModel>().SelectedTheme!.Mode);
	}
}
