using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.AppInfo;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Projects;

namespace MindMesh.Presentation.Pages.Welcome;

/// <summary>
/// Welcome screen: entry points into the application.
/// </summary>
public sealed partial class WelcomePageViewModel : PageViewModel
{
	private readonly INavigationService _navigation;
	private readonly ISettingsService _settings;
	private readonly NewProjectDialogViewModel _newProjectDialog;

	[ObservableProperty]
	private bool _showOnStartup;

	public WelcomePageViewModel(
		ILocalizer localizer,
		INavigationService navigation,
		ISettingsService settings,
		NewProjectDialogViewModel newProjectDialog,
		IAppInfo appInfo)
		: base(PageKey.Welcome, localizer)
	{
		_navigation = navigation;
		_settings = settings;
		_newProjectDialog = newProjectDialog;

		AppName = appInfo.Name;
		Version = appInfo.Version;
		_showOnStartup = settings.Current.ShowWelcomeOnStartup;
	}

	public string AppName { get; }

	public string Version { get; }

	[RelayCommand]
	private void NewProject() => _newProjectDialog.OpenCommand.Execute(null);

	[RelayCommand]
	private void OpenProjects() => _navigation.NavigateTo(PageKey.Projects);

	[RelayCommand]
	private void OpenEditor() => _navigation.NavigateTo(PageKey.Editor);

	[RelayCommand]
	private void OpenSettings() => _navigation.NavigateTo(PageKey.Settings);

	[RelayCommand]
	private void OpenAbout() => _navigation.NavigateTo(PageKey.About);

	partial void OnShowOnStartupChanged(bool value) =>
		_settings.Update(settings => settings with { ShowWelcomeOnStartup = value });
}
