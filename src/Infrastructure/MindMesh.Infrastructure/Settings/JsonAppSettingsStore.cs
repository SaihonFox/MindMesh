using System.Text.Json;
using Microsoft.Extensions.Logging;
using MindMesh.Application.Paths;
using MindMesh.Application.Settings;

namespace MindMesh.Infrastructure.Settings;

/// <summary>
/// Stores settings as an indented JSON file. Writes go through a temporary file so a crash
/// never leaves a half-written settings file behind.
/// </summary>
public sealed class JsonAppSettingsStore : IAppSettingsStore
{
	private const string CorruptFileSuffix = ".corrupt";
	private const string TemporaryFileSuffix = ".tmp";

	private readonly IAppPaths _paths;
	private readonly ILogger<JsonAppSettingsStore> _logger;

	public JsonAppSettingsStore(IAppPaths paths, ILogger<JsonAppSettingsStore> logger)
	{
		_paths = paths;
		_logger = logger;
	}

	public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
	{
		var path = _paths.SettingsFilePath;

		if (!File.Exists(path))
		{
			_logger.LogInformation("Settings file {Path} does not exist; using defaults.", path);
			return new AppSettings();
		}

		try
		{
			await using var stream = new FileStream(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				bufferSize: 4096,
				useAsync: true);

			var settings = await JsonSerializer
				.DeserializeAsync(stream, SettingsJsonContext.Default.AppSettings, cancellationToken)
				.ConfigureAwait(false);

			return settings ?? new AppSettings();
		}
		catch (JsonException exception)
		{
			_logger.LogWarning(exception, "Settings file {Path} is damaged; using defaults.", path);
			MoveCorruptFileAside(path);
			return new AppSettings();
		}
		catch (IOException exception)
		{
			_logger.LogWarning(exception, "Settings file {Path} could not be read; using defaults.", path);
			return new AppSettings();
		}
		catch (UnauthorizedAccessException exception)
		{
			_logger.LogWarning(exception, "No access to settings file {Path}; using defaults.", path);
			return new AppSettings();
		}
	}

	public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(settings);

		var path = _paths.SettingsFilePath;
		var temporaryPath = path + TemporaryFileSuffix;

		Directory.CreateDirectory(_paths.DataDirectory);

		await using (var stream = new FileStream(
						 temporaryPath,
						 FileMode.Create,
						 FileAccess.Write,
						 FileShare.None,
						 bufferSize: 4096,
						 useAsync: true))
		{
			await JsonSerializer
				.SerializeAsync(stream, settings, SettingsJsonContext.Default.AppSettings, cancellationToken)
				.ConfigureAwait(false);
		}

		File.Move(temporaryPath, path, overwrite: true);
	}

	private void MoveCorruptFileAside(string path)
	{
		try
		{
			File.Move(path, path + CorruptFileSuffix, overwrite: true);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			_logger.LogWarning(exception, "Could not move damaged settings file {Path} aside.", path);
		}
	}
}
