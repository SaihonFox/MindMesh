using Microsoft.Extensions.DependencyInjection;
using MindMesh.Application.AppInfo;
using MindMesh.Application.Paths;
using MindMesh.Application.Secrets;
using MindMesh.Application.Settings;
using MindMesh.Infrastructure.AppInfo;
using MindMesh.Infrastructure.Logging;
using MindMesh.Infrastructure.Paths;
using MindMesh.Infrastructure.Secrets;
using MindMesh.Infrastructure.Settings;

namespace MindMesh.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
	/// <summary>
	/// Registers file system storage, logging and host lifecycle hooks.
	/// </summary>
	/// <param name="services">Service collection.</param>
	/// <param name="paths">Data locations; the per-user default is used when omitted.</param>
	public static IServiceCollection AddMindMeshInfrastructure(
		this IServiceCollection services,
		IAppPaths? paths = null)
	{
		services.AddSingleton(paths ?? AppPaths.CreateDefault());
		services.AddSingleton<IAppInfo, AssemblyAppInfo>();
		services.AddSingleton<IAppSettingsStore, JsonAppSettingsStore>();
		services.AddSingleton<ISecretStore, InMemorySecretStore>();
		services.AddHostedService<SettingsLifecycleService>();
		services.AddMindMeshLogging();

		return services;
	}
}
