using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MindMesh.Application.DependencyInjection;
using MindMesh.Infrastructure.DependencyInjection;
using MindMesh.Nodes;
using MindMesh.Presentation.DependencyInjection;
using MindMesh.Presentation.Platform;
using MindMesh.UI.Platform;

namespace MindMesh.Desktop;

internal static class Program
{
	private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(10);

	[STAThread]
	public static int Main(string[] args)
	{
		IHost? host = null;

		try
		{
			host = CreateHost(args);

			// Starting the host loads the settings, so everything resolved afterwards sees them.
			host.Start();

			var logger = host.Services.GetRequiredService<ILogger<App>>();
			RegisterGlobalExceptionLogging(logger);
			logger.LogInformation("MindMesh started.");

			BuildAvaloniaApp(host.Services).StartWithClassicDesktopLifetime(args);

			logger.LogInformation("MindMesh stopped.");
			return 0;
		}
		catch (Exception exception)
		{
			ReportStartupFailure(host, exception);
			return 1;
		}
		finally
		{
			StopHost(host);
			Serilog.Log.CloseAndFlush();
		}
	}

	/// <summary>
	/// Entry point for the XAML previewer of IDEs. Without services the main window opens without a view model.
	/// </summary>
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>().UsePlatformDetect();

	private static AppBuilder BuildAvaloniaApp(IServiceProvider services) =>
		AppBuilder.Configure(() => new App(services)).UsePlatformDetect();

	private static IHost CreateHost(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);

		// Serilog replaces the default console/debug/event log providers.
		builder.Logging.ClearProviders();

		builder.Services
			.AddMindMeshInfrastructure()
			.AddMindMeshApplication()
			.AddMindMeshBuiltInNodes();

		// The platform file dialog is registered first, so the presentation layer keeps it instead of its no-op fallback.
		builder.Services.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
		builder.Services.AddSingleton<IShellService, ProcessShellService>();
		builder.Services.AddMindMeshPresentation();

		return builder.Build();
	}

	private static void RegisterGlobalExceptionLogging(ILogger logger)
	{
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
		{
			var exception = e.ExceptionObject as Exception;
			var isTerminating = e.IsTerminating;

			logger.LogCritical(exception, "Unhandled exception (terminating: {IsTerminating}).", isTerminating);
		};

		TaskScheduler.UnobservedTaskException += (_, e) =>
		{
			logger.LogError(e.Exception, "Unobserved task exception.");
			e.SetObserved();
		};
	}

	private static void ReportStartupFailure(IHost? host, Exception exception)
	{
		var logger = host?.Services.GetService<ILoggerFactory>()?.CreateLogger("MindMesh.Startup");

		if (logger is not null)
		{
			logger.LogCritical(exception, "MindMesh failed.");
		}
		else
		{
			// The host is not available, so there is no log file yet; standard error is the last resort.
			Console.Error.WriteLine(exception);
		}
	}

	private static void StopHost(IHost? host)
	{
		if (host is null)
			return;

		try
		{
			// Writes pending settings to disk.
			host.StopAsync(ShutdownTimeout).GetAwaiter().GetResult();
		}
		catch (Exception exception)
		{
			Console.Error.WriteLine(exception);
		}
		finally
		{
			host.Dispose();
		}
	}
}
