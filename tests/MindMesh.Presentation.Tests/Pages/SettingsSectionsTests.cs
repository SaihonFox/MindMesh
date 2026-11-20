using MindMesh.Application.Settings;
using MindMesh.Presentation.Hotkeys;
using MindMesh.Presentation.Pages.Settings;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Pages;

public class SettingsSectionsTests
{
	[Fact]
	public void SelectingAThemeCard_MarksOnlyThatCard_AndStoresIt()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();
		ThemeOptionViewModel ocean = page.Themes.Single(theme => theme.Mode == ThemeMode.Ocean);

		page.SelectThemeCommand.Execute(ocean);

		Assert.Equal(ThemeMode.Ocean, app.Settings.Current.Theme);
		Assert.Equal([ocean], page.Themes.Where(theme => theme.IsSelected).ToArray());
	}

	[Fact]
	public void ShowingASection_SwitchesTheVisibleGroup()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();

		Assert.True(page.IsAppearance);

		page.ShowSectionCommand.Execute(SettingsSection.Hotkeys);

		Assert.True(page.IsHotkeys);
		Assert.False(page.IsAppearance);
	}

	[Fact]
	public void CapturingAKey_ReplacesTheShortcut_AndResetRestoresIt()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();
		HotkeyRowViewModel row = page.Hotkeys.Single(item => item.Action == HotkeyAction.Find);

		page.BeginCaptureCommand.Execute(row);
		bool handled = page.Capture(new HotkeyGesture("G", Ctrl: true));

		Assert.True(handled);
		Assert.Equal(HotkeyAction.Find, app.Get<HotkeyService>().Match(new HotkeyGesture("G", Ctrl: true)));
		Assert.Null(app.Get<HotkeyService>().Match(new HotkeyGesture("F", Ctrl: true)));

		page.ResetHotkeyCommand.Execute(row);

		Assert.Equal(HotkeyAction.Find, app.Get<HotkeyService>().Match(new HotkeyGesture("F", Ctrl: true)));
	}

	[Fact]
	public void CapturingAShortcutInUse_ReportsTheConflict_AndKeepsTheOldOne()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();
		HotkeyRowViewModel row = page.Hotkeys.Single(item => item.Action == HotkeyAction.Find);

		page.BeginCaptureCommand.Execute(row);
		page.Capture(new HotkeyGesture("S", Ctrl: true));

		Assert.NotNull(row.Conflict);
		Assert.Equal(HotkeyAction.Find, app.Get<HotkeyService>().Match(new HotkeyGesture("F", Ctrl: true)));
	}

	[Fact]
	public void WhileCapturing_NoActionRuns()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();

		page.BeginCaptureCommand.Execute(page.Hotkeys[0]);

		Assert.Null(app.Get<HotkeyService>().Match(new HotkeyGesture("S", Ctrl: true)));

		page.StopCapture();

		Assert.Equal(HotkeyAction.SaveProject, app.Get<HotkeyService>().Match(new HotkeyGesture("S", Ctrl: true)));
	}

	[Fact]
	public void ChangingTheProjectsFolder_IsStored_AndCanBeReset()
	{
		using var app = new TestApp();
		var page = app.Get<SettingsPageViewModel>();

		Assert.True(page.IsDefaultProjectsDirectory);

		page.UseDefaultProjectsDirectoryCommand.Execute(null);

		Assert.Null(app.Settings.Current.ProjectsDirectory);
	}
}
