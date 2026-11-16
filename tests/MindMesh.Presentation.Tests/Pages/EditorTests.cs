using MindMesh.Application.Settings;
using MindMesh.Application.Extensions;
using MindMesh.Graph.Identifiers;
using MindMesh.Nodes.BuiltIn;
using MindMesh.Plugins.Abstractions;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Plugins.Abstractions.Projects;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Pages;

public class EditorTests
{
	[Fact]
	public void Layout_StartsFromTheStoredSettings()
	{
		using var app = new TestApp(new AppSettings
		{
			Layout = new LayoutSettings
			{
				IsInspectorVisible = false,
				IsAssistantVisible = true,
				NodeLibraryWidth = 333,
				ConsoleHeight = 150,
			},
		});

		var layout = app.Get<EditorLayoutViewModel>();

		Assert.False(layout.IsInspectorVisible);
		Assert.True(layout.IsAssistantVisible);
		Assert.Equal(333, layout.NodeLibraryWidth);
		Assert.Equal(150, layout.ConsoleHeight);
	}

	[Fact]
	public void Layout_ToggleCommands_FlipVisibilityAndStoreIt()
	{
		using var app = new TestApp();
		var layout = app.Get<EditorLayoutViewModel>();

		layout.ToggleNodeLibraryCommand.Execute(null);
		layout.ToggleInspectorCommand.Execute(null);
		layout.ToggleConsoleCommand.Execute(null);
		layout.ToggleAssistantCommand.Execute(null);

		var stored = app.Settings.Current.Layout;
		Assert.False(stored.IsNodeLibraryVisible);
		Assert.False(stored.IsInspectorVisible);
		Assert.False(stored.IsConsoleVisible);
		Assert.True(stored.IsAssistantVisible);
	}

	[Fact]
	public void Layout_ResizingIsStoredAndClamped()
	{
		using var app = new TestApp();
		var layout = app.Get<EditorLayoutViewModel>();

		layout.InspectorWidth = 410;
		layout.ConsoleHeight = 99_999;

		Assert.Equal(410, app.Settings.Current.Layout.InspectorWidth);
		Assert.Equal(LayoutLimits.ConsoleMaxHeight, app.Settings.Current.Layout.ConsoleHeight);
	}

	[Fact]
	public void Layout_Reset_RestoresDefaults()
	{
		using var app = new TestApp();
		var layout = app.Get<EditorLayoutViewModel>();
		layout.NodeLibraryWidth = 500;
		layout.IsConsoleVisible = false;
		layout.IsAssistantVisible = true;

		layout.ResetLayoutCommand.Execute(null);

		Assert.Equal(new LayoutSettings(), app.Settings.Current.Layout);
		Assert.Equal(LayoutSettings.DefaultNodeLibraryWidth, layout.NodeLibraryWidth);
	}

	[Fact]
	public void Toolbar_ModeSwitchUpdatesTheWorkspaceAndTheOtherWayRound()
	{
		using var app = new TestApp();
		var toolbar = app.Get<EditorToolbarViewModel>();
		var workspace = app.Get<EditorWorkspace>();

		toolbar.SelectedMode = toolbar.Modes.First(option => option.Mode == BuiltInExtension.LogicMode);
		Assert.Equal(BuiltInExtension.LogicMode, workspace.Mode);

		workspace.Mode = BuiltInExtension.DataFlowMode;
		Assert.Equal(BuiltInExtension.DataFlowMode, toolbar.SelectedMode!.Mode);
	}

	[Fact]
	public void Toolbar_OffersEveryModeOfTheProjectKind()
	{
		using var app = new TestApp();
		var toolbar = app.Get<EditorToolbarViewModel>();
		var catalog = app.Get<IExtensionCatalog>();

		EditorModeId[] expected = [.. catalog.FindProjectKind(BuiltInExtension.WorkflowKind)!.Modes.Select(mode => mode.Id)];

		Assert.Equal(expected, toolbar.Modes.Select(option => option.Mode).ToArray());
	}

	[Fact]
	public void Toolbar_RunIsHonestlyDisabledBeforeTheExecutionEngineExists()
	{
		using var app = new TestApp();

		Assert.False(app.Get<EditorToolbarViewModel>().HasActiveWorkflow);
		Assert.False(app.Get<AssistantViewModel>().IsAvailable);
	}

	[Fact]
	public void Toolbar_UndoAndRedoFollowTheDocument()
	{
		using var app = new TestApp();
		var toolbar = app.Get<EditorToolbarViewModel>();
		var editor = toolbar.Editor;

		Assert.False(editor.UndoCommand.CanExecute(null));

		editor.AddNode(app.Get<IExtensionCatalog>().FindNode(new NodeDefinitionId("logic.if"))!, new Point2D(200, 200));
		Assert.True(editor.UndoCommand.CanExecute(null));

		editor.UndoCommand.Execute(null);
		Assert.True(editor.RedoCommand.CanExecute(null));
		Assert.False(editor.UndoCommand.CanExecute(null));
	}

	[Fact]
	public void Console_HasTheSixTabsOfTheSpecification()
	{
		using var app = new TestApp();

		var console = app.Get<ExecutionConsoleViewModel>();

		Assert.Equal(Enum.GetValues<ConsoleTabId>(), console.Tabs.Select(tab => tab.Id).ToArray());
		Assert.All(console.Tabs, tab => Assert.False(string.IsNullOrWhiteSpace(tab.Title)));
	}
}
