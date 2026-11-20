namespace MindMesh.Presentation.Platform;

/// <summary>
/// Used where no dialog exists (tests, headless runs): the user never picks anything.
/// </summary>
public sealed class NullFilePickerService : IFilePickerService
{
	public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default) =>
		Task.FromResult<string?>(null);

	public Task<string?> PickFolderAsync(string title, CancellationToken cancellationToken = default) =>
		Task.FromResult<string?>(null);

	public Task<string?> PickImageAsync(string title, CancellationToken cancellationToken = default) =>
		Task.FromResult<string?>(null);
}
