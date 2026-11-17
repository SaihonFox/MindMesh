using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Projects;

namespace MindMesh.Presentation.Shell;

/// <summary>
/// Top bar: logo, what is open, save status, editor toolbar and the user menu.
/// </summary>
public sealed partial class TopBarViewModel : ViewModelBase
{
	private readonly INavigationService _navigation;
	private readonly EditorWorkspace _workspace;
	private readonly ProjectService _projects;

	[ObservableProperty]
	private bool _isRenaming;

	[ObservableProperty]
	private string _renameText = string.Empty;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(SaveStatusText))]
	private string? _saveError;

	public TopBarViewModel(
		ILocalizer localizer,
		INavigationService navigation,
		EditorWorkspace workspace,
		ProjectService projects,
		EditorToolbarViewModel toolbar)
		: base(localizer)
	{
		_navigation = navigation;
		_workspace = workspace;
		_projects = projects;
		Toolbar = toolbar;

		navigation.PropertyChanged += OnNavigationPropertyChanged;
		workspace.PropertyChanged += OnWorkspacePropertyChanged;
	}

	public EditorToolbarViewModel Toolbar { get; }

	/// <summary>The editor toolbar is only shown while the editor is the current page.</summary>
	public bool IsEditorActive => _navigation.CurrentPageKey == PageKey.Editor;

	public string ProjectDisplayName =>
		string.IsNullOrWhiteSpace(_workspace.ProjectName) ? Loc["Workspace_UntitledProject"] : _workspace.ProjectName;

	public string WorkflowDisplayName =>
		string.IsNullOrWhiteSpace(_workspace.WorkflowName) ? Loc["Workspace_UntitledWorkflow"] : _workspace.WorkflowName;

	/// <summary>The workflow name is shown after the project name only when it differs from it.</summary>
	public bool ShowWorkflow =>
		!string.IsNullOrWhiteSpace(_workspace.WorkflowName) && _workspace.WorkflowName != _workspace.ProjectName;

	public string SaveStatusText =>
		SaveError is null ? Loc[$"SaveStatus_{_workspace.SaveStatus}"] : string.Format(Loc["Project_SaveFailed"], SaveError);

	[RelayCommand]
	private void BeginRename()
	{
		RenameText = ProjectDisplayName;
		IsRenaming = true;
	}

	[RelayCommand]
	private void CancelRename() => IsRenaming = false;

	/// <summary>Takes the typed name, renames the project and saves it.</summary>
	[RelayCommand]
	private async Task CommitRenameAsync()
	{
		if (!IsRenaming)
		{
			return;
		}

		IsRenaming = false;

		var name = RenameText.Trim();

		if (name.Length == 0 || name == ProjectDisplayName)
		{
			return;
		}

		SaveError = await _projects.RenameAsync(name) ? null : _projects.LastError;
	}

	/// <summary>Saves the open project (Ctrl+S).</summary>
	[RelayCommand]
	private async Task SaveAsync() => SaveError = await _projects.SaveAsync() ? null : _projects.LastError;

	[RelayCommand]
	private void GoHome() => _navigation.NavigateTo(PageKey.Welcome);

	[RelayCommand]
	private void OpenSettings() => _navigation.NavigateTo(PageKey.Settings);

	[RelayCommand]
	private void OpenAbout() => _navigation.NavigateTo(PageKey.About);

	protected override void OnLanguageChanged()
	{
		OnPropertyChanged(nameof(ProjectDisplayName));
		OnPropertyChanged(nameof(WorkflowDisplayName));
		OnPropertyChanged(nameof(SaveStatusText));
	}

	private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(INavigationService.CurrentPageKey))
		{
			OnPropertyChanged(nameof(IsEditorActive));
		}
	}

	private void OnWorkspacePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(EditorWorkspace.ProjectName):
				OnPropertyChanged(nameof(ProjectDisplayName));
				OnPropertyChanged(nameof(ShowWorkflow));
				break;
			case nameof(EditorWorkspace.WorkflowName):
				OnPropertyChanged(nameof(WorkflowDisplayName));
				OnPropertyChanged(nameof(ShowWorkflow));
				break;
			case nameof(EditorWorkspace.SaveStatus):
				OnPropertyChanged(nameof(SaveStatusText));
				break;
		}
	}
}
