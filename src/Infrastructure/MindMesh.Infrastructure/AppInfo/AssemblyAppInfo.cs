using System.Reflection;
using MindMesh.Application.AppInfo;

namespace MindMesh.Infrastructure.AppInfo;

/// <summary>
/// Reads application facts from assembly metadata. All projects share one version from Directory.Build.props.
/// </summary>
public sealed class AssemblyAppInfo : IAppInfo
{
	public AssemblyAppInfo()
	{
		var assembly = typeof(AssemblyAppInfo).Assembly;

		Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
				  ?? assembly.GetName().Version?.ToString()
				  ?? "0.0.0";
	}

	public string Name => "MindMesh";

	public string Version { get; }

	public string Copyright => $"© {DateTime.UtcNow.Year} MindMesh";
}
