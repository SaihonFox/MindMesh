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

	public NewProjectDialogViewModel(ILocalizer localizer, ProjectService projects, INavigationService navigation)
		: base(localizer)
	{
		_projects = projects;
		_navigation = navigation;
	}

	[RelayCommand]
	private void Open()
	{
		Name = string.Empty;
		Description = string.Empty;
		Error = null;
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

	private bool CanCreate() => !string.IsNullOrWhiteSpace(Name);
}
