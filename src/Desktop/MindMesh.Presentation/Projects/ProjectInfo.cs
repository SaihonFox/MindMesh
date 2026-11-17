namespace MindMesh.Presentation.Projects;

/// <summary>A project file found in the projects folder.</summary>
/// <param name="IsReadable">False when the file could not be read; the name is then the file name.</param>
public sealed record ProjectInfo(string Path, string Name, string? Description, DateTime Modified, bool IsReadable);
