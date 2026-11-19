using MindMesh.Application.Paths;
using MindMesh.Application.Workflows;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Serialization;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.Presentation.Projects;

/// <summary>
/// The open project: creates, opens, renames and saves it as a workflow file. A project is one <c>.mmflow</c> file whose
/// name is the project name. Until it is saved for the first time the editor holds an unnamed draft without a file.
/// </summary>
public sealed class ProjectService
{
	public const string FileExtension = ".mmflow";

	private readonly IWorkflowStore _store;
	private readonly IAppPaths _paths;
	private readonly GraphEditorViewModel _editor;
	private readonly EditorWorkspace _workspace;
	private readonly ILocalizer _localizer;
	private Guid _id = Guid.CreateVersion7();
	private string? _description;

	public ProjectService(
		IWorkflowStore store,
		IAppPaths paths,
		GraphEditorViewModel editor,
		EditorWorkspace workspace,
		ILocalizer localizer)
	{
		_store = store;
		_paths = paths;
		_editor = editor;
		_workspace = workspace;
		_localizer = localizer;
	}

	/// <summary>The file of the open project; <c>null</c> for a draft that was never saved.</summary>
	public string? CurrentPath { get; private set; }

	/// <summary>Why the last operation failed, or <c>null</c> when it succeeded.</summary>
	public string? LastError { get; private set; }

	/// <summary>Creates an empty project with its file and opens it in the editor.</summary>
	public async Task<bool> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
	{
		name = name.Trim();

		if (name.Length == 0 || !await SaveChangesAsync(cancellationToken).ConfigureAwait(true))
			return false;

		var content = new WorkflowContent(Guid.CreateVersion7(), name, CleanDescription(description), new NodeGraph(), new GraphLayout());
		var path = NewPath(name);

		try
		{
			await _store.SaveAsync(path, content, cancellationToken).ConfigureAwait(true);
		}
		catch (Exception exception) when (IsFileProblem(exception))
		{
			LastError = exception.Message;

			return false;
		}

		Apply(content, path);

		return true;
	}

	/// <summary>
	/// Opens a project file. Returns <c>null</c> when the file cannot be opened at all; the reason is in
	/// <see cref="LastError"/>. A file that loaded with problems is opened and its result says what was dropped.
	/// </summary>
	public async Task<WorkflowLoadResult?> OpenAsync(string path, CancellationToken cancellationToken = default)
	{
		if (!await SaveChangesAsync(cancellationToken).ConfigureAwait(true))
			return null;

		WorkflowLoadResult result;

		try
		{
			result = await _store.LoadAsync(path, cancellationToken).ConfigureAwait(true);
		}
		catch (Exception exception) when (IsFileProblem(exception))
		{
			LastError = exception.Message;

			return null;
		}

		if (result.Content is not { } content)
		{
			LastError = result.Problems.Count > 0 ? result.Problems[0].Code.ToString() : path;

			return null;
		}

		Apply(content, path);

		return result;
	}

	/// <summary>Writes the open project to its file. A draft gets a file in the projects folder on the first save.</summary>
	public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
	{
		var name = string.IsNullOrWhiteSpace(_workspace.ProjectName)
			? _localizer["Workspace_UntitledProject"]
			: _workspace.ProjectName.Trim();
		var path = CurrentPath ?? NewPath(name);
		var content = new WorkflowContent(_id, name, _description, _editor.Document.Graph, _editor.Document.Layout);

		_workspace.SaveStatus = SaveStatus.Saving;

		try
		{
			await _store.SaveAsync(path, content, cancellationToken).ConfigureAwait(true);
		}
		catch (Exception exception) when (IsFileProblem(exception))
		{
			LastError = exception.Message;
			_workspace.SaveStatus = SaveStatus.Unsaved;

			return false;
		}

		CurrentPath = path;
		LastError = null;
		_workspace.ProjectName = name;
		_workspace.SaveStatus = SaveStatus.Saved;

		return true;
	}

