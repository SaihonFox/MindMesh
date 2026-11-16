using MindMesh.Infrastructure.Paths;

namespace MindMesh.Infrastructure.Tests;

public class AppPathsTests
{
	[Fact]
	public void Paths_AreBuiltFromTheDataDirectory()
	{
		var root = Path.Combine(Path.GetTempPath(), "mindmesh-paths");
		var paths = new AppPaths(root);

		Assert.Equal(root, paths.DataDirectory);
		Assert.Equal(Path.Combine(root, "settings.json"), paths.SettingsFilePath);
		Assert.Equal(Path.Combine(root, "logs"), paths.LogsDirectory);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithBlankDirectory_Throws(string directory)
	{
		Assert.Throws<ArgumentException>(() => new AppPaths(directory));
	}

	[Fact]
	public void Constructor_WithNull_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => new AppPaths(null!));
	}

	[Fact]
	public void CreateDefault_EndsWithTheProductFolder()
	{
		var paths = AppPaths.CreateDefault();

		Assert.Equal("MindMesh", Path.GetFileName(paths.DataDirectory));
	}
}
