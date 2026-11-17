using MindMesh.Application.Workflows;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Projects;
using MindMesh.Presentation.Shell;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Projects;

public class ProjectTests
{
	[Fact]
	public async Task NothingIsCreated_UntilTheDialogIsConfirmed()
	{
		using var fixture = new EditorFixture();
		var dialog = fixture.App.Get<NewProjectDialogViewModel>();

		dialog.OpenCommand.Execute(null);
		dialog.Name = "Daily report";

		Assert.True(dialog.IsOpen);
		Assert.False(Directory.Exists(fixture.App.ProjectsDirectory));
		Assert.True(dialog.CreateCommand.CanExecute(null));

		await dialog.CreateCommand.ExecuteAsync(null);

		Assert.False(dialog.IsOpen);
		Assert.Single(Directory.GetFiles(fixture.App.ProjectsDirectory, "*.mmflow"));
		Assert.Equal("Daily report", fixture.App.Get<EditorWorkspace>().ProjectName);
		Assert.Equal(PageKey.Editor, fixture.App.Get<INavigationService>().CurrentPageKey);
	}

	[Fact]
	public void TheDialog_CannotCreateWithoutAName_AndCancelLeavesNothingBehind()
	{
		using var fixture = new EditorFixture();
		var dialog = fixture.App.Get<NewProjectDialogViewModel>();

		dialog.OpenCommand.Execute(null);

		Assert.False(dialog.CreateCommand.CanExecute(null));

		dialog.Name = "   ";

		Assert.False(dialog.CreateCommand.CanExecute(null));

		dialog.CancelCommand.Execute(null);

		Assert.False(dialog.IsOpen);
		Assert.False(Directory.Exists(fixture.App.ProjectsDirectory));
	}

	[Fact]
	public async Task Save_WritesTheGraph_AndAnotherSessionCanOpenIt()
	{
		var copy = Path.Combine(Path.GetTempPath(), "mm-" + Guid.NewGuid().ToString("N") + ".mmflow");

		using (var fixture = new EditorFixture())
		{
			var projects = fixture.App.Get<ProjectService>();

			Assert.True(await projects.CreateAsync("Flow", "About flow", TestContext.Current.CancellationToken));
			fixture.Add("trigger.manual", 100, 100);

			Assert.Equal(SaveStatus.Unsaved, fixture.App.Get<EditorWorkspace>().SaveStatus);
			Assert.True(await projects.SaveAsync(TestContext.Current.CancellationToken));
			Assert.Equal(SaveStatus.Saved, fixture.App.Get<EditorWorkspace>().SaveStatus);

			var result = await fixture.App.Get<IWorkflowStore>().LoadAsync(projects.CurrentPath!, TestContext.Current.CancellationToken);

			Assert.Equal("Flow", result.Content!.Name);
			Assert.Equal("About flow", result.Content.Description);
			Assert.Single(result.Content.Graph.Nodes);

			// The first app removes its folder when it goes away, so the file is copied out for the second one.
			File.Copy(projects.CurrentPath!, copy);
		}

		try
		{
			using var other = new EditorFixture();
			var opened = await other.App.Get<ProjectService>().OpenAsync(copy, TestContext.Current.CancellationToken);

			Assert.NotNull(opened);
			Assert.Equal("Flow", other.App.Get<EditorWorkspace>().ProjectName);
			Assert.Single(other.Editor.Nodes);
			Assert.Equal(SaveStatus.Saved, other.App.Get<EditorWorkspace>().SaveStatus);
		}
		finally
		{
			File.Delete(copy);
		}
	}

	[Fact]
	public async Task Renaming_FromTheTopBar_ChangesTheNameAndSavesTheFile()
	{
		using var fixture = new EditorFixture();
		var projects = fixture.App.Get<ProjectService>();
		var topBar = fixture.App.Get<TopBarViewModel>();

		await projects.CreateAsync("Old name", null, TestContext.Current.CancellationToken);

		topBar.BeginRenameCommand.Execute(null);

		Assert.True(topBar.IsRenaming);
		Assert.Equal("Old name", topBar.RenameText);

		topBar.RenameText = "New name";
		await topBar.CommitRenameCommand.ExecuteAsync(null);

		Assert.False(topBar.IsRenaming);
		Assert.Equal("New name", topBar.ProjectDisplayName);

		var saved = await fixture.App.Get<IWorkflowStore>().LoadAsync(projects.CurrentPath!, TestContext.Current.CancellationToken);

		Assert.Equal("New name", saved.Content!.Name);
	}

	[Fact]
	public async Task Renaming_CanBeCancelled_AndAnEmptyNameIsIgnored()
	{
		using var fixture = new EditorFixture();
		var topBar = fixture.App.Get<TopBarViewModel>();

		await fixture.App.Get<ProjectService>().CreateAsync("Keep", null, TestContext.Current.CancellationToken);

		topBar.BeginRenameCommand.Execute(null);
		topBar.RenameText = "Other";
		topBar.CancelRenameCommand.Execute(null);

		Assert.Equal("Keep", topBar.ProjectDisplayName);

		topBar.BeginRenameCommand.Execute(null);
		topBar.RenameText = "  ";
		await topBar.CommitRenameCommand.ExecuteAsync(null);

		Assert.Equal("Keep", topBar.ProjectDisplayName);
	}

	[Fact]
	public async Task TheProjectsList_ShowsFiles_AndRenamesThemInPlace()
	{
		using var fixture = new EditorFixture();
		var projects = fixture.App.Get<ProjectService>();

		await projects.CreateAsync("First", null, TestContext.Current.CancellationToken);
		await projects.CreateAsync("Second", "Notes", TestContext.Current.CancellationToken);

		var page = fixture.App.Get<ProjectsPageViewModel>();

		await page.RefreshCommand.ExecuteAsync(null);

		Assert.Equal(2, page.Items.Count);
		Assert.False(page.IsEmpty);

		var item = page.Items.Single(project => project.Name == "First");

		item.BeginRenameCommand.Execute(null);
		item.RenameText = "First, renamed";
		await item.CommitRenameCommand.ExecuteAsync(null);

		Assert.Contains(page.Items, project => project.Name == "First, renamed");
		Assert.DoesNotContain(page.Items, project => project.Name == "First");
	}

	[Fact]
	public async Task SameNames_GetDifferentFiles()
	{
		using var fixture = new EditorFixture();
		var projects = fixture.App.Get<ProjectService>();

		await projects.CreateAsync("Twin", null, TestContext.Current.CancellationToken);
		await projects.CreateAsync("Twin", null, TestContext.Current.CancellationToken);

		Assert.Equal(2, Directory.GetFiles(fixture.App.ProjectsDirectory, "*.mmflow").Length);
	}

	[Fact]
	public async Task AFileNameWithBadCharacters_StillWorks()
	{
		using var fixture = new EditorFixture();

		Assert.True(await fixture.App.Get<ProjectService>().CreateAsync("a/b:c*?", null, TestContext.Current.CancellationToken));
		Assert.Single(Directory.GetFiles(fixture.App.ProjectsDirectory, "*.mmflow"));
	}
}
