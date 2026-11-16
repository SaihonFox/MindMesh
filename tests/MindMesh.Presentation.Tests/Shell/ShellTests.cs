using MindMesh.Application.Settings;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Shell;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Shell;

public class ShellTests
{
	[Fact]
	public void MainWindow_ShowsTheWelcomePageFirstByDefault()
	{
		using var app = new TestApp();

		var window = app.Get<MainWindowViewModel>();

		Assert.Equal(PageKey.Welcome, window.Navigation.CurrentPageKey);
		Assert.Equal("MindMesh", window.WindowTitle);
	}

	[Fact]
	public void MainWindow_OpensTheEditorWhenTheWelcomePageIsDisabled()
	{
		using var app = new TestApp(new AppSettings { ShowWelcomeOnStartup = false });

		var window = app.Get<MainWindowViewModel>();

		Assert.Equal(PageKey.Editor, window.Navigation.CurrentPageKey);
	}

	[Fact]
	public void Sidebar_ListsTheMainSectionsAndSettingsInTheFooter()
	{
		using var app = new TestApp();

		var sidebar = app.Get<SidebarViewModel>();

		PageKey[] expected =
		[
			PageKey.Projects,
			PageKey.Editor,
			PageKey.Templates,
			PageKey.Marketplace,
			PageKey.Executions,
			PageKey.Plugins,
		];

		Assert.Equal(expected, sidebar.Items.Select(item => item.PageKey).ToArray());
		Assert.Equal(PageKey.Settings, Assert.Single(sidebar.FooterItems).PageKey);
	}

	[Fact]
	public void Sidebar_ItemCommand_Navigates()
	{
		using var app = new TestApp();
		var sidebar = app.Get<SidebarViewModel>();
		var navigation = app.Get<INavigationService>();

		sidebar.Items.First(item => item.PageKey == PageKey.Templates).ActivateCommand.Execute(null);

		Assert.Equal(PageKey.Templates, navigation.CurrentPageKey);
	}

	[Fact]
	public void Sidebar_MarksOnlyTheCurrentPageAsSelected()
	{
		using var app = new TestApp();
		var sidebar = app.Get<SidebarViewModel>();
		var navigation = app.Get<INavigationService>();

		navigation.NavigateTo(PageKey.Plugins);
		Assert.Equal(PageKey.Plugins, Assert.Single(sidebar.Items, item => item.IsSelected).PageKey);

		navigation.NavigateTo(PageKey.Settings);
		Assert.DoesNotContain(sidebar.Items, item => item.IsSelected);
		Assert.True(sidebar.FooterItems[0].IsSelected);
	}

	[Fact]
	public void Sidebar_Toggle_ChangesWidthLabelsAndStoresTheChoice()
	{
		using var app = new TestApp();
		var sidebar = app.Get<SidebarViewModel>();

		Assert.False(sidebar.IsCompact);
		Assert.Equal(SidebarViewModel.ExpandedWidth, sidebar.Width);

		sidebar.ToggleCompactCommand.Execute(null);

		Assert.True(sidebar.IsCompact);
		Assert.Equal(SidebarViewModel.CompactWidth, sidebar.Width);
		Assert.All(sidebar.Items, item => Assert.False(item.ShowLabel));
		Assert.True(app.Settings.Current.Layout.IsSidebarCompact);
	}

	[Fact]
	public void Sidebar_RestoresTheStoredCompactState()
	{
		using var app = new TestApp(new AppSettings { Layout = new LayoutSettings { IsSidebarCompact = true } });

		var sidebar = app.Get<SidebarViewModel>();

		Assert.True(sidebar.IsCompact);
		Assert.All(sidebar.Items, item => Assert.False(item.ShowLabel));
	}

	[Fact]
	public void Sidebar_TitlesFollowTheLanguage()
	{
		using var app = new TestApp(new AppSettings { Language = "en" });
		var sidebar = app.Get<SidebarViewModel>();
		var item = sidebar.Items[0];
		var changed = new List<string?>();
		item.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

		Assert.Equal("Projects", item.Title);

		app.Settings.Update(current => current with { Language = "ru" });

		Assert.Equal("Проекты", item.Title);
		Assert.Contains(nameof(item.Title), changed);
	}

	[Fact]
	public void TopBar_ShowsPlaceholderNamesUntilSomethingIsOpened()
	{
		using var app = new TestApp(new AppSettings { Language = "en" });
		var topBar = app.Get<TopBarViewModel>();
		var workspace = app.Get<EditorWorkspace>();

		Assert.Equal("Untitled project", topBar.ProjectDisplayName);

		workspace.ProjectName = "Invoices";

		Assert.Equal("Invoices", topBar.ProjectDisplayName);
	}

	[Fact]
	public void TopBar_ToolbarIsOnlyActiveInTheEditor()
	{
		using var app = new TestApp();
		var topBar = app.Get<TopBarViewModel>();
		var navigation = app.Get<INavigationService>();
		var raised = false;
		topBar.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(TopBarViewModel.IsEditorActive);

		navigation.NavigateTo(PageKey.Welcome);
		Assert.False(topBar.IsEditorActive);

		navigation.NavigateTo(PageKey.Editor);

		Assert.True(topBar.IsEditorActive);
		Assert.True(raised);
	}

	[Fact]
	public void TopBar_Commands_Navigate()
	{
		using var app = new TestApp();
		var topBar = app.Get<TopBarViewModel>();
		var navigation = app.Get<INavigationService>();

		topBar.OpenSettingsCommand.Execute(null);
		Assert.Equal(PageKey.Settings, navigation.CurrentPageKey);

		topBar.OpenAboutCommand.Execute(null);
		Assert.Equal(PageKey.About, navigation.CurrentPageKey);

		topBar.GoHomeCommand.Execute(null);
		Assert.Equal(PageKey.Welcome, navigation.CurrentPageKey);
	}

	[Fact]
	public void TopBar_SaveStatusFollowsWorkspaceAndLanguage()
	{
		using var app = new TestApp(new AppSettings { Language = "en" });
		var topBar = app.Get<TopBarViewModel>();
		var workspace = app.Get<EditorWorkspace>();
		var before = topBar.SaveStatusText;

		workspace.SaveStatus = SaveStatus.Unsaved;
		var unsaved = topBar.SaveStatusText;
		app.Settings.Update(current => current with { Language = "ru" });

		Assert.NotEqual(before, unsaved);
		Assert.NotEqual(unsaved, topBar.SaveStatusText);
	}
}
