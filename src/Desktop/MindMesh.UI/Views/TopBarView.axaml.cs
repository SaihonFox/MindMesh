using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MindMesh.Presentation.Shell;
using MindMesh.UI.Controls;

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
			topBar.CommitRenameCommand.Execute(null);
	}

	/// <summary>The rename field gets the focus, with the whole name selected, whenever it appears.</summary>
	private void OnRenameBoxAttached(object? sender, VisualTreeAttachmentEventArgs e) => FocusWhenShown.Attach(sender, selectAll: true);
}
