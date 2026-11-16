using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;

namespace MindMesh.Presentation.Shell;

/// <summary>
/// Top bar: logo, what is open, save status, editor toolbar and the user menu.
/// </summary>
public sealed partial class TopBarViewModel : ViewModelBase
{
	private readonly INavigationService _navigation;
	private readonly EditorWorkspace _workspace;

	public TopBarViewModel(
		ILocalizer localizer,
		INavigationService navigation,
		EditorWorkspace workspace,
		EditorToolbarViewModel toolbar)
		: base(localizer)
	{
		_navigation = navigation;
		_workspace = workspace;
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

	public string SaveStatusText => Loc[$"SaveStatus_{_workspace.SaveStatus}"];

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
				break;
			case nameof(EditorWorkspace.WorkflowName):
				OnPropertyChanged(nameof(WorkflowDisplayName));
				break;
			case nameof(EditorWorkspace.SaveStatus):
				OnPropertyChanged(nameof(SaveStatusText));
				break;
		}
	}
}
