using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MindMesh.Presentation.Platform;

namespace MindMesh.UI.Platform;

/// <summary>
/// Opens the system file dialog on top of the main window.
/// </summary>
public sealed class AvaloniaFilePickerService : IFilePickerService
{
	public async Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
	{
		if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
		{
			return null;
		}

		var files = await TopLevel.GetTopLevel(window)!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = title,
			AllowMultiple = false,
		});

		return files.Count == 0 ? null : files[0].TryGetLocalPath();
	}
}
