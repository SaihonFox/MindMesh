namespace MindMesh.Application.Settings;

/// <summary>
/// Persists <see cref="AppSettings"/>. Implementations must never throw for a missing or damaged file
/// on load; they return defaults instead.
/// </summary>
public interface IAppSettingsStore
{
	Task<AppSettings> LoadAsync(CancellationToken cancellationToken);

	Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
