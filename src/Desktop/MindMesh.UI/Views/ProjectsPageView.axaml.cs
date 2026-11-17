using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MindMesh.Presentation.Projects;
using MindMesh.UI.Controls;

namespace MindMesh.UI.Views;

public partial class ProjectsPageView : UserControl
{
	public ProjectsPageView()
	{
		InitializeComponent();
	}

	/// <summary>Leaving the name field keeps the typed name, like Enter does.</summary>
	private void OnRenameLostFocus(object? sender, RoutedEventArgs e)
	{
		if (sender is Control { DataContext: ProjectItemViewModel { IsRenaming: true } item })
		{
			item.CommitRenameCommand.Execute(null);
		}
	}

	/// <summary>The rename field gets the focus, with the whole name selected, whenever it appears.</summary>
	private void OnRenameAttached(object? sender, VisualTreeAttachmentEventArgs e) => FocusWhenShown.Attach(sender, selectAll: true);
}
}
