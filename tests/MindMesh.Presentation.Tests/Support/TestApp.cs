using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Application.AppInfo;
using MindMesh.Application.DependencyInjection;
using MindMesh.Application.Paths;
using MindMesh.Application.Secrets;
using MindMesh.Application.Settings;
using MindMesh.Application.Workflows;
using MindMesh.Infrastructure.Workflows;
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
	private readonly string _projectsDirectory = Path.Combine(Path.GetTempPath(), "mindmesh-tests", Guid.NewGuid().ToString("N"));

	public TestApp(AppSettings? settings = null)
	{
		Settings = new FakeSettingsService(settings);

		var services = new ServiceCollection();
		services.AddSingleton<ISettingsService>(Settings);
		services.AddSingleton<IAppInfo, FakeAppInfo>();
		services.AddSingleton<IAppPaths>(new FakeAppPaths(_projectsDirectory));
		services.AddSingleton<IWorkflowStore, FileWorkflowStore>();
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

	/// <summary>Where the projects of this app are stored; a folder of its own, removed on dispose.</summary>
	public string ProjectsDirectory => _projectsDirectory;

	public void Dispose()
	{
		_provider.Dispose();

		if (Directory.Exists(_projectsDirectory))
		{
			Directory.Delete(_projectsDirectory, recursive: true);
		}
	}

	private sealed class FakeAppInfo : IAppInfo
	{
		public string Name => "MindMesh";

		public string Version => "1.2.3-test";

		public string Copyright => "(c) test";
	}

	private sealed class FakeAppPaths(string projectsDirectory) : IAppPaths
	{
		public string DataDirectory => "/data";

		public string SettingsFilePath => "/data/settings.json";

		public string LogsDirectory => "/data/logs";

		public string ProjectsDirectory => projectsDirectory;
	}
}
