using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MindMesh.Application.Settings;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Shell;
using MindMesh.Presentation.Tests.Support;
using MindMesh.UI.Controls;
using MindMesh.UI.Views;

namespace MindMesh.Presentation.Tests.Ui;

/// <summary>
/// Draws the real main window with a few nodes on the canvas and looks at the pixels.
/// Setting MM_UI_SNAPSHOT=1 turns the report into a failure message, which is how a CI log shows the screen.
/// </summary>
public class EditorRenderTests
{
	private sealed record RenderReport(bool NodesAreDrawn, string Text);

	private static RenderReport Render(Action<EditorFixture>? arrange = null)
	{
		using var fixture = new EditorFixture(new AppSettings { ShowWelcomeOnStartup = false });
		var model = fixture.App.Get<MainWindowViewModel>();
		var window = new MainWindow { DataContext = model, Width = 1440, Height = 860 };

		window.Show();
		Pump();

		var trigger = fixture.Add("trigger.manual", 150, 150);
		var request = fixture.Add("http.request", 520, 150);
		var branch = fixture.Add("logic.if", 520, 420);
		fixture.Drag(EditorFixture.Output(trigger, PortType.Execution), EditorFixture.Input(request, PortType.Execution));
		arrange?.Invoke(fixture);
		Pump();
		fixture.Editor.FitToViewCommand.Execute(null);
		Pump();

		var snapshot = UiSnapshot.Capture(window);
		var cards = window.GetVisualDescendants().OfType<NodeCard>().ToList();
		var report = new StringBuilder();

		report.AppendLine($"Window {snapshot.Width}x{snapshot.Height}, nodes in the model: {fixture.Editor.Nodes.Count}, cards in the visual tree: {cards.Count}");
		report.AppendLine($"Viewport: zoom {fixture.Editor.Viewport.Zoom:0.###}, offset ({fixture.Editor.Viewport.OffsetX:0.#}, {fixture.Editor.Viewport.OffsetY:0.#}), view {fixture.Editor.Viewport.ViewWidth:0.#}x{fixture.Editor.Viewport.ViewHeight:0.#}");

		foreach (var node in fixture.Editor.Nodes)
		{
			report.AppendLine($"  node {node.Title}: world ({node.X:0.#}, {node.Y:0.#}), inViewport={node.IsInViewport}");
		}

		report.AppendLine(UiSnapshot.Describe(window, typeof(NodeCard), typeof(ConnectionsLayer), typeof(MinimapControl), typeof(CanvasGridBackground), typeof(CanvasOverlay)));

		var drawn = cards.Count >= 3;

		foreach (var card in cards)
		{
			var origin = card.TranslatePoint(default, window) ?? default;
			var ink = snapshot.CountInk(new Rect(origin, card.Bounds.Size));

			report.AppendLine($"  card at ({origin.X:0.#}, {origin.Y:0.#}) has {ink} dark pixels");
			drawn &= card.IsEffectivelyVisible && card.Bounds.Width > 0 && ink > 30;
		}

		foreach (var key in new[] { "MM.Brush.Node", "MM.Brush.Border", "MM.Brush.TextPrimary", "MM.Brush.TextMuted", "MM.Brush.Canvas", "MM.Shadow.Node", "MM.Brush.Accent" })
		{
			var found = Avalonia.Application.Current!.TryGetResource(key, Avalonia.Styling.ThemeVariant.Light, out var value);

			report.AppendLine($"  resource {key}: {(found ? value?.GetType().Name + " " + value : "MISSING")}");
		}

		if (cards.Count > 0)
		{
			var first = cards[0];
			report.AppendLine($"  first card: Background={first.Background} BorderBrush={first.BorderBrush} Thickness={first.BorderThickness} Corner={first.CornerRadius} Classes=[{string.Join(' ', first.Classes)}] Child={first.Child?.GetType().Name}");

			foreach (var text in first.GetVisualDescendants().OfType<TextBlock>())
			{
				var origin = text.TranslatePoint(default, window) ?? default;
				report.AppendLine($"    text '{text.Text}' foreground={text.Foreground} at ({origin.X:0.#}, {origin.Y:0.#}) size {text.Bounds.Width:0.#}x{text.Bounds.Height:0.#} visible={text.IsEffectivelyVisible}");
			}

		}

		report.AppendLine(snapshot.PngBase64());

		return new RenderReport(drawn, report.ToString());
	}

	/// <summary>Layout and drawing take a few turns: new controls are created while measuring and drawn one tick later.</summary>
	private static void Pump()
	{
		for (var turn = 0; turn < 4; turn++)
		{
			Dispatcher.UIThread.RunJobs();
			AvaloniaHeadlessPlatform.ForceRenderTimerTick();
		}

		Dispatcher.UIThread.RunJobs();
	}

	[Fact]
	public async Task TheNodesOfTheGraphAreDrawnOnTheCanvas()
	{
		var report = await UiSession.Run(() => Render());

		Assert.True(report.NodesAreDrawn, report.Text);
	}

	[Fact]
	public async Task NodesBeyondTheFirstScreenful_AreDrawnToo()
	{
		var inks = await UiSession.Run(() =>
		{
			using var fixture = new EditorFixture(new AppSettings { ShowWelcomeOnStartup = false });
			var window = new MainWindow { DataContext = fixture.App.Get<MainWindowViewModel>(), Width = 1440, Height = 860 };

			window.Show();
			Pump();

			// The first one is inside the area that was visible at start; the others are far to the right,
			// below it, and at negative coordinates.
			fixture.Add("trigger.manual", 150, 150);
			fixture.Add("http.request", 1500, 1000);
			fixture.Add("logic.if", -900, -600);
			Pump();
			fixture.Editor.FitToViewCommand.Execute(null);
			Pump();

			var snapshot = UiSnapshot.Capture(window);

			return window.GetVisualDescendants().OfType<NodeCard>()
				.Select(card => snapshot.CountInk(new Rect(card.TranslatePoint(default, window) ?? default, card.Bounds.Size)))
				.ToList();
		});

		Assert.Equal(3, inks.Count);
		Assert.All(inks, ink => Assert.True(ink > 8, $"A node is not drawn; dark pixels per card: {string.Join(", ", inks)}."));
	}

	[Fact]
	public async Task TheScreenCanBeInspected_WhenAskedFor()
	{
		if (Environment.GetEnvironmentVariable("MM_UI_SNAPSHOT") != "1")
		{
			return;
		}

		var report = await UiSession.Run(() => Render());

		Assert.Fail(report.Text);
	}
}
