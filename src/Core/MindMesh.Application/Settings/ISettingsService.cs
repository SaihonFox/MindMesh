namespace MindMesh.Application.Settings;

/// <summary>
/// Holds the current settings in memory, notifies about changes and saves them in the background.
/// </summary>
public interface ISettingsService
{
	/// <summary>The current, always valid settings.</summary>
	AppSettings Current { get; }

	/// <summary>Raised synchronously on the calling thread after <see cref="Update"/> changed something.</summary>
	event EventHandler<SettingsChangedEventArgs>? Changed;

	/// <summary>Loads settings from the store. Called once at startup.</summary>
	Task InitializeAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Applies a change. The result is normalized; if nothing changed, no event is raised and nothing is saved.
	/// Saving is delayed and coalesced.
	/// </summary>
	void Update(Func<AppSettings, AppSettings> mutate);

	/// <summary>Saves pending changes immediately. Called on shutdown.</summary>
	Task FlushAsync(CancellationToken cancellationToken = default);
}
