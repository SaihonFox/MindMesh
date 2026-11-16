using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Application.AppInfo;
using MindMesh.Application.DependencyInjection;
using MindMesh.Application.Paths;
using MindMesh.Application.Settings;
using MindMesh.Infrastructure.DependencyInjection;
using MindMesh.Infrastructure.Paths;
using MindMesh.Infrastructure.Settings;
using MindMesh.Infrastructure.Tests.Support;

namespace MindMesh.Infrastructure.Tests;

public class ServiceRegistrationTests
{
	private static ServiceProvider BuildProvider(string dataDirectory)
	{
		var services = new ServiceCollection();
		services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
		services.AddMindMeshInfrastructure(new AppPaths(dataDirectory));
		services.AddMindMeshApplication();

		return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
	}

	[Fact]
	public void Container_ResolvesTheInfrastructureServices()
	{
		using var directory = new TemporaryDirectory();
		using var provider = BuildProvider(directory.Path);

		Assert.IsType<JsonAppSettingsStore>(provider.GetRequiredService<IAppSettingsStore>());
		Assert.IsType<SettingsService>(provider.GetRequiredService<ISettingsService>());
		Assert.Equal(directory.Path, provider.GetRequiredService<IAppPaths>().DataDirectory);
	}

	[Fact]
	public void AppInfo_HasNameVersionAndCopyright()
	{
		using var directory = new TemporaryDirectory();
		using var provider = BuildProvider(directory.Path);

		var info = provider.GetRequiredService<IAppInfo>();

		Assert.Equal("MindMesh", info.Name);
		Assert.False(string.IsNullOrWhiteSpace(info.Version));
		Assert.Contains("MindMesh", info.Copyright);
	}

	[Fact]
	public void SettingsService_IsASingleton()
	{
		using var directory = new TemporaryDirectory();
		using var provider = BuildProvider(directory.Path);

		Assert.Same(
			provider.GetRequiredService<ISettingsService>(),
			provider.GetRequiredService<ISettingsService>());
	}

	[Fact]
	public async Task HostedService_LoadsSettingsOnStartAndSavesThemOnStop()
	{
		using var directory = new TemporaryDirectory();
		using var provider = BuildProvider(directory.Path);

		var hosted = Assert.Single(provider.GetServices<IHostedService>());
		var settings = provider.GetRequiredService<ISettingsService>();
		var paths = provider.GetRequiredService<IAppPaths>();

		await hosted.StartAsync(TestContext.Current.CancellationToken);
		settings.Update(current => current with { Language = "ru" });
		await hosted.StopAsync(TestContext.Current.CancellationToken);

		Assert.True(File.Exists(paths.SettingsFilePath));
		Assert.Contains("\"ru\"", await File.ReadAllTextAsync(paths.SettingsFilePath, TestContext.Current.CancellationToken));
	}
}
