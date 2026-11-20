namespace MindMesh.Presentation.Platform;

/// <summary>
/// Used where there is no file manager (tests, headless runs): it remembers what was asked.
/// </summary>
public sealed class NullShellService : IShellService
{
	public string? LastOpenedFolder { get; private set; }

	public bool OpenFolder(string path)
	{
		LastOpenedFolder = path;

		return true;
	}
}
