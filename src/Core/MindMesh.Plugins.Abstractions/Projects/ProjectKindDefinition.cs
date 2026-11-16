namespace MindMesh.Plugins.Abstractions.Projects;

/// <summary>
/// A kind of project the application can create. Phase 2 ships one kind, Workflow; extensions add others.
/// </summary>
/// <param name="Id">Identifier of the kind.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">What projects of this kind are for.</param>
/// <param name="IconKey">Key of the icon.</param>
/// <param name="Modes">Editor modes of the kind. The first one is the default.</param>
public sealed record ProjectKindDefinition(
	ProjectKindId Id,
	string Name,
	string Description,
	string IconKey,
	IReadOnlyList<EditorModeDefinition> Modes);
