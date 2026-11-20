using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Editor;

public class MinimapTests
{
	[Fact]
	public void ASingleNode_IsDrawnSmall_AndNotAcrossTheWholeMinimap()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("trigger.manual", 150, 150);

		var rect = fixture.Editor.Minimap.WorldToMinimap(node.Bounds);

		Assert.True(rect.Width < MinimapViewModel.Width / 5, $"The node is {rect.Width:0.#} wide on a minimap of {MinimapViewModel.Width}.");
		Assert.True(rect.Height < MinimapViewModel.Height / 4, $"The node is {rect.Height:0.#} high on a minimap of {MinimapViewModel.Height}.");
	}

	[Fact]
	public void TheMinimap_StillFitsNodesThatAreFarApart()
	{
		using var fixture = new EditorFixture();
		var near = fixture.Add("trigger.manual", 0, 0);
		var far = fixture.Add("http.request", 6000, 4000);

		var map = fixture.Editor.Minimap;
		var first = map.WorldToMinimap(near.Bounds);
		var second = map.WorldToMinimap(far.Bounds);

		Assert.InRange(first.X, 0, MinimapViewModel.Width);
		Assert.InRange(second.X + second.Width, 0, MinimapViewModel.Width);
		Assert.InRange(second.Y + second.Height, 0, MinimapViewModel.Height);
	}

	[Fact]
	public void DraggingTheFrame_MovesTheCameraByTheDraggedDistance_AndNotFaster()
	{
		using var fixture = new EditorFixture();
		fixture.Add("trigger.manual", 0, 0);
		fixture.Add("http.request", 3000, 2000);

		var map = fixture.Editor.Minimap;
		var viewport = fixture.Editor.Viewport;
		var frame = map.ViewportRect;
		var grab = new Point2D(frame.X + (frame.Width / 2), frame.Y + (frame.Height / 2));
		var centerBefore = viewport.ViewCenterWorld;
		double scale = map.Scale;

		map.BeginDrag(grab);
		map.DragTo(new Point2D(grab.X + 20, grab.Y + 10));
		map.EndDrag();

		var centerAfter = viewport.ViewCenterWorld;

		Assert.Equal(20 / scale, centerAfter.X - centerBefore.X, 3);
		Assert.Equal(10 / scale, centerAfter.Y - centerBefore.Y, 3);
	}
}
