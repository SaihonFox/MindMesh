using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Pages.Editor.Library;
using MindMesh.UI.Controls;

namespace MindMesh.UI.Views;

/// <summary>
/// The graph canvas. All hit-testing is done by the view model in world coordinates; this class only translates
/// pointer and keyboard input into calls on <see cref="GraphEditorViewModel"/> and applies the viewport transform.
/// </summary>
public partial class GraphCanvasView : UserControl
{
	private enum Gesture
	{
		None,
		Pan,
		Node,
		Connect,
		RubberBand,
	}

	private GraphEditorViewModel? _editor;
	private Gesture _gesture;
	private Point _lastScreen;
	private Point2D _lastWorld;
	private bool _isSpaceDown;
	private bool _spaceUsedForPan;

	public GraphCanvasView()
	{
		InitializeComponent();

		HitLayer.PointerPressed += OnPointerPressed;
		HitLayer.PointerMoved += OnPointerMoved;
		HitLayer.PointerReleased += OnPointerReleased;
		HitLayer.PointerCaptureLost += OnPointerCaptureLost;
		HitLayer.PointerWheelChanged += OnPointerWheelChanged;
		HitLayer.SizeChanged += (_, e) => _editor?.Viewport.SetViewSize(e.NewSize.Width, e.NewSize.Height);

		QuickAddBox.KeyDown += OnQuickAddKeyDown;
		QuickAddList.DoubleTapped += OnQuickAddItemActivated;
		QuickAddList.Tapped += OnQuickAddItemActivated;
		FindBox.KeyDown += OnFindKeyDown;
	}

	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (_editor is not null)
		{
			_editor.Viewport.Changed -= OnViewportChanged;
			_editor.QuickAdd.PropertyChanged -= OnQuickAddChanged;
			_editor.Find.PropertyChanged -= OnFindChanged;
		}

		_editor = DataContext as GraphEditorViewModel;

		if (_editor is null)
			return;

		_editor.Viewport.Changed += OnViewportChanged;
		_editor.QuickAdd.PropertyChanged += OnQuickAddChanged;
		_editor.Find.PropertyChanged += OnFindChanged;

		if (HitLayer.Bounds.Width > 0)
			_editor.Viewport.SetViewSize(HitLayer.Bounds.Width, HitLayer.Bounds.Height);

