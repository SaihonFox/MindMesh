using Avalonia;
using Avalonia.Controls;

namespace MindMesh.UI.Views;

public partial class NewProjectDialogView : UserControl
{
	public NewProjectDialogView()
	{
		InitializeComponent();
	}

	/// <summary>The name field gets the focus when the dialog opens, so the user can type at once.</summary>
	private void OnVisibleChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (sender is TextBox { IsVisible: true } box)
		{
			box.Focus();
		}
	}
}
