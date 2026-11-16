namespace MindMesh.Plugins.Abstractions;

/// <summary>
/// Who provides an extension.
/// </summary>
/// <param name="Id">Unique identifier, for example <c>mindmesh.builtin</c>.</param>
/// <param name="Name">Display name.</param>
/// <param name="Version">Version of the extension.</param>
/// <param name="Author">Author or vendor.</param>
/// <param name="IsBuiltIn">True for content that ships with the application.</param>
public sealed record ExtensionInfo(string Id, string Name, Version Version, string Author, bool IsBuiltIn = false);
