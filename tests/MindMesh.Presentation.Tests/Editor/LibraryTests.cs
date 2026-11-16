using MindMesh.Application.Settings;
using MindMesh.Graph.Identifiers;
using MindMesh.Nodes.BuiltIn;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Pages.Editor.Library;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Editor;

public class LibraryTests
{
	[Fact]
	public void TheLibrary_ListsTheCategoriesOfTheCurrentMode_WithTheirNodes()
	{
		using var fixture = new EditorFixture();
		var library = fixture.App.Get<NodeLibraryViewModel>();
		var mode = fixture.Catalog.FindMode(BuiltInExtension.WorkflowMode)!;

		Assert.True(library.HasResults);
		Assert.All(library.Sections, section =>
		{
			Assert.Contains(section.Id, mode.Categories);
			Assert.NotEmpty(section.Items);
			Assert.Equal(section.Items.Count, section.Count);
		});
	}

	[Fact]
	public void OnlyTheFirstSectionStartsOpen()
	{
		using var fixture = new EditorFixture();
		var library = fixture.App.Get<NodeLibraryViewModel>();

		Assert.True(library.Sections[0].IsExpanded);
		Assert.All(library.Sections.Skip(1), section => Assert.False(section.IsExpanded));
	}

	[Fact]
	public void WhatTheUserOpenedStaysOpen_WhenTheModeChangesAndComesBack()
	{
		using var fixture = new EditorFixture();
		var library = fixture.App.Get<NodeLibraryViewModel>();
		var workspace = fixture.App.Get<EditorWorkspace>();
		var second = library.Sections[1];
		second.IsExpanded = true;

		workspace.Mode = BuiltInExtension.AiAgentMode;
		workspace.Mode = BuiltInExtension.WorkflowMode;

		Assert.True(library.Sections.Single(section => section.Id == second.Id).IsExpanded);
	}

	[Fact]
	public void TheLibrary_FollowsTheModeOfTheWorkspace()
	{
		using var fixture = new EditorFixture();
		var library = fixture.App.Get<NodeLibraryViewModel>();
		var workspace = fixture.App.Get<EditorWorkspace>();

		workspace.Mode = BuiltInExtension.AiAgentMode;

		NodeCategoryId[] expected =
		[
			.. fixture.Catalog.FindMode(BuiltInExtension.AiAgentMode)!.Categories
				.Where(id => fixture.Catalog.GetNodes(id).Count > 0),
		];

		Assert.Equal(expected, library.Sections.Select(section => section.Id).ToArray());
	}

	[Fact]
	public void Search_FindsNodesByNameIgnoringCase_AndOpensEverySectionWithAMatch()
	{
		using var fixture = new EditorFixture(new AppSettings { Language = "en" });
		var library = fixture.App.Get<NodeLibraryViewModel>();

		library.SearchText = "  DELAY ";

		Assert.True(library.IsSearching);
		Assert.NotEmpty(library.Sections);
		Assert.All(library.Sections, section => Assert.True(section.IsExpanded));
		Assert.Contains(library.Sections.SelectMany(section => section.Items), item => item.Definition.Id.Value == "logic.delay");
	}

	[Fact]
	public void Search_AlsoMatchesDescriptionsAndKeywords()
	{
		using var fixture = new EditorFixture(new AppSettings { Language = "en" });
		var library = fixture.App.Get<NodeLibraryViewModel>();

		library.SearchText = "postgres";

		Assert.Contains(library.Sections.SelectMany(section => section.Items), item => item.Definition.Id.Value == "db.postgres");
	}

	[Fact]
	public void Search_WithoutMatches_ReportsNothing()
	{
		using var fixture = new EditorFixture();
		var library = fixture.App.Get<NodeLibraryViewModel>();

		library.SearchText = "zzzzzz";

		Assert.Empty(library.Sections);
		Assert.False(library.HasResults);
	}

	[Fact]
	public void Search_UsesTheNamesOfTheCurrentLanguage()
	{
		using var fixture = new EditorFixture(new AppSettings { Language = "ru" });
		var library = fixture.App.Get<NodeLibraryViewModel>();

		library.SearchText = "Задержка";

		Assert.Contains(library.Sections.SelectMany(section => section.Items), item => item.Definition.Id.Value == "logic.delay");
	}

	[Fact]
	public void Titles_AreRefreshedWhenTheLanguageChanges()
	{
		using var fixture = new EditorFixture(new AppSettings { Language = "en" });
		var library = fixture.App.Get<NodeLibraryViewModel>();

		var before = library.Sections.SelectMany(section => section.Items).First(item => item.Definition.Id.Value == "logic.delay").Title;
		fixture.App.Settings.Update(current => current with { Language = "ru" });
		library.SearchText = string.Empty;
		var after = library.Sections.SelectMany(section => section.Items).First(item => item.Definition.Id.Value == "logic.delay").Title;

		Assert.Equal("Задержка", after);
		Assert.NotEqual(before, after);
	}

	[Fact]
	public void Items_AreSortedByTitle()
	{
		using var fixture = new EditorFixture(new AppSettings { Language = "en" });
		var library = fixture.App.Get<NodeLibraryViewModel>();

		foreach (var section in library.Sections)
		{
			var titles = section.Items.Select(item => item.Title).ToList();

			Assert.Equal(titles.OrderBy(title => title, StringComparer.CurrentCultureIgnoreCase).ToList(), titles);
		}
	}

	[Fact]
	public void AddToCanvas_PutsTheNodeInTheMiddleOfTheView_AndAnotherOneDoesNotHideIt()
	{
		using var fixture = new EditorFixture();
		var library = fixture.App.Get<NodeLibraryViewModel>();
		var item = library.Sections[0].Items[0];

		library.AddToCanvasCommand.Execute(item);
		library.AddToCanvasCommand.Execute(item);

		Assert.Equal(2, fixture.Editor.Nodes.Count);
		Assert.NotEqual((fixture.Editor.Nodes[0].X, fixture.Editor.Nodes[0].Y), (fixture.Editor.Nodes[1].X, fixture.Editor.Nodes[1].Y));
		Assert.True(fixture.Editor.Viewport.VisibleWorld.Intersects(fixture.Editor.Nodes[0].Bounds));
	}

	[Fact]
	public void DroppingATile_AddsTheNodeAtThatPoint()
	{
		using var fixture = new EditorFixture();
		var library = fixture.App.Get<NodeLibraryViewModel>();
		var item = library.Sections[0].Items[0];

		library.AddToCanvasAt(item, new Point2D(800, 600));

		var node = Assert.Single(fixture.Editor.Nodes);
		Assert.True(node.Bounds.Contains(new Point2D(800, 600)));
		Assert.Equal(item.Definition.Id, node.Definition.Id);
	}
}
