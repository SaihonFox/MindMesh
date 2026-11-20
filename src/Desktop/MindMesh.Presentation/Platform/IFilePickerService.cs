namespace MindMesh.Presentation.Platform;

/// <summary>
/// Lets view models ask the user to choose a file without knowing about windows or dialogs.
/// </summary>
public interface IFilePickerService
{
	/// <summary>Asks for a file. Returns its path, or <c>null</c> if the user cancelled or no dialog is available.</summary>
	Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default);

	/// <summary>Asks for a folder. Returns its path, or <c>null</c> if the user cancelled or no dialog is available.</summary>
	Task<string?> PickFolderAsync(string title, CancellationToken cancellationToken = default);

	/// <summary>Asks for an image. Returns its path, or <c>null</c> if the user cancelled or no dialog is available.</summary>
	Task<string?> PickImageAsync(string title, CancellationToken cancellationToken = default);
}
