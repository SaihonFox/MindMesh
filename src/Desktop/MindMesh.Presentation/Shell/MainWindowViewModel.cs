using MindMesh.Application.AppInfo;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;

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
		ISettingsService settings,
		IAppInfo appInfo)
		: base(localizer)
	{
		Navigation = navigation;
		Sidebar = sidebar;
		TopBar = topBar;
		WindowTitle = appInfo.Name;

		navigation.NavigateTo(settings.Current.ShowWelcomeOnStartup ? PageKey.Welcome : PageKey.Editor);
	}

	public INavigationService Navigation { get; }

	public SidebarViewModel Sidebar { get; }

	public TopBarViewModel TopBar { get; }

	public string WindowTitle { get; }
}
