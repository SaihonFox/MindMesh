using Microsoft.Extensions.Hosting;
using MindMesh.Application.Settings;

namespace MindMesh.Infrastructure.Settings;

/// <summary>
/// Loads settings when the host starts and makes sure pending changes are written when it stops.
/// </summary>
public sealed class SettingsLifecycleService : IHostedService
{
	private readonly ISettingsService _settings;

	public SettingsLifecycleService(ISettingsService settings)
	{
		_settings = settings;
	}

	public Task StartAsync(CancellationToken cancellationToken) =>
		_settings.InitializeAsync(cancellationToken);

	public Task StopAsync(CancellationToken cancellationToken) =>
		_settings.FlushAsync(cancellationToken);
}
