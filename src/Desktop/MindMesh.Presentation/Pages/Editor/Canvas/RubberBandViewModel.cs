using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// The selection rectangle the user drags on an empty part of the canvas.
/// </summary>
public sealed partial class RubberBandViewModel : ObservableObject
{
	[ObservableProperty]
	private bool _isActive;

	[ObservableProperty]
	private Rect2D _rect;
}
