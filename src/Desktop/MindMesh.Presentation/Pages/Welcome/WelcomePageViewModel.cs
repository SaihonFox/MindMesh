using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.AppInfo;
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
	private readonly NewProjectDialogViewModel _newProjectDialog;

	public WelcomePageViewModel(
		ILocalizer localizer,
		INavigationService navigation,
		NewProjectDialogViewModel newProjectDialog,
		ProjectsPageViewModel projects,
		IAppInfo appInfo)
		: base(PageKey.Welcome, localizer)
	{
		_navigation = navigation;
		_newProjectDialog = newProjectDialog;

		Projects = projects;
		AppName = appInfo.Name;
		Version = appInfo.Version;
	}

	/// <summary>The project gallery at the bottom of the screen, with its search.</summary>
	public ProjectsPageViewModel Projects { get; }

	public string AppName { get; }

	public override void OnNavigatedTo() => Projects.OnNavigatedTo();

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
}
