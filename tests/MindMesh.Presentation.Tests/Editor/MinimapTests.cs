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
}
