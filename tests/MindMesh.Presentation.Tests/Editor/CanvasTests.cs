using MindMesh.Graph.Ports;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Editor;

public class CanvasTests
{
	[Fact]
	public void AddingANode_PlacesItOnTheGrid_SelectsIt_AndCanBeUndone()
	{
		using var fixture = new EditorFixture();
		var editor = fixture.Editor;

		var node = editor.AddNode(fixture.Definition("logic.if"), new Point2D(403, 301));

		Assert.Equal(0, node.X % 8);
		Assert.Equal(0, node.Y % 8);
		Assert.True(node.IsSelected);
		Assert.Same(node, Assert.Single(editor.SelectedNodes));
		Assert.True(editor.HasNodes);

		editor.UndoCommand.Execute(null);

		Assert.Empty(editor.Nodes);
		Assert.False(editor.HasNodes);
	}

	[Fact]
	public void HitTesting_UsesWorldCoordinates_AndPrefersTheCardOnTop()
	{
		using var fixture = new EditorFixture();
		var first = fixture.Add("logic.if", 100, 100);
		var second = fixture.Add("logic.if", 200, 120);

		var overlap = new Point2D(250, 150);

		Assert.Same(second, fixture.Editor.HitTestNode(overlap));

		fixture.Editor.BringToFront(first);

		Assert.Same(first, fixture.Editor.HitTestNode(overlap));
		Assert.Null(fixture.Editor.HitTestNode(new Point2D(-500, -500)));
	}

	[Fact]
	public void PortHitTest_FindsTheDotUnderThePointer_AndStaysUsableWhenZoomedOut()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		var port = node.Inputs[0];

		Assert.Same(port, fixture.Editor.HitTestPort(port.Anchor));
		Assert.Same(port, fixture.Editor.HitTestPort(new Point2D(port.Anchor.X + 8, port.Anchor.Y)));
		Assert.Null(fixture.Editor.HitTestPort(new Point2D(port.Anchor.X + 80, port.Anchor.Y)));

		fixture.Editor.Viewport.ZoomAt(new Point2D(0, 0), 0.25);

