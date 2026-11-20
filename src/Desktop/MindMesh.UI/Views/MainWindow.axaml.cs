using Avalonia.Controls;
using Avalonia.Input;
using MindMesh.Presentation.Shell;
using MindMesh.UI.Input;

namespace MindMesh.UI.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		// Tunnel, so save and new project work while a text box has the focus.
		AddHandler(KeyDownEvent, OnWindowKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
	}

	private void OnWindowKeyDown(object? sender, KeyEventArgs e)
	{
		if (DataContext is not MainWindowViewModel viewModel || HotkeyInput.ToGesture(e) is not { } gesture)
			return;

		if (viewModel.Hotkeys.Match(gesture) is { } action && viewModel.RunHotkey(action))
			e.Handled = true;
	}
}
