using MindMesh.Application.Paths;

namespace MindMesh.Infrastructure.Paths;

/// <summary>
/// Per-user data locations. The data directory can be overridden, which tests rely on.
/// </summary>
public sealed class AppPaths : IAppPaths
{
	public const string SettingsFileName = "settings.json";

	public const string LogsDirectoryName = "logs";

	public const string ProjectsDirectoryName = "projects";

	public AppPaths(string dataDirectory, string? projectsDirectory = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
		DataDirectory = dataDirectory;
		ProjectsDirectory = string.IsNullOrWhiteSpace(projectsDirectory)
			? Path.Combine(dataDirectory, ProjectsDirectoryName)
			: projectsDirectory;
	}

	public string DataDirectory { get; }

	public string SettingsFilePath => Path.Combine(DataDirectory, SettingsFileName);

	public string LogsDirectory => Path.Combine(DataDirectory, LogsDirectoryName);

	public string ProjectsDirectory { get; }

	/// <summary>
	/// Creates the default location: the per-user local application data folder of the operating system.
	/// </summary>
	public static AppPaths CreateDefault()
	{
		var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

		if (string.IsNullOrWhiteSpace(localData))
		{
			// Rare: sandboxed environments without a user profile folder.
			localData = AppContext.BaseDirectory;
		}

		// Projects are the user's own files, so they live in Documents where they are easy to find and back up.
		var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		var projects = string.IsNullOrWhiteSpace(documents) ? null : Path.Combine(documents, "MindMesh");

		return new AppPaths(Path.Combine(localData, "MindMesh"), projects);
	}
}
