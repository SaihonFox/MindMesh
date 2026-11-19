using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MindMesh.Graph.Ports;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.UI.Controls;

/// <summary>
/// Draws all connections of the graph in one pass, below the node cards. Connections are drawn in screen space
/// (computed from world coordinates and the viewport), so lines keep a constant width at every zoom level.
/// </summary>
public sealed class ConnectionsLayer : Control
{
	/// <summary>Labels are hidden below this zoom; they would only be a smudge.</summary>
	private const double MinimumLabelZoom = 0.55;

	public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
		AvaloniaProperty.Register<ConnectionsLayer, GraphEditorViewModel?>(nameof(Editor));

	private readonly ConnectionPalette _palette = new();
	private readonly Dictionary<string, FormattedText> _labels = [];
	private readonly HashSet<ConnectionViewModel> _watched = [];
	private GraphEditorViewModel? _attached;

	static ConnectionsLayer()
	{
		AffectsRender<ConnectionsLayer>(EditorProperty);
		IsHitTestVisibleProperty.OverrideDefaultValue<ConnectionsLayer>(false);
		EditorProperty.Changed.AddClassHandler<ConnectionsLayer>((layer, e) => layer.OnEditorChanged(e.OldValue as GraphEditorViewModel, e.NewValue as GraphEditorViewModel));
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
			_labels.Clear();
			InvalidateVisual();
		}
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		if (_attached is not { } editor)
			return;

		_palette.Refresh(this);

		var viewport = editor.Viewport;
		var zoom = viewport.Zoom;
		var selected = new List<ConnectionViewModel>();

		foreach (var connection in editor.Connections)
		{
			if (!connection.IsInViewport)
				continue;

			if (connection.IsSelected)
			{
				selected.Add(connection);
				continue;
			}

			DrawConnection(context, viewport, connection, zoom, isSelected: false);
		}

		foreach (var connection in selected)
			DrawConnection(context, viewport, connection, zoom, isSelected: true);

		if (zoom >= MinimumLabelZoom)
		{
			foreach (var connection in editor.Connections)
			{
				if (connection.IsInViewport && connection.HasLabel)
					DrawLabel(context, viewport, connection);
			}
		}
	}

	private void DrawConnection(DrawingContext context, ViewportViewModel viewport, ConnectionViewModel connection, double zoom, bool isSelected)
	{
		var brush = isSelected ? _palette.SelectionBorder : _palette.ForKind(connection.Kind);

		if (brush is null)
			return;

		var curve = connection.Curve;
		var start = ToScreen(viewport, curve.Start);
		var control1 = ToScreen(viewport, curve.Control1);
		var control2 = ToScreen(viewport, curve.Control2);
		var end = ToScreen(viewport, curve.End);

		var geometry = new StreamGeometry();

		using (var figure = geometry.Open())
		{
			figure.BeginFigure(start, false);
			figure.CubicBezierTo(control1, control2, end);
			figure.EndFigure(false);
		}

		var thickness = (isSelected ? 3.5 : 2.25) * Math.Clamp(zoom, 0.6, 1.4);
		var dash = connection.Kind == PortKind.Event ? new DashStyle([4, 3], 0) : null;
		var pen = new Pen(brush, thickness, dash, PenLineCap.Round, PenLineJoin.Round);

		using (context.PushOpacity(connection.IsDimmed ? 0.3 : 1))
		{
			context.DrawGeometry(null, pen, geometry);
		}
	}

	private void DrawLabel(DrawingContext context, ViewportViewModel viewport, ConnectionViewModel connection)
	{
		if (_palette.PillText is null)
			return;

		var text = GetLabel(connection.Label);
		var middle = ToScreen(viewport, connection.Curve.Midpoint);
		var padding = new Size(8, 3);
		var rect = new Rect(
			middle.X - (text.Width / 2) - padding.Width,
			middle.Y - (text.Height / 2) - padding.Height,
			text.Width + (2 * padding.Width),
			text.Height + (2 * padding.Height));

		using (context.PushOpacity(connection.IsDimmed ? 0.3 : 1))
		{
			context.DrawRectangle(_palette.PillBackground, new Pen(_palette.PillBorder, 1), rect, rect.Height / 2, rect.Height / 2);
			context.DrawText(text, new Point(rect.X + padding.Width, rect.Y + padding.Height));
		}
	}

	private FormattedText GetLabel(string label)
	{
		if (_labels.TryGetValue(label, out var cached))
			return cached;

		var text = new FormattedText(
			label,
			System.Globalization.CultureInfo.CurrentUICulture,
			FlowDirection.LeftToRight,
			Typeface.Default,
			11,
			_palette.PillText);

		_labels[label] = text;
		return text;
	}

	private static Point ToScreen(ViewportViewModel viewport, Point2D world)
	{
		var screen = viewport.WorldToScreen(world);
		return new Point(screen.X, screen.Y);
	}

	private void OnEditorChanged(GraphEditorViewModel? oldEditor, GraphEditorViewModel? newEditor)
	{
		if (oldEditor is not null)
		{
			oldEditor.Viewport.Changed -= OnRedrawRequested;
			oldEditor.Connections.CollectionChanged -= OnConnectionsChanged;

			foreach (var connection in _watched)
				connection.PropertyChanged -= OnConnectionPropertyChanged;

			_watched.Clear();
		}

		_attached = newEditor;

		if (newEditor is not null)
		{
			newEditor.Viewport.Changed += OnRedrawRequested;
			newEditor.Connections.CollectionChanged += OnConnectionsChanged;

			foreach (var connection in newEditor.Connections)
				Watch(connection);
		}

		InvalidateVisual();
	}

	private void OnRedrawRequested(object? sender, EventArgs e) => InvalidateVisual();

	private void OnConnectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (var connection in _watched)
				connection.PropertyChanged -= OnConnectionPropertyChanged;

			_watched.Clear();

			if (_attached is not null)
			{
				foreach (var connection in _attached.Connections)
					Watch(connection);
			}
		}

		if (e.OldItems is not null)
		{
			foreach (ConnectionViewModel connection in e.OldItems)
			{
				if (_watched.Remove(connection))
					connection.PropertyChanged -= OnConnectionPropertyChanged;
			}
		}

		if (e.NewItems is not null)
		{
			foreach (ConnectionViewModel connection in e.NewItems)
				Watch(connection);
		}

		InvalidateVisual();
	}

	private void Watch(ConnectionViewModel connection)
	{
		if (_watched.Add(connection))
			connection.PropertyChanged += OnConnectionPropertyChanged;
	}

	private void OnConnectionPropertyChanged(object? sender, PropertyChangedEventArgs e) => InvalidateVisual();
}
