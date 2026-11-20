namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>One entry of the zoom menu.</summary>
/// <param name="Zoom">The zoom factor, 1 being 100%.</param>
/// <param name="Text">The label shown in the menu.</param>
public sealed record ZoomPreset(double Zoom, string Text);
