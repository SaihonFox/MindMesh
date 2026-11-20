using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MindMesh.Presentation.Pages.Settings;
using MindMesh.UI.Input;

namespace MindMesh.UI.Views;

public partial class SettingsPageView : UserControl
{
	public SettingsPageView()
	{
		InitializeComponent();

		// Tunnel, so the pressed keys reach the row that waits for a shortcut before anything else reacts to them.
		AddHandler(KeyDownEvent, OnKeyDownTunnel, RoutingStrategies.Tunnel);
		DetachedFromVisualTree += (_, _) => (DataContext as SettingsPageViewModel)?.StopCapture();
	}

	private void OnKeyDownTunnel(object? sender, KeyEventArgs e)
	{
		if (DataContext is not SettingsPageViewModel { CapturingRow: not null } viewModel)
			return;

		if (e.Key == Key.Escape)
		{
			viewModel.StopCapture();
			e.Handled = true;

			return;
		}

		if (HotkeyInput.ToGesture(e) is { } gesture)
		{
			viewModel.Capture(gesture);
			e.Handled = true;
		}
	}
}