		ApplyViewport();
	}

	// ---- Viewport ----------------------------------------------------------------------------------------------

	private void OnViewportChanged(object? sender, EventArgs e) => ApplyViewport();

	private void ApplyViewport()
	{
		if (_editor is not { } editor)
			return;

		var viewport = editor.Viewport;

		WorldLayer.RenderTransform = new MatrixTransform(new Matrix(viewport.Zoom, 0, 0, viewport.Zoom, viewport.OffsetX, viewport.OffsetY));
		GridLayer.Zoom = viewport.Zoom;
		GridLayer.Offset = new Point(viewport.OffsetX, viewport.OffsetY);
	}

	private Point2D ToWorld(Point screen) => _editor!.Viewport.ScreenToWorld(new Point2D(screen.X, screen.Y));

	/// <summary>
	/// Converts a point given relative to <paramref name="relativeTo"/> (for example the window) into world coordinates.
	/// Returns false when the point is not over the canvas.
	/// </summary>
	public bool TryGetWorldPoint(Visual relativeTo, Point point, out Point2D world)
	{
		world = default;

		if (_editor is null || relativeTo.TranslatePoint(point, HitLayer) is not { } local
			|| !new Rect(HitLayer.Bounds.Size).Contains(local))
		{
			return false;
		}

		world = ToWorld(local);
		return true;
	}

	// ---- Pointer -----------------------------------------------------------------------------------------------

	private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (_editor is not { } editor)
			return;

		Focus();

		var point = e.GetCurrentPoint(HitLayer);
		var screen = point.Position;
		var world = ToWorld(screen);
		var modifiers = e.KeyModifiers;
		var additive = modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Shift);

		_lastScreen = screen;
		_lastWorld = world;

		if (editor.QuickAdd.IsOpen)
			editor.QuickAdd.Close();

		var properties = point.Properties;

		if (properties.IsMiddleButtonPressed || (properties.IsLeftButtonPressed && _isSpaceDown))
		{
			_spaceUsedForPan = _isSpaceDown;
			BeginGesture(Gesture.Pan, e);
			HitLayer.Cursor = new Cursor(StandardCursorType.SizeAll);
			return;
		}

		if (properties.IsRightButtonPressed)
		{
			ShowContextMenu(editor, world);
			e.Handled = true;
			return;
		}

		if (!properties.IsLeftButtonPressed)
			return;

		if (editor.HitTestPort(world) is { } port)
		{
			editor.BeginConnection(port, world);
			BeginGesture(Gesture.Connect, e);
			HitLayer.Cursor = new Cursor(StandardCursorType.Cross);
			return;
		}

		if (editor.HitTestNode(world) is { } node)
		{
			editor.BeginNodeDrag(node, world, additive);
			BeginGesture(Gesture.Node, e);
			return;
		}

		if (editor.HitTestConnection(world) is { } connection)
		{
			editor.SelectConnection(connection);
			return;
		}

		editor.BeginRubberBand(world, additive);
		BeginGesture(Gesture.RubberBand, e);
	}

	private void BeginGesture(Gesture gesture, PointerPressedEventArgs e)
	{
		_gesture = gesture;
		e.Pointer.Capture(HitLayer);
		e.Handled = true;
	}

	private void OnPointerMoved(object? sender, PointerEventArgs e)
	{
		if (_editor is not { } editor)
			return;

		var screen = e.GetPosition(HitLayer);
		var world = ToWorld(screen);

		_lastWorld = world;

		switch (_gesture)
		{
			case Gesture.Pan:
				editor.Viewport.PanBy(screen.X - _lastScreen.X, screen.Y - _lastScreen.Y);
				_spaceUsedForPan = _isSpaceDown;
				break;

			case Gesture.Node:
				editor.UpdateNodeDrag(world);
				break;

			case Gesture.Connect:
				editor.UpdateConnection(world);
				break;

			case Gesture.RubberBand:
				editor.UpdateRubberBand(world);
				break;
		}

		_lastScreen = screen;
	}

	private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (_editor is not { } editor || _gesture == Gesture.None)
			return;

		var world = ToWorld(e.GetPosition(HitLayer));
		var gesture = _gesture;

		_gesture = Gesture.None;
		HitLayer.Cursor = Cursor.Default;
		e.Pointer.Capture(null);

		switch (gesture)
		{
			case Gesture.Node:
				editor.EndNodeDrag();
				break;

			case Gesture.Connect:
				editor.CompleteConnection(world);
				break;

			case Gesture.RubberBand:
				editor.EndRubberBand();
				break;
		}

		e.Handled = true;
	}

	private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
	{
		// The capture is also released by OnPointerReleased, which clears the gesture first.
		if (_editor is not { } editor || _gesture == Gesture.None)
			return;

		var gesture = _gesture;

		_gesture = Gesture.None;
		HitLayer.Cursor = Cursor.Default;

		switch (gesture)
		{
			case Gesture.Node:
				editor.CancelNodeDrag();
				break;

			case Gesture.Connect:
				editor.CancelConnection();
				break;

			case Gesture.RubberBand:
				editor.EndRubberBand();
				break;
		}
	}

	private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (_editor is not { } editor)
			return;

		var viewport = editor.Viewport;

		if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
		{
			viewport.PanBy(e.Delta.Y * 60, 0);
		}
		else if (e.Delta.X != 0 && e.Delta.Y == 0)
		{
			viewport.PanBy(e.Delta.X * 60, 0);
		}
		else
		{
			var position = e.GetPosition(HitLayer);
			viewport.ZoomAt(new Point2D(position.X, position.Y), Math.Pow(1.1, e.Delta.Y));
		}

		e.Handled = true;
	}

	// ---- Context menu ------------------------------------------------------------------------------------------

	private void ShowContextMenu(GraphEditorViewModel editor, Point2D world)
	{
		IReadOnlyList<MenuEntry> entries;

		if (editor.HitTestNode(world) is { } node)
		{
			entries = editor.BuildNodeMenu(node);
		}
		else if (editor.HitTestConnection(world) is { } connection)
		{
			editor.SelectConnection(connection);
			entries = editor.BuildConnectionMenu(connection);
		}
		else
		{
			entries = editor.BuildCanvasMenu(world);
		}

		var menu = new ContextMenu { ItemsSource = entries.Select(CreateMenuItem).ToList() };

		menu.Open(HitLayer);
	}

	private static Control CreateMenuItem(MenuEntry entry)
	{
		if (entry.IsSeparator)
			return new Separator();

		var item = new MenuItem
		{
			Header = entry.Title,
			IsEnabled = entry.IsEnabled,
		};

		if (entry.IconKey is { } iconKey)
			item.Icon = new Icon { Key = iconKey, Width = 14, Height = 14 };

		if (entry.Shortcut is { } shortcut && TryParseGesture(shortcut) is { } gesture)
			item.InputGesture = gesture;

		var execute = entry.Execute;

		item.Click += (_, _) => execute?.Invoke();

		return item;
	}

	private static KeyGesture? TryParseGesture(string text)
	{
		try
		{
			return KeyGesture.Parse(text);
		}
		catch (Exception ex) when (ex is ArgumentException or FormatException)
		{
			return null;
		}
	}

	// ---- Keyboard ----------------------------------------------------------------------------------------------

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (_editor is not { } editor || e.Handled || e.Source is TextBox)
			return;

		var control = e.KeyModifiers.HasFlag(KeyModifiers.Control);
		var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
		bool handled = true;

		switch (e.Key)
		{
			case Key.Delete or Key.Back:
				editor.DeleteSelection();
				break;

			case Key.Escape:
				if (editor.Pending.IsActive)
				{
					editor.CancelConnection();
				}
				else if (editor.QuickAdd.IsOpen)
				{
					editor.QuickAdd.Close();
				}
				else if (editor.Find.IsOpen)
				{
					editor.Find.Close();
				}
				else
				{
					editor.ClearSelection();
				}

				break;

			case Key.Space when !control:
				if (!_isSpaceDown)
				{
					_isSpaceDown = true;
					_spaceUsedForPan = false;
				}

				break;

			case Key.C when control:
				editor.Copy();
				break;

			case Key.V when control:
				editor.PasteAt(_lastWorld);
				break;

			case Key.D when control:
				editor.Duplicate();
				break;

			case Key.A when control:
				editor.SelectAll();
				break;

			case Key.Z when control && shift:
			case Key.Y when control:
				editor.Document.Redo();
				break;

			case Key.Z when control:
				editor.Document.Undo();
				break;

			case Key.E when control:
				editor.ToggleDisabled();
				break;

			case Key.F when control:
				editor.Find.Open();
				break;

			case Key.D0 when control:
			case Key.NumPad0 when control:
				editor.FitToViewCommand.Execute(null);
				break;

			case Key.D1 when control:
			case Key.NumPad1 when control:
				editor.ResetZoomCommand.Execute(null);
				break;

			default:
				handled = false;
				break;
		}

		e.Handled = handled;
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);

		if (e.Key != Key.Space || !_isSpaceDown)
			return;

		_isSpaceDown = false;

		// A tap on Space opens the quick-add box; holding it and dragging pans instead.
		if (!_spaceUsedForPan && _editor is { } editor && e.Source is not TextBox)
			editor.QuickAdd.Open(_lastWorld);

		_spaceUsedForPan = false;
	}

	// ---- Quick add and find ------------------------------------------------------------------------------------

	private void OnQuickAddChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(QuickAddViewModel.IsOpen) || _editor is not { } editor)
			return;

		if (!editor.QuickAdd.IsOpen)
		{
			Dispatcher.UIThread.Post(() => Focus());
			return;
		}

		var screen = editor.Viewport.WorldToScreen(editor.QuickAdd.Position);
		var x = Math.Clamp(screen.X, 8, Math.Max(8, Bounds.Width - QuickAddHost.Width - 8));
		var y = Math.Clamp(screen.Y, 8, Math.Max(8, Bounds.Height - 120));

		QuickAddHost.Margin = new Thickness(x, y, 0, 0);

		Dispatcher.UIThread.Post(() =>
		{
			QuickAddBox.Focus();
			QuickAddBox.SelectAll();
		});
	}

	private void OnFindChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(CanvasFindViewModel.IsOpen) || _editor is not { } editor)
			return;

		if (editor.Find.IsOpen)
		{
			Dispatcher.UIThread.Post(() =>
			{
				FindBox.Focus();
				FindBox.SelectAll();
			});
		}
		else
		{
			Dispatcher.UIThread.Post(() => Focus());
		}
	}

	private void OnQuickAddKeyDown(object? sender, KeyEventArgs e)
	{
		if (_editor is not { } editor)
			return;

		switch (e.Key)
		{
			case Key.Down:
				editor.QuickAdd.MoveSelection(1);
				break;

			case Key.Up:
				editor.QuickAdd.MoveSelection(-1);
				break;

			case Key.Enter:
				editor.QuickAdd.Confirm();
				break;

			case Key.Escape:
				editor.QuickAdd.Close();
				break;

			default:
				return;
		}

		e.Handled = true;
	}

	private void OnQuickAddItemActivated(object? sender, RoutedEventArgs e)
	{
		if (_editor is { } editor && (e.Source as Control)?.DataContext is LibraryNodeViewModel item)
			editor.QuickAdd.Confirm(item);
	}

	private void OnFindKeyDown(object? sender, KeyEventArgs e)
	{
		if (_editor is not { } editor)
			return;

		switch (e.Key)
		{
			case Key.Enter when e.KeyModifiers.HasFlag(KeyModifiers.Shift):
				editor.Find.Previous();
				break;

			case Key.Enter:
				editor.Find.Next();
				break;

			case Key.Escape:
				editor.Find.Close();
				break;

			default:
				return;
		}

		e.Handled = true;
	}

	private void OnFindPreviousClick(object? sender, RoutedEventArgs e) => _editor?.Find.Previous();

	private void OnFindNextClick(object? sender, RoutedEventArgs e) => _editor?.Find.Next();

	private void OnFindCloseClick(object? sender, RoutedEventArgs e) => _editor?.Find.Close();
}
