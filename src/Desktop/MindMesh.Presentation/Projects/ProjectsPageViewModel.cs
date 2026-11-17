using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Platform;

namespace MindMesh.Presentation.Projects;

/// <summary>The projects page: what is in the projects folder, with opening, renaming and creating.</summary>
public sealed partial class ProjectsPageViewModel : PageViewModel
{
	private readonly ProjectService _projects;
	private readonly NewProjectDialogViewModel _dialog;
	private readonly INavigationService _navigation;
	private readonly IFilePickerService _picker;

	[ObservableProperty]
	private string? _message;

	public ProjectsPageViewModel(
		ILocalizer localizer,
		ProjectService projects,
		NewProjectDialogViewModel dialog,
		INavigationService navigation,
		IFilePickerService picker)
		: base(PageKey.Projects, localizer)
	{
		_projects = projects;
		_dialog = dialog;
		_navigation = navigation;
		_picker = picker;
	}

	public ObservableCollection<ProjectItemViewModel> Items { get; } = [];

	public bool IsEmpty => Items.Count == 0;

	public override void OnNavigatedTo() => _ = RefreshAsync();

	/// <summary>Reads the projects folder again.</summary>
	[RelayCommand]
	public async Task RefreshAsync()
	{
		var projects = await _projects.ListAsync();

		Items.Clear();

		foreach (var project in projects)
		{
			Items.Add(new ProjectItemViewModel(project, Loc["Project_Open"], Loc["Project_Rename"], OpenItemAsync, RenameItemAsync));
		}

		OnPropertyChanged(nameof(IsEmpty));
	}

	[RelayCommand]
	private void New() => _dialog.OpenCommand.Execute(null);

	[RelayCommand]
	private async Task OpenFileAsync()
	{
		var path = await _picker.PickFileAsync(Loc["Project_PickTitle"]);

		if (path is not null)
		{
			await OpenPathAsync(path);
		}
	}

	private Task OpenItemAsync(ProjectItemViewModel item) => OpenPathAsync(item.Path);

	private async Task OpenPathAsync(string path)
	{
		var result = await _projects.OpenAsync(path);

		if (result is null)
		{
			Message = string.Format(Loc["Project_OpenFailed"], Path.GetFileName(path), _projects.LastError);

			return;
		}

		Message = result.IsLossy ? string.Format(Loc["Project_OpenedWithLoss"], Path.GetFileName(path)) : null;
		_navigation.NavigateTo(PageKey.Editor);
	}

	private async Task RenameItemAsync(ProjectItemViewModel item, string name)
	{
		if (await _projects.RenameFileAsync(item.Path, name))
		{
			Message = null;
			await RefreshAsync();
		}
		else
		{
			Message = string.Format(Loc["Project_RenameFailed"], _projects.LastError);
		}
	}
}
