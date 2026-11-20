using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Shell;
using MindMesh.UI.Views;

namespace MindMesh.Desktop;

// "Avalonia.Application" is written out because the namespace MindMesh.Application would otherwise win the name lookup.
public partial class App : Avalonia.Application
{
	private readonly IServiceProvider? _services;

	/// <summary>Used by the XAML previewer, which has no service provider.</summary>
	public App()
	{
	}

	public App(IServiceProvider services)
	{
		_services = services;
	}

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		IncludeCodeEditorStyles();
		FollowThemeSetting();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow
			{
				DataContext = _services?.GetRequiredService<MainWindowViewModel>(),
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

	/// <summary>
	/// The code editor of the inspector brings its own styles. They are added here and not in XAML, so a missing
	/// style file costs the editor its looks instead of stopping the application at startup.
	/// </summary>
	private void IncludeCodeEditorStyles()
	{
		try
		{
			Styles.Add(new StyleInclude(new Uri("avares://MindMesh.Desktop/"))
			{
				Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml"),
			});
		}
		catch (Exception exception)
		{
			_services?.GetService<ILogger<App>>()?.LogWarning(exception, "The code editor styles could not be loaded.");
		}
	}

	/// <summary>Applies the theme from the settings now and whenever it changes.</summary>
	private void FollowThemeSetting()
	{
		if (_services?.GetService<ISettingsService>() is not { } settings)
			return;

		RequestedThemeVariant = ToVariant(settings.Current.Theme);
		settings.Changed += (_, e) =>
		{
			if (e.Previous.Theme != e.Current.Theme)
				RequestedThemeVariant = ToVariant(e.Current.Theme);
		};
	}

	private static ThemeVariant ToVariant(ThemeMode mode) => mode switch
	{
		ThemeMode.Dark => ThemeVariant.Dark,
		ThemeMode.Midnight => new ThemeVariant("Midnight", ThemeVariant.Dark),
		ThemeMode.Forest => new ThemeVariant("Forest", ThemeVariant.Dark),
		ThemeMode.Ocean => new ThemeVariant("Ocean", ThemeVariant.Light),
		ThemeMode.Rose => new ThemeVariant("Rose", ThemeVariant.Light),
		ThemeMode.System => ThemeVariant.Default,
		_ => ThemeVariant.Light,
	};
}
