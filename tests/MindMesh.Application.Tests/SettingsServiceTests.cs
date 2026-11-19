using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Application.Settings;
using MindMesh.Application.Tests.Support;

namespace MindMesh.Application.Tests;

public class SettingsServiceTests
{
	private static readonly TimeSpan LongDelay = TimeSpan.FromHours(1);

	private static SettingsService CreateService(InMemoryAppSettingsStore store, TimeSpan? saveDelay = null) =>
		new(
			store,
			new SettingsServiceOptions { SaveDelay = saveDelay ?? LongDelay },
			TimeProvider.System,
			NullLogger<SettingsService>.Instance);

	[Fact]
	public async Task InitializeAsync_LoadsAndNormalizesSettings()
	{
		var store = new InMemoryAppSettingsStore
		{
			ToLoad = new AppSettings
			{
				Language = "zz",
				Layout = new LayoutSettings { ConsoleHeight = double.NaN },
			},
		};
		var service = CreateService(store);

		await service.InitializeAsync(TestContext.Current.CancellationToken);

		Assert.Equal("auto", service.Current.Language);
		Assert.Equal(LayoutSettings.DefaultConsoleHeight, service.Current.Layout.ConsoleHeight);
	}

	[Fact]
	public async Task InitializeAsync_DoesNotSaveWhatItJustLoaded()
	{
		var store = new InMemoryAppSettingsStore { ToLoad = new AppSettings { Language = "ru" } };
		var service = CreateService(store);

		await service.InitializeAsync(TestContext.Current.CancellationToken);
		await service.FlushAsync(TestContext.Current.CancellationToken);

		Assert.Equal(0, store.SaveCount);
	}

	[Fact]
	public void Update_RaisesChangedWithPreviousAndCurrent()
	{
		var service = CreateService(new InMemoryAppSettingsStore());
		SettingsChangedEventArgs? received = null;
		service.Changed += (_, e) => received = e;

		service.Update(settings => settings with { Language = "ru" });

		Assert.NotNull(received);
		Assert.Equal("auto", received.Previous.Language);
		Assert.Equal("ru", received.Current.Language);
		Assert.Equal("ru", service.Current.Language);
	}

	[Fact]
	public async Task Update_WithSameValue_RaisesNothingAndSavesNothing()
	{
		var store = new InMemoryAppSettingsStore();
		var service = CreateService(store);
		bool raised = false;
		service.Changed += (_, _) => raised = true;

		service.Update(settings => settings);
		await service.FlushAsync(TestContext.Current.CancellationToken);

		Assert.False(raised);
		Assert.Equal(0, store.SaveCount);
	}

	[Fact]
	public void Update_NormalizesTheResult()
	{
		var service = CreateService(new InMemoryAppSettingsStore());

		service.Update(settings => settings with
		{
			Layout = settings.Layout with { NodeLibraryWidth = 99_999 },
		});

		Assert.Equal(LayoutLimits.SidePanelMaxWidth, service.Current.Layout.NodeLibraryWidth);
	}

	[Fact]
	public async Task FlushAsync_SavesSeveralChangesInOneWrite()
	{
		var store = new InMemoryAppSettingsStore();
		var service = CreateService(store);

		service.Update(settings => settings with { Language = "ru" });
		service.Update(settings => settings with { ShowWelcomeOnStartup = false });
		service.Update(settings => settings with { Layout = settings.Layout with { IsSidebarCompact = true } });
		await service.FlushAsync(TestContext.Current.CancellationToken);

		Assert.Equal(1, store.SaveCount);
		Assert.Equal(service.Current, store.LastSaved);
		Assert.Equal("ru", store.LastSaved!.Language);
		Assert.False(store.LastSaved.ShowWelcomeOnStartup);
		Assert.True(store.LastSaved.Layout.IsSidebarCompact);
	}

	[Fact]
	public async Task FlushAsync_WithoutChanges_DoesNotSave()
	{
		var store = new InMemoryAppSettingsStore();
		var service = CreateService(store);

		await service.FlushAsync(TestContext.Current.CancellationToken);

		Assert.Equal(0, store.SaveCount);
	}

	[Fact]
	public async Task FlushAsync_CalledTwice_SavesOnce()
	{
		var store = new InMemoryAppSettingsStore();
		var service = CreateService(store);

		service.Update(settings => settings with { Language = "en" });
		await service.FlushAsync(TestContext.Current.CancellationToken);
		await service.FlushAsync(TestContext.Current.CancellationToken);

		Assert.Equal(1, store.SaveCount);
	}

	[Fact]
	public async Task Update_SavesInTheBackgroundAfterTheDelay()
	{
		var store = new InMemoryAppSettingsStore();
		var service = CreateService(store, TimeSpan.FromMilliseconds(20));

		service.Update(settings => settings with { Language = "ru" });

		await store.FirstSave.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
		Assert.Equal("ru", store.LastSaved!.Language);
	}

	[Fact]
	public async Task FailedSave_IsRetriedByTheNextFlush()
	{
		var store = new InMemoryAppSettingsStore { FailNextSave = new IOException("disk full") };
		var service = CreateService(store);

		service.Update(settings => settings with { Language = "ru" });

		await Assert.ThrowsAsync<IOException>(() => service.FlushAsync(TestContext.Current.CancellationToken));
		Assert.Equal(0, store.SaveCount);

		await service.FlushAsync(TestContext.Current.CancellationToken);

		Assert.Equal(1, store.SaveCount);
		Assert.Equal("ru", store.LastSaved!.Language);
	}

	[Fact]
	public void Update_WithNullDelegate_Throws()
	{
		var service = CreateService(new InMemoryAppSettingsStore());

		Assert.Throws<ArgumentNullException>(() => service.Update(null!));
	}
}
