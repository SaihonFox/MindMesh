using Avalonia.Input;
using MindMesh.Presentation.Hotkeys;

namespace MindMesh.UI.Input;

/// <summary>Turns a key press into the gesture the hotkey service understands.</summary>
public static class HotkeyInput
{
	/// <summary>The gesture of a key press; null for a modifier key pressed alone.</summary>
	public static HotkeyGesture? ToGesture(KeyEventArgs e)
	{
		if (e.Key is Key.None or Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt
			or Key.LWin or Key.RWin or Key.System)
			return null;

		bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);

		return new HotkeyGesture(e.Key.ToString(), ctrl, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Alt));
	}
}
