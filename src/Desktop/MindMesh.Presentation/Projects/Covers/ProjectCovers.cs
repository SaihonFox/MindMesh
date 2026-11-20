namespace MindMesh.Presentation.Projects.Covers;

/// <summary>
/// The cover picture of a project. It is stored next to the project file as <c>Name.cover.png</c>:
/// either an image the user chose or a random abstract one.
/// </summary>
public sealed class ProjectCovers
{
	public const string Suffix = ".cover.png";

	/// <summary>Where the cover of a project file is stored.</summary>
	public static string CoverPathOf(string projectPath) => Path.ChangeExtension(projectPath, null) + Suffix;

	/// <summary>The cover file of the project, or null when it has none (or it cannot be read).</summary>
	public string? Find(string projectPath)
	{
		string path = CoverPathOf(projectPath);

		return File.Exists(path) ? path : null;
	}

	/// <summary>Makes sure the project has a cover: a random abstract one is created when there is none. Returns its path or null.</summary>
	public async Task<string?> EnsureAsync(string projectPath, CancellationToken cancellationToken = default)
	{
		if (Find(projectPath) is { } existing)
			return existing;

		return await RerollAsync(projectPath, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Replaces the cover with a new random abstract picture. Returns its path, or null when it could not be written.</summary>
	public async Task<string?> RerollAsync(string projectPath, CancellationToken cancellationToken = default)
	{
		string path = CoverPathOf(projectPath);

		try
		{
			byte[] png = await Task.Run(() => CoverGenerator.CreatePng(Random.Shared.Next()), cancellationToken).ConfigureAwait(false);

			await File.WriteAllBytesAsync(path, png, cancellationToken).ConfigureAwait(false);

			return path;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			return null;
		}
	}

	/// <summary>Uses an image file of the user as the cover. Returns the cover path, or null when it could not be copied.</summary>
	public async Task<string?> SetImageAsync(string projectPath, string imagePath, CancellationToken cancellationToken = default)
	{
		string path = CoverPathOf(projectPath);

		try
		{
			byte[] bytes = await File.ReadAllBytesAsync(imagePath, cancellationToken).ConfigureAwait(false);

			await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);

			return path;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			return null;
		}
	}
}
