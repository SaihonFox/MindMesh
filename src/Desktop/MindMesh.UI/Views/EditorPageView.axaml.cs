using Avalonia;
using Avalonia.Controls;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor;

namespace MindMesh.UI.Views;

public partial class EditorPageView : UserControl
{
	public EditorPageView()
	{
		InitializeComponent();

		LibraryView.NodeDropped += OnNodeDropped;
	}

	/// <summary>A tile dragged out of the library: add the node where it was released if that is on the canvas.</summary>
	private void OnNodeDropped(object? sender, NodeDroppedEventArgs e)
	{
		if (DataContext is not EditorPageViewModel page
			|| TopLevel.GetTopLevel(this) is not { } topLevel
			|| !CanvasView.TryGetWorldPoint(topLevel, e.WindowPoint, out var world))
		{
			return;
		}

		// The pointer holds the node by its middle, not by its corner.
		var position = new Point2D(world.X - (NodeMetrics.Width / 2), world.Y - (NodeMetrics.HeaderHeight / 2));

		page.Library.AddToCanvasAt(e.Item, position);
	}
}
