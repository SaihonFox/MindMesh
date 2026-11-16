using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Pages.Editor.Inspector;
using MindMesh.Presentation.Pages.Editor.Library;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// The workflow editor screen. It is a single shared instance, so panel state and (later) the open
/// graph survive navigation to other screens.
/// </summary>
public sealed class EditorPageViewModel : PageViewModel
{
	public EditorPageViewModel(
		ILocalizer localizer,
		EditorWorkspace workspace,
		EditorLayoutViewModel layout,
		GraphEditorViewModel editor,
		NodeLibraryViewModel library,
		InspectorViewModel inspector,
		ExecutionConsoleViewModel console,
		AssistantViewModel assistant)
		: base(PageKey.Editor, localizer)
	{
		Workspace = workspace;
		Layout = layout;
		Editor = editor;
		Library = library;
		Inspector = inspector;
		Console = console;
		Assistant = assistant;
	}

	public EditorWorkspace Workspace { get; }

	public EditorLayoutViewModel Layout { get; }

	public GraphEditorViewModel Editor { get; }

	public NodeLibraryViewModel Library { get; }

	public InspectorViewModel Inspector { get; }

	public ExecutionConsoleViewModel Console { get; }

	public AssistantViewModel Assistant { get; }
}
