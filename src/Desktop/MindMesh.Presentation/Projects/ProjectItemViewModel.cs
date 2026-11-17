using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindMesh.Presentation.Projects;

/// <summary>One row of the projects list: a project file that can be opened or renamed in place.</summary>
public sealed partial class ProjectItemViewModel : ObservableObject
{
	private readonly Func<ProjectItemViewModel, Task> _open;
	private readonly Func<ProjectItemViewModel, string, Task> _rename;

	[ObservableProperty]
	private string _name;

	[ObservableProperty]
	private bool _isRenaming;

	[ObservableProperty]
	private string _renameText = string.Empty;

	public ProjectItemViewModel(
		ProjectInfo info,
		string openLabel,
		string renameLabel,
		Func<ProjectItemViewModel, Task> open,
		Func<ProjectItemViewModel, string, Task> rename)
	{
		Info = info;
		_name = info.Name;
		OpenLabel = openLabel;
		RenameLabel = renameLabel;
		_open = open;
		_rename = rename;
	}

	public ProjectInfo Info { get; }

	public string OpenLabel { get; }

	public string RenameLabel { get; }

	public string Path => Info.Path;

	public string Description => Info.Description ?? string.Empty;

	public bool HasDescription => !string.IsNullOrWhiteSpace(Info.Description);

	public bool IsReadable => Info.IsReadable;

	public string ModifiedText => Info.Modified.ToString("g", CultureInfo.CurrentCulture);

	[RelayCommand]
	private Task OpenAsync() => _open(this);

	[RelayCommand]
	private void BeginRename()
	{
		RenameText = Name;
		IsRenaming = true;
	}

	[RelayCommand]
	private void CancelRename() => IsRenaming = false;

	[RelayCommand]
	private async Task CommitRenameAsync()
	{
		if (!IsRenaming)
		{
			return;
		}

		IsRenaming = false;

		var text = RenameText.Trim();

		if (text.Length > 0 && text != Name)
		{
			await _rename(this, text);
		}
	}
}
