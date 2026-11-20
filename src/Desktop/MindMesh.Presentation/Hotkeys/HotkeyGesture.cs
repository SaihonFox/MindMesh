namespace MindMesh.Presentation.Hotkeys;

/// <summary>
/// A key with modifiers, kept as text so the presentation layer needs no UI types. <see cref="Key"/> is the name of
/// the key as the UI layer reports it (for example <c>Z</c>, <c>Delete</c>, <c>D0</c>).
/// </summary>
public readonly record struct HotkeyGesture(string Key, bool Ctrl = false, bool Shift = false, bool Alt = false)
{
	/// <summary>Reads <c>Ctrl+Shift+Z</c>; false for text that is not a gesture.</summary>
	public static bool TryParse(string? text, out HotkeyGesture gesture)
	{
		gesture = default;

		if (string.IsNullOrWhiteSpace(text))
			return false;

		string[] parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length == 0)
			return false;

		bool ctrl = false;
		bool shift = false;
		bool alt = false;

		foreach (string part in parts[..^1])
		{
			switch (part.ToLowerInvariant())
			{
				case "ctrl":
					ctrl = true;
					break;
				case "shift":
					shift = true;
					break;
				case "alt":
					alt = true;
					break;
				default:
					return false;
			}
		}

		gesture = new HotkeyGesture(parts[^1], ctrl, shift, alt);

		return true;
	}

	/// <summary>The text that is stored in the settings: <c>Ctrl+Shift+Z</c>.</summary>
	public override string ToString() => Prefix() + Key;

	/// <summary>The text shown to the user: digit keys without the technical prefix.</summary>
	public string Display
	{
		get
		{
			string key = Key switch
			{
				_ when Key.Length == 2 && Key[0] == 'D' && char.IsDigit(Key[1]) => Key[1].ToString(),
				_ when Key.StartsWith("NumPad", StringComparison.Ordinal) => "Num " + Key[6..],
				"Back" => "Backspace",
				_ => Key,
			};

			return Prefix() + key;
		}
	}

	private string Prefix() => (Ctrl ? "Ctrl+" : string.Empty) + (Shift ? "Shift+" : string.Empty) + (Alt ? "Alt+" : string.Empty);
}
