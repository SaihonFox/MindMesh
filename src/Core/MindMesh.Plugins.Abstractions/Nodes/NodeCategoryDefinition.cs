using MindMesh.Graph.Identifiers;

namespace MindMesh.Plugins.Abstractions.Nodes;

/// <summary>
/// A group of nodes in the library, such as Triggers or HTTP.
/// </summary>
/// <param name="Id">Identifier; built-in categories also use it as the localization key suffix.</param>
/// <param name="Name">Display name when no translation exists.</param>
/// <param name="IconKey">Key of the category icon.</param>
/// <param name="Tone">Color family of the category.</param>
public sealed record NodeCategoryDefinition(NodeCategoryId Id, string Name, string IconKey, AccentTone Tone);
