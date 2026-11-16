namespace MindMesh.Presentation.Navigation;

/// <summary>
/// Tells the page factory how to create the page for a key.
/// </summary>
public sealed record PageRegistration(PageKey Key, Func<IServiceProvider, PageViewModel> Create);
