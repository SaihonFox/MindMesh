namespace MindMesh.Presentation.Tests.Support;

/// <summary>
/// Finds the repository root (the folder that contains <c>MindMesh.slnx</c>) starting from the test binaries.
/// </summary>
internal static class RepositoryPaths
{
	private static readonly Lazy<string> Root = new(FindRoot);

	public static string RepositoryRoot => Root.Value;

	private static string FindRoot()
	{
		for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
		{
			if (File.Exists(Path.Combine(directory.FullName, "MindMesh.slnx")))
			{
				return directory.FullName;
			}
		}

		throw new InvalidOperationException("MindMesh.slnx was not found above " + AppContext.BaseDirectory);
	}
}
