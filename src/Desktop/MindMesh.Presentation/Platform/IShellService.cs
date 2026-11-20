namespace MindMesh.Presentation.Platform;

/// <summary>
/// Asks the operating system to show something: a folder in the file manager.
/// </summary>
public interface IShellService
{
	/// <summary>Opens a folder in the file manager. Returns false when it could not be opened.</summary>
	bool OpenFolder(string path);
}
