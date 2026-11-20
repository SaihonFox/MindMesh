namespace MindMesh.Presentation.Hotkeys;

/// <summary>An action with its default shortcuts.</summary>
/// <param name="Action">The action.</param>
/// <param name="Defaults">Shortcuts used until the user picks one; any of them triggers the action.</param>
public sealed record HotkeyDefinition(HotkeyAction Action, IReadOnlyList<HotkeyGesture> Defaults)
{
	/// <summary>The key of the localized name of the action.</summary>
	public string TitleKey => $"Hotkey_{Action}";
}