		// A 10-pixel target is 10 / zoom world units, so a point 30 units away is still a hit at 25% zoom.
		Assert.Same(port, fixture.Editor.HitTestPort(new Point2D(port.Anchor.X + 30, port.Anchor.Y)));
	}

	[Fact]
	public void Dragging_MovesTheNodeOnTheGrid_AsOneUndoStep()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		var start = new Point2D(150, 130);

		fixture.Editor.BeginNodeDrag(node, start, additive: false);
		fixture.Editor.UpdateNodeDrag(new Point2D(150 + 53, 130 + 29));
		fixture.Editor.UpdateNodeDrag(new Point2D(150 + 101, 130 + 61));
		fixture.Editor.EndNodeDrag();

		Assert.Equal(204, node.X);
		Assert.Equal(164, node.Y);

		fixture.Editor.UndoCommand.Execute(null);

		Assert.Equal(100, node.X);
		Assert.Equal(100, node.Y);
	}

	[Fact]
	public void AClickWithoutMovement_DoesNotMoveOrCreateAnUndoStep()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		fixture.Editor.Document.ClearHistory();

		fixture.Editor.BeginNodeDrag(node, new Point2D(150, 130), additive: false);
		fixture.Editor.UpdateNodeDrag(new Point2D(151, 131));
		fixture.Editor.EndNodeDrag();

		Assert.Equal(100, node.X);
		Assert.False(fixture.Editor.Document.CanUndo);
	}

	[Fact]
	public void TheDragThreshold_IsMeasuredOnScreen_NotInWorldUnits()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		fixture.Editor.Viewport.ZoomAt(new Point2D(0, 0), 0.5);

		fixture.Editor.BeginNodeDrag(node, new Point2D(150, 130), additive: false);
		fixture.Editor.UpdateNodeDrag(new Point2D(152, 130));

		// 2 world units are 1 screen pixel at 50% zoom: below the threshold.
		Assert.False(fixture.Editor.IsDraggingNodes);

		fixture.Editor.UpdateNodeDrag(new Point2D(160, 130));

		Assert.True(fixture.Editor.IsDraggingNodes);
		fixture.Editor.CancelNodeDrag();
		Assert.Equal(100, node.X);
	}

	[Fact]
	public void CancellingADrag_PutsTheNodesBack()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);

		fixture.Editor.BeginNodeDrag(node, new Point2D(150, 130), additive: false);
		fixture.Editor.UpdateNodeDrag(new Point2D(400, 400));
		fixture.Editor.CancelNodeDrag();

		Assert.Equal(100, node.X);
		Assert.Equal(100, node.Y);
	}

	[Fact]
	public void DraggingOneSelectedNode_MovesAllSelectedNodes()
	{
		using var fixture = new EditorFixture();
		var first = fixture.Add("logic.if", 100, 100);
		var second = fixture.Add("logic.delay", 500, 100);
		fixture.Editor.SelectAll();

		fixture.Editor.BeginNodeDrag(first, new Point2D(150, 130), additive: false);
		fixture.Editor.UpdateNodeDrag(new Point2D(150 + 80, 130 + 40));
		fixture.Editor.EndNodeDrag();

		Assert.Equal(180, first.X);
		Assert.Equal(580, second.X);
		Assert.Equal(140, second.Y);

		fixture.Editor.UndoCommand.Execute(null);

		Assert.Equal(100, first.X);
		Assert.Equal(500, second.X);
	}

	[Fact]
	public void SelectionRules_ClickReplaces_AdditiveToggles_RubberBandSelects()
	{
		using var fixture = new EditorFixture();
		var first = fixture.Add("logic.if", 100, 100);
		var second = fixture.Add("logic.delay", 500, 100);
		var editor = fixture.Editor;

		editor.SelectNode(first, additive: false);
		editor.SelectNode(second, additive: true);
		Assert.Equal(2, editor.SelectedNodes.Count);

		editor.SelectNode(second, additive: true);
		Assert.Same(first, Assert.Single(editor.SelectedNodes));

		editor.ClearSelection();
		Assert.Empty(editor.SelectedNodes);

		editor.BeginRubberBand(new Point2D(50, 50), additive: false);
		editor.UpdateRubberBand(new Point2D(420, 300));
		editor.EndRubberBand();

		Assert.Same(first, Assert.Single(editor.SelectedNodes));
		Assert.False(editor.RubberBand.IsActive);
	}

	[Fact]
	public void DraggingFromAnOutputToACompatibleInput_CreatesAConnection()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);
		var from = EditorFixture.Output(trigger, PortType.Execution);
		var to = EditorFixture.Input(delay, PortType.Execution);

		fixture.Drag(from, to);

		var connection = Assert.Single(fixture.Editor.Connections);
		Assert.Same(from, connection.Source);
		Assert.Same(to, connection.Target);
		Assert.True(from.IsConnected);
		Assert.True(to.IsConnected);
		Assert.False(fixture.Editor.Pending.IsActive);
		Assert.All(fixture.Editor.Nodes.SelectMany(node => node.Ports), port => Assert.Equal(PortHint.None, port.Hint));

		fixture.Editor.UndoCommand.Execute(null);

		Assert.Empty(fixture.Editor.Connections);
		Assert.False(from.IsConnected);
	}

	[Fact]
	public void DraggingBackwards_FromAnInputToAnOutput_ConnectsThemToo()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);

		fixture.Drag(EditorFixture.Input(delay, PortType.Execution), EditorFixture.Output(trigger, PortType.Execution));

		var connection = Assert.Single(fixture.Editor.Connections);
		Assert.Same(trigger, connection.Source.Owner);
	}

	[Fact]
	public void WhileConnecting_PortsAreHintedAsCompatibleOrNot()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);
		var from = EditorFixture.Output(trigger, PortType.Execution);

		fixture.Editor.BeginConnection(from, from.Anchor);

		Assert.Equal(PortHint.Source, from.Hint);
		Assert.Equal(PortHint.Compatible, EditorFixture.Input(delay, PortType.Execution).Hint);
		Assert.True(fixture.Editor.Pending.IsActive);

		var dataInput = delay.Inputs.FirstOrDefault(port => port.Kind == PortKind.Data);

		if (dataInput is not null)
		{
			Assert.Equal(PortHint.Incompatible, dataInput.Hint);
		}

		fixture.Editor.CancelConnection();

		Assert.False(fixture.Editor.Pending.IsActive);
		Assert.All(fixture.Editor.Nodes.SelectMany(node => node.Ports), port => Assert.Equal(PortHint.None, port.Hint));
	}

	[Fact]
	public void DroppingAConnectionOnEmptySpace_OpensTheQuickAddBoxForThatPort()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var from = EditorFixture.Output(trigger, PortType.Execution);

		fixture.Editor.BeginConnection(from, from.Anchor);
		fixture.Editor.UpdateConnection(new Point2D(900, 700));
		fixture.Editor.CompleteConnection(new Point2D(900, 700));

		Assert.True(fixture.Editor.QuickAdd.IsOpen);
		Assert.Same(from, fixture.Editor.QuickAdd.FromPort);
		Assert.Empty(fixture.Editor.Connections);
	}

	[Fact]
	public void QuickAddWithAPort_OffersOnlyNodesThatCanConnect_AndConnectsInOneStep()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var from = EditorFixture.Output(trigger, PortType.Execution);
		var quickAdd = fixture.Editor.QuickAdd;

		quickAdd.Open(new Point2D(900, 300), from);
		quickAdd.SearchText = "delay";

		Assert.All(quickAdd.Results, item =>
			Assert.NotNull(MindMesh.Presentation.Pages.Editor.Canvas.NodeGraphPorts.FindCompatiblePort(item.Definition, from.Definition)));
		var delay = Assert.Single(quickAdd.Results, item => item.Definition.Id.Value == "logic.delay");

		fixture.Editor.Document.ClearHistory();
		quickAdd.Confirm(delay);

		Assert.False(quickAdd.IsOpen);
		Assert.Equal(2, fixture.Editor.Nodes.Count);
		Assert.Single(fixture.Editor.Connections);

		fixture.Editor.UndoCommand.Execute(null);

		Assert.Single(fixture.Editor.Nodes);
		Assert.Empty(fixture.Editor.Connections);
		Assert.False(fixture.Editor.Document.CanUndo);
	}

	[Fact]
	public void QuickAddWithoutAPort_AddsAtItsPosition_AndSearchFiltersTheResults()
	{
		using var fixture = new EditorFixture();
		var quickAdd = fixture.Editor.QuickAdd;

		quickAdd.Open(new Point2D(400, 300));
		var all = quickAdd.Results.Count;
		quickAdd.SearchText = "webhook";

		Assert.True(quickAdd.Results.Count < all);
		Assert.NotEmpty(quickAdd.Results);

		quickAdd.Confirm();

		Assert.Single(fixture.Editor.Nodes);
		Assert.False(quickAdd.IsOpen);
	}

	[Fact]
	public void ReconnectingAnOccupiedInput_AndDroppingOnEmptySpace_DeletesTheConnection()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);
		var from = EditorFixture.Output(trigger, PortType.Execution);
		var to = EditorFixture.Input(delay, PortType.Execution);
		fixture.Drag(from, to);

		fixture.Editor.BeginConnection(to, to.Anchor);

		Assert.True(fixture.Editor.Pending.IsActive);

		fixture.Editor.UpdateConnection(new Point2D(900, 700));
		fixture.Editor.CompleteConnection(new Point2D(900, 700));

		Assert.Empty(fixture.Editor.Connections);

		fixture.Editor.UndoCommand.Execute(null);

		Assert.Single(fixture.Editor.Connections);
	}

	[Fact]
	public void ReconnectingAndDroppingBackOnTheSameInput_ChangesNothing()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);
		var to = EditorFixture.Input(delay, PortType.Execution);
		fixture.Drag(EditorFixture.Output(trigger, PortType.Execution), to);
		fixture.Editor.Document.ClearHistory();

		fixture.Editor.BeginConnection(to, to.Anchor);
		fixture.Editor.UpdateConnection(to.Anchor);
		fixture.Editor.CompleteConnection(to.Anchor);

		Assert.Single(fixture.Editor.Connections);
		Assert.False(fixture.Editor.Document.CanUndo);
	}

	[Fact]
	public void AConnection_CanBeClickedSelectedAndDeleted()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 600, 300);
		var from = EditorFixture.Output(trigger, PortType.Execution);
		var to = EditorFixture.Input(delay, PortType.Execution);
		fixture.Drag(from, to);

		var middle = new Point2D((from.Anchor.X + to.Anchor.X) / 2, (from.Anchor.Y + to.Anchor.Y) / 2);
		var hit = fixture.Editor.HitTestConnection(middle);

		Assert.NotNull(hit);
		Assert.Null(fixture.Editor.HitTestConnection(new Point2D(middle.X, middle.Y + 400)));

		fixture.Editor.SelectConnection(hit);
		Assert.Same(hit, fixture.Editor.SelectedConnection);
		Assert.Empty(fixture.Editor.SelectedNodes);

		fixture.Editor.DeleteSelection();

		Assert.Empty(fixture.Editor.Connections);
		Assert.Equal(2, fixture.Editor.Nodes.Count);
	}

	[Fact]
	public void DeletingNodes_RemovesTheirConnections_AndUndoBringsEverythingBack()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);
		fixture.Drag(EditorFixture.Output(trigger, PortType.Execution), EditorFixture.Input(delay, PortType.Execution));

		fixture.Editor.SelectNode(trigger, additive: false);
		fixture.Editor.DeleteSelection();

		Assert.Single(fixture.Editor.Nodes);
		Assert.Empty(fixture.Editor.Connections);
		Assert.False(EditorFixture.Input(delay, PortType.Execution).IsConnected);

		fixture.Editor.UndoCommand.Execute(null);

		Assert.Equal(2, fixture.Editor.Nodes.Count);
		Assert.Single(fixture.Editor.Connections);
		Assert.True(EditorFixture.Input(fixture.Editor.Nodes.Single(node => node.Id == delay.Id), PortType.Execution).IsConnected);
	}

	[Fact]
	public void CopyAndPaste_CopiesNodesAndTheConnectionBetweenThem()
	{
		using var fixture = new EditorFixture();
		var trigger = fixture.Add("trigger.manual", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);
		fixture.Drag(EditorFixture.Output(trigger, PortType.Execution), EditorFixture.Input(delay, PortType.Execution));
		fixture.Editor.SelectAll();

		Assert.False(fixture.Editor.CanPaste);
		fixture.Editor.Copy();
		Assert.True(fixture.Editor.CanPaste);

		fixture.Editor.Paste();

		Assert.Equal(4, fixture.Editor.Nodes.Count);
		Assert.Equal(2, fixture.Editor.Connections.Count);
		Assert.Equal(2, fixture.Editor.SelectedNodes.Count);

		fixture.Editor.Paste();
		Assert.Equal(6, fixture.Editor.Nodes.Count);

		fixture.Editor.UndoCommand.Execute(null);
		Assert.Equal(4, fixture.Editor.Nodes.Count);
	}

	[Fact]
	public void Paste_ShiftsEachCopy_SoCopiesDoNotHideEachOther()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		fixture.Editor.SelectNode(node, additive: false);
		fixture.Editor.Copy();

		fixture.Editor.Paste();
		var first = Assert.Single(fixture.Editor.SelectedNodes);
		fixture.Editor.Paste();
		var second = Assert.Single(fixture.Editor.SelectedNodes);

		Assert.NotEqual((node.X, node.Y), (first.X, first.Y));
		Assert.NotEqual((first.X, first.Y), (second.X, second.Y));
	}

	[Fact]
	public void Duplicate_CopiesTheSelection_WithoutTouchingTheClipboard()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		fixture.Editor.SelectNode(node, additive: false);

		fixture.Editor.Duplicate();

		Assert.Equal(2, fixture.Editor.Nodes.Count);
		Assert.False(fixture.Editor.CanPaste);
		Assert.NotSame(node, Assert.Single(fixture.Editor.SelectedNodes));
	}

	[Fact]
	public void DisablingANode_ShowsItAsDisabled_AndTogglesBack()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);

		fixture.Editor.ToggleDisabled();
		Assert.True(node.IsDisabled);
		Assert.Equal(NodeVisualState.Disabled, node.VisualState);

		fixture.Editor.ToggleDisabled();
		Assert.False(node.IsDisabled);
		Assert.NotEqual(NodeVisualState.Disabled, node.VisualState);
	}

	[Fact]
	public void ACollapsedNode_IsShorter_AndItsPortsCannotBeHit()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		var port = node.Inputs[0];
		var anchor = port.Anchor;
		var expandedHeight = node.Height;

		fixture.Editor.ToggleCollapsed();

		Assert.True(node.IsCollapsed);
		Assert.True(node.Height < expandedHeight);
		Assert.Null(fixture.Editor.HitTestPort(anchor with { Y = anchor.Y + 100 }));

		fixture.Editor.ToggleCollapsed();

		Assert.False(node.IsCollapsed);
		Assert.Equal(expandedHeight, node.Height);
	}

	[Fact]
	public void ANodeWithMissingRequiredSettings_ShowsAWarning()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("http.request", 100, 100);
		fixture.Editor.ClearSelection();

		Assert.True(node.HasIssues);
		Assert.Equal(NodeVisualState.Warning, node.VisualState);
	}

	[Fact]
	public void NodesFarOutsideTheView_AreCulled_UntilTheViewReachesThem()
	{
		using var fixture = new EditorFixture();
		var near = fixture.Add("logic.if", 100, 100);
		var far = fixture.Add("logic.delay", 20000, 20000);
		fixture.Editor.ClearSelection();

		Assert.True(near.IsInViewport);
		Assert.False(far.IsInViewport);
		Assert.Null(fixture.Editor.HitTestNode(new Point2D(20100, 20050)));

		fixture.Editor.Viewport.CenterOn(far.Bounds.Center);

		Assert.True(far.IsInViewport);
		Assert.False(near.IsInViewport);
	}

	[Fact]
	public void Zooming_KeepsThePointUnderThePointerStill()
	{
		using var fixture = new EditorFixture();
		var viewport = fixture.Editor.Viewport;
		var anchor = new Point2D(300, 200);
		var before = viewport.ScreenToWorld(anchor);

		viewport.ZoomAt(anchor, 1.5);

		var after = viewport.ScreenToWorld(anchor);
		Assert.Equal(before.X, after.X, 6);
		Assert.Equal(before.Y, after.Y, 6);
		Assert.True(viewport.Zoom > 1);
	}

	[Fact]
	public void Zoom_IsClamped_AndResetReturnsToOneHundredPercent()
	{
		using var fixture = new EditorFixture();
		var viewport = fixture.Editor.Viewport;

		viewport.ZoomAt(new Point2D(0, 0), 1000);
		Assert.Equal(ViewportState.MaximumZoom, viewport.Zoom);

		viewport.ZoomAt(new Point2D(0, 0), 0.00001);
		Assert.Equal(ViewportState.MinimumZoom, viewport.Zoom);

		viewport.ResetZoom();
		Assert.Equal(1, viewport.Zoom);
		Assert.Equal("100%", viewport.ZoomText);
	}

	[Fact]
	public void PanningAndConversions_RoundTrip()
	{
		using var fixture = new EditorFixture();
		var viewport = fixture.Editor.Viewport;

		viewport.PanBy(120, -40);
		viewport.ZoomAt(new Point2D(10, 10), 0.8);

		var world = new Point2D(321, -77);
		var back = viewport.ScreenToWorld(viewport.WorldToScreen(world));

		Assert.Equal(world.X, back.X, 6);
		Assert.Equal(world.Y, back.Y, 6);
	}

	[Fact]
	public void FitToView_ShowsEveryNode()
	{
		using var fixture = new EditorFixture();
		var first = fixture.Add("logic.if", 0, 0);
		var second = fixture.Add("logic.delay", 3000, 2000);
		fixture.Editor.ClearSelection();

		fixture.Editor.FitToViewCommand.Execute(null);

		var visible = fixture.Editor.Viewport.VisibleWorld;
		Assert.True(visible.Intersects(first.Bounds));
		Assert.True(visible.Contains(second.Bounds.Center));
		Assert.True(fixture.Editor.Viewport.Zoom < 1);
	}

	[Fact]
	public void Find_MarksMatchingNodes_AndStepsThroughThem()
	{
		using var fixture = new EditorFixture();
		var request = fixture.Add("http.request", 100, 100);
		var delay = fixture.Add("logic.delay", 500, 100);
		fixture.Editor.ClearSelection();
		var find = fixture.Editor.Find;

		find.Open();
		find.SearchText = "delay";

		Assert.True(find.HasMatches);
		Assert.Equal(1, find.MatchCount);
		Assert.True(delay.IsFindMatch);
		Assert.False(request.IsFindMatch);

		find.Close();

		Assert.False(delay.IsFindMatch);
	}

	[Fact]
	public void TheMenus_OfferTheActionsThatApply()
	{
		using var fixture = new EditorFixture();
		var node = fixture.Add("logic.if", 100, 100);
		var editor = fixture.Editor;

		var canvasMenu = editor.BuildCanvasMenu(new Point2D(0, 0));
		var nodeMenu = editor.BuildNodeMenu(node);

		Assert.Contains(canvasMenu, entry => !entry.IsSeparator && entry.IsEnabled);
		Assert.Contains(canvasMenu, entry => entry is { IsSeparator: false, IsEnabled: false }); // Paste with an empty clipboard.
		Assert.Contains(nodeMenu, entry => entry.Shortcut == "Ctrl+D");
		Assert.All(nodeMenu.Where(entry => !entry.IsSeparator), entry => Assert.False(string.IsNullOrWhiteSpace(entry.Title)));
	}

	[Fact]
	public void TheGridCanBeSwitchedOff_ThenNodesLandWhereTheyAreDropped()
	{
		using var fixture = new EditorFixture();
		fixture.Editor.SnapToGrid = false;

		var node = fixture.Editor.AddNode(fixture.Definition("logic.if"), new Point2D(403, 301));

		Assert.Equal(403 - (NodeMetrics.Width / 2), node.X, 6);
	}
}
