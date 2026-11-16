using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Application.Settings;
using MindMesh.Infrastructure.Paths;
using MindMesh.Infrastructure.Settings;
using MindMesh.Infrastructure.Tests.Support;

namespace MindMesh.Infrastructure.Tests;

public class JsonAppSettingsStoreTests : IDisposable
{
	private readonly TemporaryDirectory _directory = new();
	private readonly AppPaths _paths;
	private readonly JsonAppSettingsStore _store;

	public JsonAppSettingsStoreTests()
	{
		// A nested folder that does not exist yet: the store has to create it.
		_paths = new AppPaths(Path.Combine(_directory.Path, "data"));
		_store = new JsonAppSettingsStore(_paths, NullLogger<JsonAppSettingsStore>.Instance);
	}

	public void Dispose() => _directory.Dispose();

	[Fact]
	public async Task LoadAsync_WithoutFile_ReturnsDefaults()
	{
		var settings = await _store.LoadAsync(TestContext.Current.CancellationToken);

		Assert.Equal(new AppSettings(), settings);
	}

	[Fact]
	public async Task SaveAsync_CreatesDirectoryAndFile()
	{
		await _store.SaveAsync(new AppSettings(), TestContext.Current.CancellationToken);

		Assert.True(File.Exists(_paths.SettingsFilePath));
	}

	[Fact]
	public async Task SaveAsync_ThenLoadAsync_RoundTripsEveryValue()
	{
		var expected = new AppSettings
		{
			Language = "ru",
			ShowWelcomeOnStartup = false,
			Layout = new LayoutSettings
			{
				IsSidebarCompact = true,
				IsNodeLibraryVisible = false,
				IsInspectorVisible = false,
				IsConsoleVisible = false,
				IsAssistantVisible = true,
				NodeLibraryWidth = 310,
				InspectorWidth = 350,
				AssistantWidth = 400,
				ConsoleHeight = 180,
			},
		};

		await _store.SaveAsync(expected, TestContext.Current.CancellationToken);
		var actual = await _store.LoadAsync(TestContext.Current.CancellationToken);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task SaveAsync_WritesTheThemeAsText()
	{
		await _store.SaveAsync(new AppSettings { Theme = ThemeMode.Dark }, TestContext.Current.CancellationToken);

		var json = await File.ReadAllTextAsync(_paths.SettingsFilePath, TestContext.Current.CancellationToken);

		Assert.Contains("\"Dark\"", json);
	}

	[Fact]
	public async Task SaveAsync_LeavesNoTemporaryFileBehind()
	{
		await _store.SaveAsync(new AppSettings(), TestContext.Current.CancellationToken);
		await _store.SaveAsync(new AppSettings { Language = "en" }, TestContext.Current.CancellationToken);

		var files = Directory.GetFiles(_paths.DataDirectory);

		var single = Assert.Single(files);
		Assert.Equal("settings.json", Path.GetFileName(single));
	}

	[Fact]
	public async Task SaveAsync_OverwritesAnExistingFile()
	{
		await _store.SaveAsync(new AppSettings { Language = "ru" }, TestContext.Current.CancellationToken);
		await _store.SaveAsync(new AppSettings { Language = "en" }, TestContext.Current.CancellationToken);

		var loaded = await _store.LoadAsync(TestContext.Current.CancellationToken);

		Assert.Equal("en", loaded.Language);
	}

	[Fact]
	public async Task LoadAsync_WithDamagedFile_ReturnsDefaultsAndKeepsACopy()
	{
		Directory.CreateDirectory(_paths.DataDirectory);
		await File.WriteAllTextAsync(_paths.SettingsFilePath, "{ this is not json", TestContext.Current.CancellationToken);

		var settings = await _store.LoadAsync(TestContext.Current.CancellationToken);

		Assert.Equal(new AppSettings(), settings);
		Assert.False(File.Exists(_paths.SettingsFilePath));
		Assert.True(File.Exists(_paths.SettingsFilePath + ".corrupt"));
	}

	[Fact]
	public async Task LoadAsync_WithEmptyJsonObject_ReturnsDefaults()
	{
		Directory.CreateDirectory(_paths.DataDirectory);
		await File.WriteAllTextAsync(_paths.SettingsFilePath, "{}", TestContext.Current.CancellationToken);

		var settings = await _store.LoadAsync(TestContext.Current.CancellationToken);

		Assert.Equal(new AppSettings(), settings);
	}

	[Fact]
	public async Task LoadAsync_IgnoresUnknownProperties()
	{
		Directory.CreateDirectory(_paths.DataDirectory);
		await File.WriteAllTextAsync(
			_paths.SettingsFilePath,
			"{ \"language\": \"ru\", \"fromTheFuture\": 42 }",
			TestContext.Current.CancellationToken);

		var settings = await _store.LoadAsync(TestContext.Current.CancellationToken);

		Assert.Equal("ru", settings.Language);
	}

	[Fact]
	public async Task SaveAsync_WithCancelledToken_Throws()
	{
		using var source = new CancellationTokenSource();
		await source.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => _store.SaveAsync(new AppSettings(), source.Token));
	}

	[Fact]
	public async Task SaveAsync_WithNull_Throws()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() => _store.SaveAsync(null!, TestContext.Current.CancellationToken));
	}
}
