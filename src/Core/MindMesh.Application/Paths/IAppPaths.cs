namespace MindMesh.Application.Paths;

/// <summary>
/// Locations of per-user application data.
/// </summary>
public interface IAppPaths
{
	string DataDirectory { get; }

	string SettingsFilePath { get; }

	string LogsDirectory { get; }

	/// <summary>Where new projects are stored as workflow files.</summary>
	string ProjectsDirectory { get; }
}
