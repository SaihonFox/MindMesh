namespace MindMesh.Application.AppInfo;

/// <summary>
/// Static facts about the running application.
/// </summary>
public interface IAppInfo
{
	string Name { get; }

	string Version { get; }

	string Copyright { get; }
}
