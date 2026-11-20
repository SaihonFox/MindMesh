using MindMesh.Application.Settings;

namespace MindMesh.Presentation.Hotkeys;

/// <summary>
/// Knows which shortcut triggers which action: the defaults, changed by what the user chose in the settings.
/// A shortcut the user picks replaces all defaults of that action.
/// </summary>
public sealed class HotkeyService
{
	private readonly ISettingsService _settings;

	public HotkeyService(ISettingsService settings)
	{
		_settings = settings;
	}

	public static IReadOnlyList<HotkeyDefinition> Definitions { get; } =
	[
		new(HotkeyAction.SaveProject, [Ctrl("S")]),
		new(HotkeyAction.NewProject, [Ctrl("N")]),
		new(HotkeyAction.Undo, [Ctrl("Z")]),
		new(HotkeyAction.Redo, [new HotkeyGesture("Z", Ctrl: true, Shift: true), Ctrl("Y")]),
		new(HotkeyAction.Copy, [Ctrl("C")]),
		new(HotkeyAction.Paste, [Ctrl("V")]),
		new(HotkeyAction.Duplicate, [Ctrl("D")]),
		new(HotkeyAction.SelectAll, [Ctrl("A")]),
		new(HotkeyAction.DeleteSelection, [new HotkeyGesture("Delete"), new HotkeyGesture("Back")]),
		new(HotkeyAction.ToggleDisabled, [Ctrl("E")]),
		new(HotkeyAction.Find, [Ctrl("F")]),
		new(HotkeyAction.FitToView, [Ctrl("D0"), Ctrl("NumPad0")]),
		new(HotkeyAction.ResetZoom, [Ctrl("D1"), Ctrl("NumPad1")]),
	];

	/// <summary>True while the settings wait for the user to press a new shortcut; no action runs then.</summary>
	public bool IsCapturing { get; set; }

	/// <summary>Raised after a shortcut was changed or reset.</summary>
	public event EventHandler? Changed;

	/// <summary>The shortcuts that trigger an action now.</summary>
	public IReadOnlyList<HotkeyGesture> GesturesOf(HotkeyAction action)
	{
		if (_settings.Current.Hotkeys.TryGetValue(action.ToString(), out string? text) && HotkeyGesture.TryParse(text, out HotkeyGesture custom))
			return [custom];

		return Definitions.First(definition => definition.Action == action).Defaults;
	}

	/// <summary>True when the user changed the shortcut of the action.</summary>
	public bool IsCustom(HotkeyAction action) => _settings.Current.Hotkeys.ContainsKey(action.ToString());

	/// <summary>The action a pressed key triggers, or null.</summary>
	public HotkeyAction? Match(HotkeyGesture pressed) => IsCapturing ? null : FindOwner(pressed);

	private HotkeyAction? FindOwner(HotkeyGesture pressed)
	{
		foreach (HotkeyDefinition definition in Definitions)
		{
			if (GesturesOf(definition.Action).Contains(pressed))
				return definition.Action;
		}

		return null;
	}

	/// <summary>
	/// Gives an action a new shortcut. When another action already uses it, nothing changes and that action is returned.
	/// </summary>
	public HotkeyAction? Assign(HotkeyAction action, HotkeyGesture gesture)
	{
		if (FindOwner(gesture) is { } owner && owner != action)
			return owner;

		_settings.Update(settings => settings with
		{
			Hotkeys = new Dictionary<string, string>(settings.Hotkeys) { [action.ToString()] = gesture.ToString() },
		});
		Changed?.Invoke(this, EventArgs.Empty);

		return null;
	}

	public void Reset(HotkeyAction action)
	{
		_settings.Update(settings =>
		{
			Dictionary<string, string> hotkeys = new(settings.Hotkeys);

			hotkeys.Remove(action.ToString());

			return settings with { Hotkeys = hotkeys };
		});
		Changed?.Invoke(this, EventArgs.Empty);
	}

	public void ResetAll()
	{
		_settings.Update(settings => settings with { Hotkeys = [] });
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private static HotkeyGesture Ctrl(string key) => new(key, Ctrl: true);
}
