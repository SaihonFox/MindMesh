using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MindMesh.Presentation.Projects;

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

	/// <summary>The field gets the focus and the whole name is selected as soon as it appears.</summary>
	private void OnRenameVisibleChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (sender is TextBox { IsVisible: true } box)
		{
			box.Focus();
			box.SelectAll();
		}
	}
}
