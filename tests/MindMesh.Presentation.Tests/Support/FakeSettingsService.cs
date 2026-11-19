using MindMesh.Application.Settings;

namespace MindMesh.Presentation.Tests.Support;

/// <summary>
/// Settings service that works fully in memory: applies and normalizes updates, raises
/// <see cref="Changed"/> and counts changes. Nothing is ever written to disk.
/// </summary>
internal sealed class FakeSettingsService : ISettingsService
{
	public FakeSettingsService(AppSettings? initial = null)
	{
		Current = (initial ?? new AppSettings()).Normalize();
	}

	public event EventHandler<SettingsChangedEventArgs>? Changed;

	public AppSettings Current { get; private set; }

	public int UpdateCount { get; private set; }

	public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	public void Update(Func<AppSettings, AppSettings> mutate)
	{
		var previous = Current;
		var updated = mutate(previous).Normalize();

		if (updated == previous)
			return;

		Current = updated;
		UpdateCount++;
		Changed?.Invoke(this, new SettingsChangedEventArgs(previous, updated));
	}

	public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
