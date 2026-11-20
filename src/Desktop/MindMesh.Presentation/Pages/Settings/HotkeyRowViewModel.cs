using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Hotkeys;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Settings;

/// <summary>
/// One line of the hotkey settings: an action and the shortcut that triggers it.
/// </summary>
public sealed partial class HotkeyRowViewModel : ViewModelBase
{
	private readonly HotkeyService _hotkeys;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Shortcut))]
	private bool _isCapturing;

	[ObservableProperty]
	private string? _conflict;

	public HotkeyRowViewModel(HotkeyAction action, HotkeyService hotkeys, ILocalizer localizer)
		: base(localizer)
	{
		Action = action;
		_hotkeys = hotkeys;
	}

	public HotkeyAction Action { get; }

	public string Title => Loc[$"Hotkey_{Action}"];

	public string ResetText => Loc["Settings_HotkeyReset"];

	public bool IsCustom => _hotkeys.IsCustom(Action);

	/// <summary>The shortcut as text; while the row waits for a key it asks for one.</summary>
	public string Shortcut => IsCapturing
		? Loc["Settings_PressKeys"]
		: string.Join("  ·  ", _hotkeys.GesturesOf(Action).Select(gesture => gesture.Display));

	/// <summary>Re-reads the shortcut after it changed.</summary>
	public void Refresh()
	{
		OnPropertyChanged(nameof(Shortcut));
		OnPropertyChanged(nameof(IsCustom));
	}

	protected override void OnLanguageChanged()
	{
		OnPropertyChanged(nameof(Title));
		OnPropertyChanged(nameof(ResetText));
		OnPropertyChanged(nameof(Shortcut));
	}
}
