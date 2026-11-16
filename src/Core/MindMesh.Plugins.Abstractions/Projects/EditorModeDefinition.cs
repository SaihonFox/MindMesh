using MindMesh.Graph.Identifiers;

namespace MindMesh.Plugins.Abstractions.Projects;

/// <summary>
/// A mode of the editor: it decides which categories the node library shows. Modes share one graph engine.
/// </summary>
/// <param name="Id">Identifier of the mode; built-in modes also use it as the localization key suffix.</param>
/// <param name="Name">Display name when no translation exists.</param>
/// <param name="Categories">Categories shown in the library in this mode, in order.</param>
public sealed record EditorModeDefinition(EditorModeId Id, string Name, IReadOnlyList<NodeCategoryId> Categories);
