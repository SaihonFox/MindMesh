namespace MindMesh.Application.Extensions;

/// <summary>
/// Describes a registration that the catalog rejected.
/// </summary>
/// <param name="ExtensionId">The extension that made the registration.</param>
/// <param name="Message">What was wrong.</param>
public sealed record ExtensionProblem(string ExtensionId, string Message);
