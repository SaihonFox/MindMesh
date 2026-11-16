using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Pages.Placeholder;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Navigation;

public class NavigationTests
{
	private sealed class RecordingPage : PageViewModel
	{
		public RecordingPage(PageKey key, ILocalizer localizer, List<string> log)
			: base(key, localizer)
		{
			Log = log;
		}

		public List<string> Log { get; }

		public override void OnNavigatedTo() => Log.Add($"to:{Key}");

		public override void OnNavigatedFrom() => Log.Add($"from:{Key}");
	}

	private sealed class RecordingFactory : IPageFactory
	{
		private readonly ILocalizer _localizer;

		public RecordingFactory(ILocalizer localizer)
		{
			_localizer = localizer;
		}

		public List<string> Log { get; } = [];

		public int Created { get; private set; }

		public PageViewModel Create(PageKey key)
		{
			Created++;
			return new RecordingPage(key, _localizer, Log);
		}
	}

	private static (NavigationService Service, RecordingFactory Factory) CreateService()
	{
		var localizer = new Localizer(new FakeSettingsService());
		var factory = new RecordingFactory(localizer);

		return (new NavigationService(factory, NullLogger<NavigationService>.Instance), factory);
	}

	[Fact]
	public void Initially_NoPageIsShown()
	{
		var (service, _) = CreateService();

		Assert.Null(service.CurrentPage);
		Assert.Null(service.CurrentPageKey);
	}

	[Fact]
	public void NavigateTo_ShowsThePageAndNotifiesIt()
	{
		var (service, factory) = CreateService();

		service.NavigateTo(PageKey.Settings);

		Assert.Equal(PageKey.Settings, service.CurrentPageKey);
		Assert.Equal(PageKey.Settings, service.CurrentPage!.Key);
		string[] expected = ["to:Settings"];

		Assert.Equal(expected, factory.Log.ToArray());
	}

	[Fact]
	public void NavigateTo_LeavesThePreviousPageBeforeEnteringTheNext()
	{
		var (service, factory) = CreateService();

		service.NavigateTo(PageKey.Welcome);
		service.NavigateTo(PageKey.About);

		string[] expected = ["to:Welcome", "from:Welcome", "to:About"];

		Assert.Equal(expected, factory.Log.ToArray());
	}

	[Fact]
	public void NavigateTo_TheCurrentPage_DoesNothing()
	{
		var (service, factory) = CreateService();

		service.NavigateTo(PageKey.Welcome);
		service.NavigateTo(PageKey.Welcome);

		Assert.Equal(1, factory.Created);
		string[] expected = ["to:Welcome"];

		Assert.Equal(expected, factory.Log.ToArray());
	}

	[Fact]
	public void NavigateTo_RaisesPropertyChangedForPageAndKey()
	{
		var (service, _) = CreateService();
		var changed = new List<string?>();
		service.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

		service.NavigateTo(PageKey.Plugins);

		Assert.Contains(nameof(INavigationService.CurrentPage), changed);
		Assert.Contains(nameof(INavigationService.CurrentPageKey), changed);
	}

	[Fact]
	public void PageFactory_CreatesEveryPageKey()
	{
		using var app = new TestApp();
		var factory = app.Get<IPageFactory>();

		foreach (var key in Enum.GetValues<PageKey>())
		{
			var page = factory.Create(key);

			Assert.Equal(key, page.Key);
		}
	}

	[Fact]
	public void PageFactory_WithUnknownKey_Throws()
	{
		using var app = new TestApp();
		var factory = app.Get<IPageFactory>();

		Assert.Throws<InvalidOperationException>(() => factory.Create((PageKey)999));
	}

	[Fact]
	public void PageFactory_WithDuplicateRegistration_Throws()
	{
		using var app = new TestApp();

		var registrations = new[]
		{
			new PageRegistration(PageKey.Welcome, _ => throw new NotSupportedException()),
			new PageRegistration(PageKey.Welcome, _ => throw new NotSupportedException()),
		};

		Assert.Throws<InvalidOperationException>(() => new PageFactory(app.Services, registrations));
	}

	[Fact]
	public void EditorPage_IsASingleton_SoPanelStateSurvivesNavigation()
	{
		using var app = new TestApp();
		var factory = app.Get<IPageFactory>();

		Assert.Same(factory.Create(PageKey.Editor), factory.Create(PageKey.Editor));
	}

	[Fact]
	public void OtherPages_AreCreatedFresh()
	{
		using var app = new TestApp();
		var factory = app.Get<IPageFactory>();

		Assert.NotSame(factory.Create(PageKey.Settings), factory.Create(PageKey.Settings));
		Assert.NotSame(factory.Create(PageKey.Welcome), factory.Create(PageKey.Welcome));
	}

	[Theory]
	[InlineData(PageKey.Projects, 3)]
	[InlineData(PageKey.Executions, 5)]
	[InlineData(PageKey.Templates, 9)]
	[InlineData(PageKey.Marketplace, 9)]
	[InlineData(PageKey.Plugins, 9)]
	public void PlaceholderPages_StateThePhaseThatDeliversThem(PageKey key, int phase)
	{
		using var app = new TestApp();

		var page = Assert.IsType<PlaceholderPageViewModel>(app.Get<IPageFactory>().Create(key));

		Assert.Equal(phase, page.Phase);
		Assert.False(string.IsNullOrWhiteSpace(page.Title));
	}

	[Fact]
	public void EditorPage_IsTheEditorPageViewModel()
	{
		using var app = new TestApp();

		Assert.IsType<EditorPageViewModel>(app.Get<IPageFactory>().Create(PageKey.Editor));
	}
}
