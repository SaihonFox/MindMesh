using Microsoft.Extensions.Logging;

namespace MindMesh.Application.Settings;

/// <inheritdoc />
public sealed class SettingsService : ISettingsService
{
	private readonly IAppSettingsStore _store;
	private readonly SettingsServiceOptions _options;
	private readonly TimeProvider _timeProvider;
	private readonly ILogger<SettingsService> _logger;

	private readonly object _gate = new();
	private readonly SemaphoreSlim _saveLock = new(1, 1);

	private AppSettings _current = new AppSettings().Normalize();
	private CancellationTokenSource? _pendingSave;
	private bool _isDirty;

	public SettingsService(
		IAppSettingsStore store,
		SettingsServiceOptions options,
		TimeProvider timeProvider,
		ILogger<SettingsService> logger)
	{
		_store = store;
		_options = options;
		_timeProvider = timeProvider;
		_logger = logger;
	}

	public event EventHandler<SettingsChangedEventArgs>? Changed;

	public AppSettings Current
	{
		get
		{
			lock (_gate)
			{
				return _current;
			}
		}
	}

	public async Task InitializeAsync(CancellationToken cancellationToken)
	{
		var loaded = await _store.LoadAsync(cancellationToken).ConfigureAwait(false);
		var normalized = loaded.Normalize();

		lock (_gate)
		{
			_current = normalized;
			_isDirty = false;
		}

		_logger.LogInformation("Settings loaded (language: {Language}).", normalized.Language);
	}

	public void Update(Func<AppSettings, AppSettings> mutate)
	{
		ArgumentNullException.ThrowIfNull(mutate);

		AppSettings previous;
		AppSettings updated;

		lock (_gate)
		{
			previous = _current;
			updated = mutate(previous).Normalize();

			if (updated == previous)
			{
				return;
			}

			_current = updated;
			_isDirty = true;
			ScheduleSaveLocked();
		}

		Changed?.Invoke(this, new SettingsChangedEventArgs(previous, updated));
	}

	public async Task FlushAsync(CancellationToken cancellationToken = default)
	{
		lock (_gate)
		{
			_pendingSave?.Cancel();
		}

		await PersistAsync(cancellationToken).ConfigureAwait(false);
	}

	private void ScheduleSaveLocked()
	{
		_pendingSave?.Cancel();
		_pendingSave?.Dispose();

		var source = new CancellationTokenSource();
		_pendingSave = source;

		_ = SaveAfterDelayAsync(source.Token);
	}

	private async Task SaveAfterDelayAsync(CancellationToken delayToken)
	{
		try
		{
			await Task.Delay(_options.SaveDelay, _timeProvider, delayToken).ConfigureAwait(false);

			// The write itself is not cancellable: a newer change only re-schedules a later write.
			await PersistAsync(CancellationToken.None).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (delayToken.IsCancellationRequested)
		{
			// Expected: a newer change replaced this save or FlushAsync saved earlier.
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Could not save settings in the background.");
		}
	}

	private async Task PersistAsync(CancellationToken cancellationToken)
	{
		await _saveLock.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			AppSettings snapshot;

			lock (_gate)
			{
				if (!_isDirty)
				{
					return;
				}

				snapshot = _current;
				_isDirty = false;
			}

			try
			{
				await _store.SaveAsync(snapshot, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				lock (_gate)
				{
					_isDirty = true;
				}

				throw;
			}
		}
		finally
		{
			_saveLock.Release();
		}
	}
}
