using Microsoft.Extensions.DependencyInjection;
using MindMesh.Application.Paths;
using Serilog;
using Serilog.Events;

namespace MindMesh.Infrastructure.Logging;

public static class LoggingServiceCollectionExtensions
{
	private const string OutputTemplate =
		"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

	private const int RetainedFileCount = 14;

	/// <summary>
	/// Routes all <c>Microsoft.Extensions.Logging</c> output into a daily rolling log file.
	/// </summary>
	public static IServiceCollection AddMindMeshLogging(this IServiceCollection services)
	{
		services.AddSerilog((serviceProvider, configuration) =>
		{
			var paths = serviceProvider.GetRequiredService<IAppPaths>();

			configuration
				.MinimumLevel.Is(GetMinimumLevel())
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.File(
					path: Path.Combine(paths.LogsDirectory, "mindmesh-.log"),
					outputTemplate: OutputTemplate,
					rollingInterval: RollingInterval.Day,
					retainedFileCountLimit: RetainedFileCount,
					shared: true);
		});

		return services;
	}

	private static LogEventLevel GetMinimumLevel() =>
#if DEBUG
		LogEventLevel.Debug;
#else
		LogEventLevel.Information;
#endif
}
