using MindMesh.Application.Paths;

namespace MindMesh.Infrastructure.Paths;

/// <summary>
/// Per-user data locations. The data directory can be overridden, which tests rely on.
/// </summary>
public sealed class AppPaths : IAppPaths
{
	public const string SettingsFileName = "settings.json";

	public const string LogsDirectoryName = "logs";

	public AppPaths(string dataDirectory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
		DataDirectory = dataDirectory;
	}

	public string DataDirectory { get; }

	public string SettingsFilePath => Path.Combine(DataDirectory, SettingsFileName);

	public string LogsDirectory => Path.Combine(DataDirectory, LogsDirectoryName);

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

		return new AppPaths(Path.Combine(localData, "MindMesh"));
	}
}
