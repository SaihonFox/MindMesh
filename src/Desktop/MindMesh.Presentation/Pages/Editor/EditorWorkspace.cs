using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Extensions;
using MindMesh.Plugins.Abstractions.Projects;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// State shared by the top bar and the editor: what is open and in which mode it is edited.
/// Names are <c>null</c> until a project or workflow is opened; the UI then shows a localized placeholder.
/// </summary>
public sealed partial class EditorWorkspace : ObservableObject
{
	[ObservableProperty]
	private EditorModeId _mode;

	[ObservableProperty]
	private string? _projectName;

	[ObservableProperty]
	private string? _workflowName;

	[ObservableProperty]
	private SaveStatus _saveStatus = SaveStatus.Saved;

	/// <summary>The project starts in the first mode of the first project kind.</summary>
	public EditorWorkspace(IExtensionCatalog catalog)
	{
		var kind = catalog.ProjectKinds.FirstOrDefault();

		ProjectKind = kind?.Id ?? default;
		_mode = kind?.Modes[0].Id ?? default;
	}

	/// <summary>The kind of the open project.</summary>
	public ProjectKindId ProjectKind { get; }
}
