using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.UI.Controls;

/// <summary>
/// Draws what exists only while the user is dragging: the loose end of a new connection and the selection rectangle.
/// </summary>
public sealed class CanvasOverlay : Control
{
	public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
		AvaloniaProperty.Register<CanvasOverlay, GraphEditorViewModel?>(nameof(Editor));

	private readonly ConnectionPalette _palette = new();
	private GraphEditorViewModel? _attached;

	static CanvasOverlay()
	{
		IsHitTestVisibleProperty.OverrideDefaultValue<CanvasOverlay>(false);
		EditorProperty.Changed.AddClassHandler<CanvasOverlay>((overlay, e) => overlay.OnEditorChanged(e.OldValue as GraphEditorViewModel, e.NewValue as GraphEditorViewModel));
	}

	public GraphEditorViewModel? Editor
	{
		get => GetValue(EditorProperty);
		set => SetValue(EditorProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property.Name == "ActualThemeVariant")
		{
			InvalidateVisual();
		}
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		if (_attached is not { } editor)
		{
			return;
		}

		_palette.Refresh(this);

		if (editor.Pending.IsActive)
		{
			DrawPending(context, editor);
		}

		if (editor.RubberBand.IsActive)
		{
			DrawRubberBand(context, editor);
		}
	}

	private void DrawPending(DrawingContext context, GraphEditorViewModel editor)
	{
		var pending = editor.Pending;
		var brush = pending.IsValid ? _palette.SelectionBorder : _palette.ForKind(pending.Kind);

		if (brush is null)
		{
			return;
		}

		var curve = pending.Curve;
		var geometry = new StreamGeometry();

		using (var figure = geometry.Open())
		{
			figure.BeginFigure(ToScreen(editor, curve.Start), false);
			figure.CubicBezierTo(ToScreen(editor, curve.Control1), ToScreen(editor, curve.Control2), ToScreen(editor, curve.End));
			figure.EndFigure(false);
		}

		var pen = new Pen(brush, 2.5, new DashStyle([5, 4], 0), PenLineCap.Round, PenLineJoin.Round);
		context.DrawGeometry(null, pen, geometry);
	}

	private void DrawRubberBand(DrawingContext context, GraphEditorViewModel editor)
	{
		var rect = editor.RubberBand.Rect;
		var topLeft = editor.Viewport.WorldToScreen(new Point2D(rect.X, rect.Y));
		var bottomRight = editor.Viewport.WorldToScreen(new Point2D(rect.Right, rect.Bottom));
		var screen = new Rect(new Point(topLeft.X, topLeft.Y), new Point(bottomRight.X, bottomRight.Y));

		context.DrawRectangle(_palette.Selection, new Pen(_palette.SelectionBorder, 1), screen);
	}

	private static Point ToScreen(GraphEditorViewModel editor, Point2D world)
	{
		var screen = editor.Viewport.WorldToScreen(world);
		return new Point(screen.X, screen.Y);
	}

	private void OnEditorChanged(GraphEditorViewModel? oldEditor, GraphEditorViewModel? newEditor)
	{
		if (oldEditor is not null)
		{
			oldEditor.Pending.PropertyChanged -= OnChanged;
			oldEditor.RubberBand.PropertyChanged -= OnChanged;
			oldEditor.Viewport.Changed -= OnViewportChanged;
		}

		_attached = newEditor;

		if (newEditor is not null)
		{
			newEditor.Pending.PropertyChanged += OnChanged;
			newEditor.RubberBand.PropertyChanged += OnChanged;
			newEditor.Viewport.Changed += OnViewportChanged;
		}

		InvalidateVisual();
	}

	private void OnChanged(object? sender, PropertyChangedEventArgs e) => InvalidateVisual();

	private void OnViewportChanged(object? sender, EventArgs e) => InvalidateVisual();
}
