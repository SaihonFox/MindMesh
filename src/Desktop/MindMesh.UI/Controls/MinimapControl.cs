using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.UI.Controls;

/// <summary>
/// The overview of the whole graph. Click or drag to move the view there.
/// </summary>
public sealed class MinimapControl : Control
{
	public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
		AvaloniaProperty.Register<MinimapControl, GraphEditorViewModel?>(nameof(Editor));

	private readonly HashSet<NodeViewModel> _watched = [];
	private GraphEditorViewModel? _attached;
	private bool _isDragging;

	static MinimapControl()
	{
		WidthProperty.OverrideDefaultValue<MinimapControl>(MinimapViewModel.Width);
		HeightProperty.OverrideDefaultValue<MinimapControl>(MinimapViewModel.Height);
		ClipToBoundsProperty.OverrideDefaultValue<MinimapControl>(true);
		CursorProperty.OverrideDefaultValue<MinimapControl>(new Cursor(StandardCursorType.Hand));
		EditorProperty.Changed.AddClassHandler<MinimapControl>((map, e) => map.OnEditorChanged(e.OldValue as GraphEditorViewModel, e.NewValue as GraphEditorViewModel));
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

		var background = Find("MM.Brush.Overlay");
		context.DrawRectangle(background, null, new Rect(Bounds.Size));

		if (_attached is not { } editor)
		{
			return;
		}

		var map = editor.Minimap;

		foreach (var node in editor.Nodes)
		{
			var rect = map.WorldToMinimap(node.Bounds);
			var brush = Find($"MM.Brush.Tone.{node.Tone}");

			if (brush is null)
			{
				continue;
			}

			using (context.PushOpacity(node.IsSelected ? 1 : 0.55))
			{
				context.DrawRectangle(brush, null, new Rect(rect.X, rect.Y, Math.Max(rect.Width, 2), Math.Max(rect.Height, 2)), 1.5, 1.5);
			}
		}

		var view = map.ViewportRect;
		var accent = Find("MM.Brush.Accent");
		var fill = Find("MM.Brush.Selection");

		context.DrawRectangle(fill, new Pen(accent, 1.5), new Rect(view.X, view.Y, view.Width, view.Height));
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		base.OnPointerPressed(e);

		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
		{
			_isDragging = true;
			e.Pointer.Capture(this);
			Navigate(e);
			e.Handled = true;
		}
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		base.OnPointerMoved(e);

		if (_isDragging)
		{
			Navigate(e);
			e.Handled = true;
		}
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		base.OnPointerReleased(e);

		if (_isDragging)
		{
			_isDragging = false;
			e.Pointer.Capture(null);
			e.Handled = true;
		}
	}

	private void Navigate(PointerEventArgs e)
	{
		if (_attached is null)
		{
			return;
		}

		var point = e.GetPosition(this);
		_attached.Minimap.NavigateTo(new Point2D(point.X, point.Y));
	}

	private IBrush? Find(string key) =>
		this.TryFindResource(key, ActualThemeVariant, out var resource) ? resource as IBrush : null;

	private void OnEditorChanged(GraphEditorViewModel? oldEditor, GraphEditorViewModel? newEditor)
	{
		if (oldEditor is not null)
		{
			oldEditor.Minimap.Changed -= OnMapChanged;
			oldEditor.Nodes.CollectionChanged -= OnNodesChanged;

			foreach (var node in _watched)
			{
				node.PropertyChanged -= OnNodePropertyChanged;
			}

			_watched.Clear();
		}

		_attached = newEditor;

		if (newEditor is not null)
		{
			newEditor.Minimap.Changed += OnMapChanged;
			newEditor.Nodes.CollectionChanged += OnNodesChanged;

			foreach (var node in newEditor.Nodes)
			{
				Watch(node);
			}
		}

		InvalidateVisual();
	}

	private void OnMapChanged(object? sender, EventArgs e) => InvalidateVisual();

	private void OnNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (var node in _watched)
			{
				node.PropertyChanged -= OnNodePropertyChanged;
			}

			_watched.Clear();

			if (_attached is not null)
			{
				foreach (var node in _attached.Nodes)
				{
					Watch(node);
				}
			}
		}

		if (e.OldItems is not null)
		{
			foreach (NodeViewModel node in e.OldItems)
			{
				if (_watched.Remove(node))
				{
					node.PropertyChanged -= OnNodePropertyChanged;
				}
			}
		}

		if (e.NewItems is not null)
		{
			foreach (NodeViewModel node in e.NewItems)
			{
				Watch(node);
			}
		}

		InvalidateVisual();
	}

	private void Watch(NodeViewModel node)
	{
		if (_watched.Add(node))
		{
			node.PropertyChanged += OnNodePropertyChanged;
		}
	}

	private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(NodeViewModel.IsSelected) or nameof(NodeViewModel.Bounds))
		{
			InvalidateVisual();
		}
	}
}
