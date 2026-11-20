namespace MindMesh.Presentation.Projects;

/// <summary>A kind of project offered in the new-project dialog; only the available ones can be chosen.</summary>
/// <param name="Title">The localized name.</param>
/// <param name="Hint">A short localized description, or the "coming soon" note.</param>
/// <param name="IconKey">The icon of the kind.</param>
/// <param name="IsAvailable">False for kinds that are announced but not built yet.</param>
public sealed record ProjectKindOption(string Title, string Hint, string IconKey, bool IsAvailable);
