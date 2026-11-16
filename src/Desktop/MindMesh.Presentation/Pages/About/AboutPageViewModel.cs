using System.Runtime.InteropServices;
using MindMesh.Application.AppInfo;
using MindMesh.Application.Paths;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;

namespace MindMesh.Presentation.Pages.About;

/// <summary>
/// About screen: version and diagnostic information.
/// </summary>
public sealed class AboutPageViewModel : PageViewModel
{
	public AboutPageViewModel(ILocalizer localizer, IAppInfo appInfo, IAppPaths paths)
		: base(PageKey.About, localizer)
	{
		AppName = appInfo.Name;
		Version = appInfo.Version;
		Copyright = appInfo.Copyright;
		Runtime = RuntimeInformation.FrameworkDescription;
		OperatingSystem = RuntimeInformation.OSDescription;
		DataDirectory = paths.DataDirectory;
	}

	public string AppName { get; }

	public string Version { get; }

	public string Copyright { get; }

	public string Runtime { get; }

	public string OperatingSystem { get; }

	public string DataDirectory { get; }
}
