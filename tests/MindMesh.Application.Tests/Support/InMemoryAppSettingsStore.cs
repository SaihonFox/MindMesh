using MindMesh.Application.Settings;

namespace MindMesh.Application.Tests.Support;

/// <summary>
/// Settings store that keeps everything in memory and records what was saved.
/// </summary>
internal sealed class InMemoryAppSettingsStore : IAppSettingsStore
{
	private readonly object _gate = new();
	private readonly List<AppSettings> _saved = [];

	public AppSettings ToLoad { get; set; } = new();

	/// <summary>When set, the next save fails with this exception (once).</summary>
	public Exception? FailNextSave { get; set; }

	/// <summary>Completed when the first save succeeds.</summary>
	public TaskCompletionSource FirstSave { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

	public int SaveCount
	{
		get
		{
			lock (_gate)
			{
				return _saved.Count;
			}
		}
	}

	public AppSettings? LastSaved
	{
		get
		{
			lock (_gate)
			{
				return _saved.LastOrDefault();
			}
		}
	}

	public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(ToLoad);

	public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
	{
		if (FailNextSave is { } failure)
		{
			FailNextSave = null;
			return Task.FromException(failure);
		}

		lock (_gate)
		{
			_saved.Add(settings);
		}

		FirstSave.TrySetResult();
		return Task.CompletedTask;
	}
}
