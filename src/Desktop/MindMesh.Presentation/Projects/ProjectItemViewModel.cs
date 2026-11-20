using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindMesh.Presentation.Projects;

/// <summary>One row of the projects list: a project file that can be opened or renamed in place.</summary>
public sealed partial class ProjectItemViewModel : ObservableObject
{
	private readonly Func<ProjectItemViewModel, Task> _open;
	private readonly Func<ProjectItemViewModel, string, Task> _rename;
	private readonly Func<ProjectItemViewModel, Task> _reroll;
	private readonly Func<ProjectItemViewModel, Task> _chooseImage;

	[ObservableProperty]
	private string _name;

	[ObservableProperty]
	private bool _isRenaming;

	[ObservableProperty]
	private string _renameText = string.Empty;

	/// <summary>The cover picture file, or null while there is none.</summary>
	[ObservableProperty]
	private string? _coverPath;

	public ProjectItemViewModel(
		ProjectInfo info,
		string openLabel,
		string renameLabel,
		string rerollLabel,
		string coverLabel,
		Func<ProjectItemViewModel, Task> open,
		Func<ProjectItemViewModel, string, Task> rename,
		Func<ProjectItemViewModel, Task> reroll,
		Func<ProjectItemViewModel, Task> chooseImage,
		string? coverPath)
	{
		Info = info;
		_name = info.Name;
		OpenLabel = openLabel;
		RenameLabel = renameLabel;
		RerollLabel = rerollLabel;
		CoverLabel = coverLabel;
		_open = open;
		_rename = rename;
		_reroll = reroll;
		_chooseImage = chooseImage;
		_coverPath = coverPath;
	}

	/// <summary>Tells the view to load the cover file again after it was replaced.</summary>
	public void ReloadCover(string? path)
	{
		CoverPath = path;
		OnPropertyChanged(nameof(CoverPath));
	}

	public ProjectInfo Info { get; }

	public string OpenLabel { get; }

	public string RenameLabel { get; }

	public string RerollLabel { get; }

	public string CoverLabel { get; }

	public string Path => Info.Path;

	public string Description => Info.Description ?? string.Empty;

	public bool HasDescription => !string.IsNullOrWhiteSpace(Info.Description);

	public bool IsReadable => Info.IsReadable;

	public string ModifiedText => Info.Modified.ToString("g", CultureInfo.CurrentCulture);

	[RelayCommand]
	private Task OpenAsync() => _open(this);

	[RelayCommand]
	private Task RerollCoverAsync() => _reroll(this);

	[RelayCommand]
	private Task ChooseCoverAsync() => _chooseImage(this);

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
			return;

		IsRenaming = false;

		var text = RenameText.Trim();

		if (text.Length > 0 && text != Name)
			await _rename(this, text);
	}
}
