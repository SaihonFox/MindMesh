using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.Localization;
using MindMesh.Application.Paths;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Hotkeys;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Platform;
using MindMesh.Presentation.Shell;

namespace MindMesh.Presentation.Pages.Settings;

/// <summary>
/// Settings, split into sections: appearance, hotkeys and general (language, storage).
/// </summary>
public sealed partial class SettingsPageViewModel : PageViewModel
{
	private const string AutoLanguageDisplayName = "Auto · Авто";

	private readonly ISettingsService _settings;
	private readonly EditorLayoutViewModel _editorLayout;
	private readonly INavigationService _navigation;
	private readonly HotkeyService _hotkeys;
	private readonly IFilePickerService _filePicker;
	private readonly IShellService _shell;
	private readonly IAppPaths _paths;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsAppearance), nameof(IsHotkeys), nameof(IsGeneral))]
	private SettingsSection _section = SettingsSection.Appearance;

	[ObservableProperty]
	private LanguageOptionViewModel? _selectedLanguage;

	[ObservableProperty]
	private ThemeOptionViewModel? _selectedTheme;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ProjectsDirectory), nameof(IsDefaultProjectsDirectory))]
	private string? _customProjectsDirectory;

	[ObservableProperty]
	private HotkeyRowViewModel? _capturingRow;

	public SettingsPageViewModel(
		ILocalizer localizer,
		ISettingsService settings,
		IAppPaths paths,
		SidebarViewModel sidebar,
		EditorLayoutViewModel editorLayout,
		INavigationService navigation,
		HotkeyService hotkeys,
		IFilePickerService filePicker,
		IShellService shell)
		: base(PageKey.Settings, localizer)
	{
		_navigation = navigation;
		_settings = settings;
		_editorLayout = editorLayout;
		_hotkeys = hotkeys;
		_filePicker = filePicker;
		_shell = shell;
		_paths = paths;

		Sidebar = sidebar;
		DataDirectory = paths.DataDirectory;
		LogsDirectory = paths.LogsDirectory;
		_customProjectsDirectory = settings.Current.ProjectsDirectory;

		Languages =
		[
			new LanguageOptionViewModel(AppLanguages.Auto, AutoLanguageDisplayName),
			.. AppLanguages.Supported.Select(language => new LanguageOptionViewModel(language.Code, language.NativeName)),
		];

		_selectedLanguage = Languages.FirstOrDefault(option => option.Code == settings.Current.Language)
							?? Languages[0];

		Themes = [.. Enum.GetValues<ThemeMode>().Select(mode => new ThemeOptionViewModel(mode, localizer))];
		_selectedTheme = Themes.First(option => option.Mode == settings.Current.Theme);
		_selectedTheme.IsSelected = true;

		Hotkeys = [.. HotkeyService.Definitions.Select(definition => new HotkeyRowViewModel(definition.Action, hotkeys, localizer))];
	}

	public SidebarViewModel Sidebar { get; }

	public IReadOnlyList<LanguageOptionViewModel> Languages { get; }

	public IReadOnlyList<ThemeOptionViewModel> Themes { get; }

	public ObservableCollection<HotkeyRowViewModel> Hotkeys { get; }

	public string DataDirectory { get; }

	public string LogsDirectory { get; }

	public bool IsAppearance => Section == SettingsSection.Appearance;

	public bool IsHotkeys => Section == SettingsSection.Hotkeys;

	public bool IsGeneral => Section == SettingsSection.General;

	/// <summary>The folder new projects are saved in and the project list reads from.</summary>
	public string ProjectsDirectory => CustomProjectsDirectory ?? _paths.ProjectsDirectory;

	public bool IsDefaultProjectsDirectory => CustomProjectsDirectory is null;

	[RelayCommand]
	private void GoBack() => _navigation.GoBack();

	[RelayCommand]
	private void ShowSection(SettingsSection section)
	{
		StopCapture();
		Section = section;
	}

	[RelayCommand]
	private void ResetLayout()
	{
		_editorLayout.ResetLayoutCommand.Execute(null);
		Sidebar.IsCompact = false;
	}

	[RelayCommand]
	private void SelectTheme(ThemeOptionViewModel theme) => SelectedTheme = theme;

	[RelayCommand]
	private async Task ChangeProjectsDirectoryAsync()
	{
		string? folder = await _filePicker.PickFolderAsync(Loc["Settings_ChooseProjectsFolder"]);

		if (string.IsNullOrWhiteSpace(folder))
			return;

		SetProjectsDirectory(folder);
	}

	[RelayCommand]
	private void UseDefaultProjectsDirectory() => SetProjectsDirectory(null);

	[RelayCommand]
	private void OpenProjectsDirectory()
	{
		Directory.CreateDirectory(ProjectsDirectory);
		_shell.OpenFolder(ProjectsDirectory);
	}

	[RelayCommand]
	private void OpenLogsDirectory()
	{
		Directory.CreateDirectory(LogsDirectory);
		_shell.OpenFolder(LogsDirectory);
	}

	/// <summary>Starts waiting for a new shortcut of the row.</summary>
	[RelayCommand]
	private void BeginCapture(HotkeyRowViewModel row)
	{
		StopCapture();
		row.Conflict = null;
		row.IsCapturing = true;
		CapturingRow = row;
		_hotkeys.IsCapturing = true;
	}

	[RelayCommand]
	private void ResetHotkey(HotkeyRowViewModel row)
	{
		StopCapture();
		_hotkeys.Reset(row.Action);
		row.Conflict = null;
		row.Refresh();
	}

	[RelayCommand]
	private void ResetAllHotkeys()
	{
		StopCapture();
		_hotkeys.ResetAll();

		foreach (HotkeyRowViewModel row in Hotkeys)
		{
			row.Conflict = null;
			row.Refresh();
		}
	}

	/// <summary>Gives the row waiting for a key its new shortcut. Returns false when no row is waiting.</summary>
	public bool Capture(HotkeyGesture gesture)
	{
		if (CapturingRow is not { } row)
			return false;

		HotkeyAction? owner = _hotkeys.Assign(row.Action, gesture);

		if (owner is { } other)
		{
			row.Conflict = string.Format(Loc["Settings_HotkeyConflict"], Loc[$"Hotkey_{other}"]);
			row.IsCapturing = false;
			CapturingRow = null;
			_hotkeys.IsCapturing = false;

			return true;
		}

		row.Conflict = null;
		StopCapture();
		row.Refresh();

		return true;
	}

	/// <summary>Stops waiting for a shortcut without changing anything.</summary>
	public void StopCapture()
	{
		if (CapturingRow is { } row)
			row.IsCapturing = false;

		CapturingRow = null;
		_hotkeys.IsCapturing = false;
	}

	private void SetProjectsDirectory(string? folder)
	{
		CustomProjectsDirectory = folder;
		_settings.Update(settings => settings with { ProjectsDirectory = folder });
	}

	partial void OnSelectedThemeChanged(ThemeOptionViewModel? value)
	{
		foreach (ThemeOptionViewModel theme in Themes)
			theme.IsSelected = theme == value;

		if (value is not null)
			_settings.Update(settings => settings with { Theme = value.Mode });
	}

	partial void OnSelectedLanguageChanged(LanguageOptionViewModel? value)
	{
		if (value is not null)
			_settings.Update(settings => settings with { Language = value.Code });
	}

	protected override void OnLanguageChanged()
	{
		base.OnLanguageChanged();
		OnPropertyChanged(nameof(ProjectsDirectory));
	}
}
