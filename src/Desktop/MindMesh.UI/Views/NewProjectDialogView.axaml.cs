using Avalonia;
using Avalonia.Controls;
using MindMesh.UI.Controls;

namespace MindMesh.UI.Views;

public partial class NewProjectDialogView : UserControl
{
	public NewProjectDialogView()
	{
		InitializeComponent();
	}

	/// <summary>The name field gets the focus whenever the dialog opens, so the user can type at once.</summary>
	private void OnNameAttached(object? sender, VisualTreeAttachmentEventArgs e) => FocusWhenShown.Attach(sender, selectAll: false);
}
