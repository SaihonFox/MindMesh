using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;

namespace MindMesh.Presentation.Projects;

/// <summary>
/// The "New project" dialog. Nothing exists until the user presses Create: the name and description are edited first,
/// and only then the project file is made and opened in the editor.
/// </summary>
public sealed partial class NewProjectDialogViewModel : ViewModelBase
{
	private readonly ProjectService _projects;
	private readonly INavigationService _navigation;

	[ObservableProperty]
	private bool _isOpen;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(CreateCommand))]
	private string _name = string.Empty;

	[ObservableProperty]
	private string _description = string.Empty;

	[ObservableProperty]
	private string? _error;

	[ObservableProperty]
	private ProjectKindOption? _selectedKind;

	public NewProjectDialogViewModel(ILocalizer localizer, ProjectService projects, INavigationService navigation)
		: base(localizer)
	{
		_projects = projects;
		_navigation = navigation;

		Kinds = BuildKinds();
		_selectedKind = Kinds[0];
	}

	/// <summary>The kinds of project: Workflow is built, the others are placeholders for what comes later.</summary>
	public IReadOnlyList<ProjectKindOption> Kinds { get; private set; }

	protected override void OnLanguageChanged()
	{
		Kinds = BuildKinds();
		OnPropertyChanged(nameof(Kinds));
		SelectedKind = Kinds[0];
	}

	[RelayCommand]
	private void Open()
	{
		Name = string.Empty;
		Description = string.Empty;
		Error = null;
		SelectedKind = Kinds[0];
		IsOpen = true;
	}

	[RelayCommand]
	private void Cancel() => IsOpen = false;

	[RelayCommand(CanExecute = nameof(CanCreate))]
	private async Task CreateAsync()
	{
		if (await _projects.CreateAsync(Name, Description))
		{
			IsOpen = false;
			_navigation.NavigateTo(PageKey.Editor);
		}
		else
		{
			Error = string.Format(Loc["Project_CreateFailed"], _projects.LastError);
		}
	}

	private bool CanCreate() => !string.IsNullOrWhiteSpace(Name) && SelectedKind is { IsAvailable: true };

	partial void OnSelectedKindChanged(ProjectKindOption? value) => CreateCommand.NotifyCanExecuteChanged();

	private List<ProjectKindOption> BuildKinds()
	{
		string soon = Loc["ProjectKind_Soon"];

		return
		[
			new ProjectKindOption(Loc["ProjectKind_Workflow"], Loc["ProjectKind_WorkflowHint"], IconKeys.Workflows, true),
			new ProjectKindOption(Loc["ProjectKind_Agent"], soon, IconKeys.Assistant, false),
			new ProjectKindOption(Loc["ProjectKind_Design"], soon, IconKeys.Templates, false),
			new ProjectKindOption(Loc["ProjectKind_Minecraft"], soon, IconKeys.Plugins, false),
			new ProjectKindOption(Loc["ProjectKind_Document"], soon, IconKeys.List, false),
		];
	}
}
