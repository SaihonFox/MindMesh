using MindMesh.Application.AppInfo;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Hotkeys;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Projects;

namespace MindMesh.Presentation.Shell;

/// <summary>
/// Root view model of the main window.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
	public MainWindowViewModel(
		ILocalizer localizer,
		INavigationService navigation,
		SidebarViewModel sidebar,
		TopBarViewModel topBar,
		NewProjectDialogViewModel newProjectDialog,
		HotkeyService hotkeys,
		ISettingsService settings,
		IAppInfo appInfo)
		: base(localizer)
	{
		Navigation = navigation;
		Sidebar = sidebar;
		TopBar = topBar;
		NewProjectDialog = newProjectDialog;
		Hotkeys = hotkeys;
		WindowTitle = appInfo.Name;

		navigation.NavigateTo(settings.Current.ShowWelcomeOnStartup ? PageKey.Welcome : PageKey.Editor);
	}

	public INavigationService Navigation { get; }

	public SidebarViewModel Sidebar { get; }

	public TopBarViewModel TopBar { get; }

	public NewProjectDialogViewModel NewProjectDialog { get; }

	public HotkeyService Hotkeys { get; }

	public string WindowTitle { get; }

	/// <summary>Runs the window-wide actions (save, new project). Returns false for any other action.</summary>
	public bool RunHotkey(HotkeyAction action)
	{
		switch (action)
		{
			case HotkeyAction.SaveProject:
				TopBar.SaveCommand.Execute(null);
				return true;

			case HotkeyAction.NewProject:
				NewProjectDialog.OpenCommand.Execute(null);
				return true;

			default:
				return false;
		}
	}
}
