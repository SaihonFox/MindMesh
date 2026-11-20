using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Platform;
using MindMesh.Presentation.Projects.Covers;

namespace MindMesh.Presentation.Projects;

/// <summary>The projects page: what is in the projects folder, with opening, renaming and creating.</summary>
public sealed partial class ProjectsPageViewModel : PageViewModel
{
	private readonly ProjectService _projects;
	private readonly NewProjectDialogViewModel _dialog;
	private readonly INavigationService _navigation;
	private readonly IFilePickerService _picker;
	private readonly ProjectCovers _covers;
	private List<ProjectItemViewModel> _all = [];

	[ObservableProperty]
	private string? _message;

	[ObservableProperty]
	private string _search = string.Empty;

	public ProjectsPageViewModel(
		ILocalizer localizer,
		ProjectService projects,
		NewProjectDialogViewModel dialog,
		INavigationService navigation,
		IFilePickerService picker,
		ProjectCovers covers)
		: base(PageKey.Projects, localizer)
	{
		_projects = projects;
		_dialog = dialog;
		_navigation = navigation;
		_picker = picker;
		_covers = covers;
	}

	public ObservableCollection<ProjectItemViewModel> Items { get; } = [];

	/// <summary>True when the projects folder has no project at all.</summary>
	public bool IsEmpty => _all.Count == 0;

	/// <summary>True when there are projects, but none matches the search text.</summary>
	public bool HasNoMatches => _all.Count > 0 && Items.Count == 0;

	public override void OnNavigatedTo() => _ = RefreshAsync();

	/// <summary>Reads the projects folder again.</summary>
	[RelayCommand]
	public async Task RefreshAsync()
	{
		var projects = await _projects.ListAsync();
		List<ProjectItemViewModel> items = [];

		foreach (var project in projects)
		{
			string? cover = await _covers.EnsureAsync(project.Path);

			items.Add(new ProjectItemViewModel(
				project,
				Loc["Project_Open"],
				Loc["Project_Rename"],
				Loc["Project_RandomCover"],
				Loc["Project_ChangeCover"],
				OpenItemAsync,
				RenameItemAsync,
				RerollCoverAsync,
				ChooseCoverAsync,
				cover));
		}

		_all = items;
		ApplySearch();
	}

	partial void OnSearchChanged(string value) => ApplySearch();

	private void ApplySearch()
	{
		string text = Search.Trim();

		Items.Clear();

		foreach (ProjectItemViewModel item in _all)
		{
			if (text.Length == 0
				|| item.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase)
				|| item.Description.Contains(text, StringComparison.CurrentCultureIgnoreCase))
				Items.Add(item);
		}

		OnPropertyChanged(nameof(IsEmpty));
		OnPropertyChanged(nameof(HasNoMatches));
	}

	private async Task RerollCoverAsync(ProjectItemViewModel item) => item.ReloadCover(await _covers.RerollAsync(item.Path));

	private async Task ChooseCoverAsync(ProjectItemViewModel item)
	{
		string? image = await _picker.PickImageAsync(Loc["Project_ChooseCover"]);

		if (image is not null)
			item.ReloadCover(await _covers.SetImageAsync(item.Path, image) ?? item.CoverPath);
	}

	[RelayCommand]
	private void New() => _dialog.OpenCommand.Execute(null);

	[RelayCommand]
	private async Task OpenFileAsync()
	{
		var path = await _picker.PickFileAsync(Loc["Project_PickTitle"]);

		if (path is not null)
			await OpenPathAsync(path);
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
