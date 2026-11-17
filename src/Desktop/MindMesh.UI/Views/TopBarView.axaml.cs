using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MindMesh.Presentation.Shell;

namespace MindMesh.UI.Views;

public partial class TopBarView : UserControl
{
	public TopBarView()
	{
		InitializeComponent();
	}

	/// <summary>Leaving the name field keeps the typed name, like Enter does.</summary>
	private void OnRenameLostFocus(object? sender, RoutedEventArgs e)
	{
		if (DataContext is TopBarViewModel { IsRenaming: true } topBar)
		{
			topBar.CommitRenameCommand.Execute(null);
		}
	}

	/// <summary>The field gets the focus and the whole name is selected as soon as it appears.</summary>
	private void OnRenameBoxVisibleChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (sender is TextBox { IsVisible: true } box)
		{
			box.Focus();
			box.SelectAll();
		}
	}
}
