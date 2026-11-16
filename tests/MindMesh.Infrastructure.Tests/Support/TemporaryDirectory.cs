namespace MindMesh.Infrastructure.Tests.Support;

/// <summary>
/// A unique directory under the system temp folder that is deleted when disposed.
/// </summary>
internal sealed class TemporaryDirectory : IDisposable
{
	public TemporaryDirectory()
	{
		Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MindMeshTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(Path);
	}

	public string Path { get; }

	public void Dispose()
	{
		try
		{
			Directory.Delete(Path, recursive: true);
		}
		catch (IOException)
		{
			// Best effort: a locked temp file must not fail the test run.
		}
		catch (UnauthorizedAccessException)
		{
			// Best effort, see above.
		}
	}
}
