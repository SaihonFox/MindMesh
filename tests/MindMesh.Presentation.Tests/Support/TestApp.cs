using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Application.AppInfo;
using MindMesh.Application.DependencyInjection;
using MindMesh.Application.Paths;
using MindMesh.Application.Secrets;
using MindMesh.Application.Settings;
using MindMesh.Infrastructure.Secrets;
using MindMesh.Nodes;
using MindMesh.Presentation.DependencyInjection;

namespace MindMesh.Presentation.Tests.Support;

/// <summary>
/// The real presentation object graph on top of in-memory fakes of the application and infrastructure layers.
/// </summary>
internal sealed class TestApp : IDisposable
{
	private readonly ServiceProvider _provider;

	public TestApp(AppSettings? settings = null)
	{
		Settings = new FakeSettingsService(settings);

		var services = new ServiceCollection();
		services.AddSingleton<ISettingsService>(Settings);
		services.AddSingleton<IAppInfo, FakeAppInfo>();
		services.AddSingleton<IAppPaths, FakeAppPaths>();
		services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
		services.AddSingleton<ISecretStore, InMemorySecretStore>();

		// The settings fake registered above wins: the application layer only adds what is still missing.
		services.AddMindMeshApplication();
		services.AddMindMeshBuiltInNodes();
		services.AddMindMeshPresentation();

		_provider = services.BuildServiceProvider(new ServiceProviderOptions
		{
			ValidateOnBuild = true,
			ValidateScopes = true,
		});
	}

	public FakeSettingsService Settings { get; }

	public IServiceProvider Services => _provider;

	public T Get<T>()
		where T : notnull => _provider.GetRequiredService<T>();

	public void Dispose() => _provider.Dispose();

	private sealed class FakeAppInfo : IAppInfo
	{
		public string Name => "MindMesh";

		public string Version => "1.2.3-test";

		public string Copyright => "(c) test";
	}

	private sealed class FakeAppPaths : IAppPaths
	{
		public string DataDirectory => "/data";

		public string SettingsFilePath => "/data/settings.json";

		public string LogsDirectory => "/data/logs";
	}
}
