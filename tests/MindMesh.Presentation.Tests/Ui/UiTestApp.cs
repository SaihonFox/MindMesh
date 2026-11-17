using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace MindMesh.Presentation.Tests.Ui;

/// <summary>
/// The application of the UI tests: the same theme and styles as the real one, drawn by Skia without a screen.
/// </summary>
public sealed class UiTestApp : Avalonia.Application
{
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<UiTestApp>()
			.UseSkia()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });

	public override void Initialize()
	{
		RequestedThemeVariant = ThemeVariant.Light;

		var baseUri = new Uri("avares://MindMesh.UI/");

		Resources.MergedDictionaries.Add(new ResourceInclude(baseUri) { Source = new Uri("avares://MindMesh.UI/Themes/MindMeshColors.axaml") });
		Resources.MergedDictionaries.Add(new ResourceInclude(baseUri) { Source = new Uri("avares://MindMesh.UI/Themes/MindMeshTokens.axaml") });

		Styles.Add(new FluentTheme());
		Styles.Add(new StyleInclude(baseUri) { Source = new Uri("avares://MindMesh.UI/Themes/MindMeshStyles.axaml") });
	}
}
