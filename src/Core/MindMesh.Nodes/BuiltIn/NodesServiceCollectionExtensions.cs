using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MindMesh.Nodes.BuiltIn;
using MindMesh.Plugins.Abstractions;

namespace MindMesh.Nodes;

public static class NodesServiceCollectionExtensions
{
	/// <summary>Registers the extension with the nodes that ship with MindMesh.</summary>
	public static IServiceCollection AddMindMeshBuiltInNodes(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.TryAddEnumerable(ServiceDescriptor.Singleton<IMindMeshExtension, BuiltInExtension>());
		return services;
	}
}
