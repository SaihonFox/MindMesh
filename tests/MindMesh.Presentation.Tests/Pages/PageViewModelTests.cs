using MindMesh.Application.Settings;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.About;
using MindMesh.Presentation.Pages.Settings;
using MindMesh.Presentation.Pages.Welcome;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Pages;

public class PageViewModelTests
{
	private sealed class ProbeViewModel : ViewModelBase
	{
		public ProbeViewModel(ILocalizer localizer)
			: base(localizer)
		{
		}

		public int LanguageChangedCount { get; private set; }

		protected override void OnLanguageChanged() => LanguageChangedCount++;
	}

	[Fact]
	public void ViewModelBase_RaisesLocAndCallsTheHookWhenTheLanguageChanges()
	{
		var settings = new FakeSettingsService(new AppSettings { Language = "en" });
		var probe = new ProbeViewModel(new Localizer(settings));
		var changed = new List<string?>();
		probe.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

		settings.Update(current => current with { Language = "ru" });

		Assert.Contains("Loc", changed);
		Assert.Equal(1, probe.LanguageChangedCount);
	}

	[Fact]
	public void ViewModelBase_DoesNotReactToOtherLocalizerNotifications()
	{
		var settings = new FakeSettingsService();
		var probe = new ProbeViewModel(new Localizer(settings));

		settings.Update(current => current with { ShowWelcomeOnStartup = false });

		Assert.Equal(0, probe.LanguageChangedCount);
	}

	[Fact]
	public void ViewModelBase_DoesNotKeepShortLivedViewModelsAlive()
	{
		var settings = new FakeSettingsService();
		var localizer = new Localizer(settings);

		var reference = CreateAndForget(localizer);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.False(reference.TryGetTarget(out _));

		// The stale subscription is removed on the next notification without errors.
		settings.Update(current => current with { Language = "ru" });
	}

	private static WeakReference<ProbeViewModel> CreateAndForget(ILocalizer localizer) =>
		new(new ProbeViewModel(localizer));

	[Fact]
	public void Welcome_ExposesTheApplicationNameAndVersion()
	{
		using var app = new TestApp();
		var welcome = app.Get<WelcomePageViewModel>();

		Assert.Equal("MindMesh", welcome.AppName);
		Assert.Equal("1.2.3-test", welcome.Version);
	}

	[Fact]
	public void Welcome_Commands_Navigate()
	{
		using var app = new TestApp();
		var welcome = app.Get<WelcomePageViewModel>();
		var navigation = app.Get<INavigationService>();

		welcome.OpenEditorCommand.Execute(null);
		Assert.Equal(PageKey.Editor, navigation.CurrentPageKey);

		welcome.OpenSettingsCommand.Execute(null);
		Assert.Equal(PageKey.Settings, navigation.CurrentPageKey);

		welcome.OpenAboutCommand.Execute(null);
		Assert.Equal(PageKey.About, navigation.CurrentPageKey);
	}

	[Fact]
	public void Settings_OffersAutoEnglishAndRussian()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();

		string[] expected = ["auto", "en", "ru"];

		Assert.Equal(expected, page.Languages.Select(option => option.Code).ToArray());
	}

	[Fact]
	public void Settings_SelectsTheStoredLanguage()
	{
		using var app = new TestApp(new AppSettings { Language = "ru" });

		Assert.Equal("ru", app.Get<SettingsPageViewModel>().SelectedLanguage!.Code);
	}

	[Fact]
	public void Settings_ChangingTheLanguageStoresItAndSwitchesTheUi()
	{
		using var app = new TestApp(new AppSettings { Language = "en" });
		var page = app.Get<SettingsPageViewModel>();
		var localizer = app.Get<ILocalizer>();

		page.SelectedLanguage = page.Languages.First(option => option.Code == "ru");

		Assert.Equal("ru", app.Settings.Current.Language);
		Assert.Equal("Проекты", localizer["Sidebar_Projects"]);
	}

	[Fact]
	public void Settings_ResetLayout_RestoresPanelsAndTheSidebar()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();
		app.Get<MindMesh.Presentation.Shell.SidebarViewModel>().IsCompact = true;
		app.Get<MindMesh.Presentation.Pages.Editor.EditorLayoutViewModel>().InspectorWidth = 500;

		page.ResetLayoutCommand.Execute(null);

		Assert.Equal(new LayoutSettings(), app.Settings.Current.Layout);
	}

	[Fact]
	public void Settings_ShowsTheDataLocations()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();

		Assert.Equal("/data", page.DataDirectory);
		Assert.Equal("/data/logs", page.LogsDirectory);
	}

	[Fact]
	public void About_ShowsVersionAndLocations()
	{
		using var app = new TestApp();
		var page = app.Get<AboutPageViewModel>();

		Assert.Equal("1.2.3-test", page.Version);
		Assert.Equal("/data", page.DataDirectory);
	}
}