	/// <summary>Gives the open project a new name and saves it.</summary>
	public Task<bool> RenameAsync(string name, CancellationToken cancellationToken = default)
	{
		name = name.Trim();

		if (name.Length == 0)
			return Task.FromResult(false);

		_workspace.ProjectName = name;

		return SaveAsync(cancellationToken);
	}

	/// <summary>Renames a project file that is not necessarily the open one.</summary>
	public async Task<bool> RenameFileAsync(string path, string name, CancellationToken cancellationToken = default)
	{
		name = name.Trim();

		if (name.Length == 0)
			return false;

		if (string.Equals(path, CurrentPath, StringComparison.OrdinalIgnoreCase))
			return await RenameAsync(name, cancellationToken).ConfigureAwait(true);

		try
		{
			var result = await _store.LoadAsync(path, cancellationToken).ConfigureAwait(true);

			// A file that lost something while loading would lose it for good when written back.
			if (result.Content is not { } content || result.IsLossy)
			{
				LastError = result.Problems.Count > 0 ? result.Problems[0].Code.ToString() : path;

				return false;
			}

			await _store.SaveAsync(path, content with { Name = name }, cancellationToken).ConfigureAwait(true);
		}
		catch (Exception exception) when (IsFileProblem(exception))
		{
			LastError = exception.Message;

			return false;
		}

		LastError = null;

		return true;
	}

	/// <summary>The projects in the projects folder, the most recently changed first.</summary>
	public async Task<IReadOnlyList<ProjectInfo>> ListAsync(CancellationToken cancellationToken = default)
	{
		var directory = _paths.ProjectsDirectory;

		if (!Directory.Exists(directory))
			return [];

		var projects = new List<ProjectInfo>();

		foreach (var file in Directory.EnumerateFiles(directory, "*" + FileExtension))
		{
			var modified = File.GetLastWriteTime(file);
			var unreadable = new ProjectInfo(file, Path.GetFileNameWithoutExtension(file), null, modified, false);

			try
			{
				var result = await _store.LoadAsync(file, cancellationToken).ConfigureAwait(true);

				projects.Add(result.Content is { } content
					? new ProjectInfo(file, content.Name, content.Description, modified, true)
					: unreadable);
			}
			catch (Exception exception) when (IsFileProblem(exception))
			{
				projects.Add(unreadable);
			}
		}

		return [.. projects.OrderByDescending(project => project.Modified)];
	}

	/// <summary>Saves what is unsaved before another project replaces it in the editor.</summary>
	private async Task<bool> SaveChangesAsync(CancellationToken cancellationToken) =>
		_workspace.SaveStatus != SaveStatus.Unsaved || await SaveAsync(cancellationToken).ConfigureAwait(true);

	private void Apply(WorkflowContent content, string path)
	{
		_id = content.Id;
		_description = content.Description;
		CurrentPath = path;
		LastError = null;

		_editor.OpenDocument(new GraphDocument(content.Graph, content.Layout));
		_workspace.ProjectName = content.Name;
		_workspace.WorkflowName = null;
		_workspace.SaveStatus = SaveStatus.Saved;
		_editor.FitToViewCommand.Execute(null);
	}

	/// <summary>A file name for a new project: the name made safe, and numbered when the file exists already.</summary>
	private string NewPath(string name)
	{
		var invalid = Path.GetInvalidFileNameChars();
		var safe = new string([.. name.Select(character => invalid.Contains(character) ? '_' : character)]).Trim().TrimEnd('.');

		if (safe.Length == 0)
			safe = "project";

		Directory.CreateDirectory(_paths.ProjectsDirectory);

		var path = Path.Combine(_paths.ProjectsDirectory, safe + FileExtension);

		for (int number = 2; File.Exists(path); number++)
			path = Path.Combine(_paths.ProjectsDirectory, $"{safe} ({number}){FileExtension}");

		return path;
	}

	private static string? CleanDescription(string? description) =>
		string.IsNullOrWhiteSpace(description) ? null : description.Trim();

	private static bool IsFileProblem(Exception exception) => exception is IOException or UnauthorizedAccessException;
}
