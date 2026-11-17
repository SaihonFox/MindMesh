using Avalonia;
using Avalonia.Controls;

namespace MindMesh.UI.Controls;

/// <summary>Gives a text box the keyboard focus every time it becomes visible, for fields that appear on demand.</summary>
public static class FocusWhenShown
{
	/// <summary>Call from the box's <c>AttachedToVisualTree</c> event.</summary>
	public static void Attach(object? sender, bool selectAll)
	{
		if (sender is not TextBox box)
		{
			return;
		}

		box.PropertyChanged += (_, e) =>
		{
			if (e.Property != Visual.IsVisibleProperty || !box.IsVisible)
			{
				return;
			}

			box.Focus();

			if (selectAll)
			{
				box.SelectAll();
			}
		};
	}
}
