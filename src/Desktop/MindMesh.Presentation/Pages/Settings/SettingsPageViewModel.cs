using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.Localization;
using MindMesh.Application.Paths;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Shell;

namespace MindMesh.Presentation.Pages.Settings;

/// <summary>
/// General settings: language, theme, layout and storage locations.
/// </summary>
public sealed partial class SettingsPageViewModel : PageViewModel
{
	private const string AutoLanguageDisplayName = "Auto · Авто";

	private readonly ISettingsService _settings;
	private readonly EditorLayoutViewModel _editorLayout;
	private readonly INavigationService _navigation;

	[ObservableProperty]
	private LanguageOptionViewModel? _selectedLanguage;

	[ObservableProperty]
	private ThemeOptionViewModel? _selectedTheme;

	public SettingsPageViewModel(
		ILocalizer localizer,
		ISettingsService settings,
		IAppPaths paths,
		SidebarViewModel sidebar,
		EditorLayoutViewModel editorLayout,
		INavigationService navigation)
		: base(PageKey.Settings, localizer)
	{
		_navigation = navigation;
		_settings = settings;
		_editorLayout = editorLayout;

		Sidebar = sidebar;
		DataDirectory = paths.DataDirectory;
		LogsDirectory = paths.LogsDirectory;

		Languages =
		[
			new LanguageOptionViewModel(AppLanguages.Auto, AutoLanguageDisplayName),
			.. AppLanguages.Supported.Select(language => new LanguageOptionViewModel(language.Code, language.NativeName)),
		];

		_selectedLanguage = Languages.FirstOrDefault(option => option.Code == settings.Current.Language)
							?? Languages[0];

		Themes = [.. Enum.GetValues<ThemeMode>().Select(mode => new ThemeOptionViewModel(mode, localizer))];
		_selectedTheme = Themes.First(option => option.Mode == settings.Current.Theme);
	}

	public SidebarViewModel Sidebar { get; }

	public IReadOnlyList<LanguageOptionViewModel> Languages { get; }

	public IReadOnlyList<ThemeOptionViewModel> Themes { get; }

	public string DataDirectory { get; }

	public string LogsDirectory { get; }

	[RelayCommand]
	private void GoBack() => _navigation.GoBack();

	[RelayCommand]
	private void ResetLayout()
	{
		_editorLayout.ResetLayoutCommand.Execute(null);
		Sidebar.IsCompact = false;
	}

	partial void OnSelectedThemeChanged(ThemeOptionViewModel? value)
	{
		if (value is not null)
			_settings.Update(settings => settings with { Theme = value.Mode });
	}

	partial void OnSelectedLanguageChanged(LanguageOptionViewModel? value)
	{
		if (value is not null)
			_settings.Update(settings => settings with { Language = value.Code });
	}
}
