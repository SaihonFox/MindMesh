using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MindMesh.Application.Extensions;
using MindMesh.Application.Settings;

namespace MindMesh.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
	/// <summary>
	/// Registers application services. Storage and platform adapters come from the infrastructure layer.
	/// </summary>
	public static IServiceCollection AddMindMeshApplication(this IServiceCollection services)
	{
		services.TryAddSingleton(TimeProvider.System);
		services.TryAddSingleton(new SettingsServiceOptions());
		services.TryAddSingleton<ISettingsService, SettingsService>();
		services.TryAddSingleton<IExtensionCatalog, ExtensionCatalog>();

		return services;
	}
}
