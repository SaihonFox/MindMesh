using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MindMesh.Presentation.Platform;

namespace MindMesh.UI.Platform;

/// <summary>
/// Opens the system file dialogs on top of the main window.
/// </summary>
public sealed class AvaloniaFilePickerService : IFilePickerService
{
	public async Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
	{
		if (GetStorage() is not { } storage)
			return null;

		IReadOnlyList<IStorageFile> files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = title,
			AllowMultiple = false,
		});

		return files.Count == 0 ? null : files[0].TryGetLocalPath();
	}

	public async Task<string?> PickImageAsync(string title, CancellationToken cancellationToken = default)
	{
		if (GetStorage() is not { } storage)
			return null;

		IReadOnlyList<IStorageFile> files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = title,
			AllowMultiple = false,
			FileTypeFilter = [FilePickerFileTypes.ImageAll],
		});

		return files.Count == 0 ? null : files[0].TryGetLocalPath();
	}

	public async Task<string?> PickFolderAsync(string title, CancellationToken cancellationToken = default)
	{
		if (GetStorage() is not { } storage)
			return null;

		IReadOnlyList<IStorageFolder> folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
		{
			Title = title,
			AllowMultiple = false,
		});

		return folders.Count == 0 ? null : folders[0].TryGetLocalPath();
	}

	private static IStorageProvider? GetStorage() =>
		Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }
			? TopLevel.GetTopLevel(window)?.StorageProvider
			: null;
}
