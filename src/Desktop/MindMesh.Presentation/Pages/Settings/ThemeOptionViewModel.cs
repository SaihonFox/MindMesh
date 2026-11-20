using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Settings;

/// <summary>
/// One theme card of the appearance settings. The colors draw the small preview; they mirror the theme palettes.
/// </summary>
public sealed partial class ThemeOptionViewModel : ViewModelBase
{
	public ThemeOptionViewModel(ThemeMode mode, ILocalizer localizer)
		: base(localizer)
	{
		Mode = mode;

		(Background, Panel, Node, Accent, Text) = mode switch
		{
			ThemeMode.Light => ("#F3F4F6", "#FFFFFF", "#FFFFFF", "#6D5EF5", "#1D2330"),
			ThemeMode.Dark => ("#111318", "#191C23", "#20242D", "#8B7CFF", "#E7EAF0"),
			ThemeMode.Midnight => ("#0A0E1A", "#111729", "#172036", "#5B8CFF", "#E6ECFA"),
			ThemeMode.Forest => ("#0F1511", "#151D18", "#1C2820", "#4FC98A", "#E4EEE6"),
			ThemeMode.Ocean => ("#EAF2F7", "#FFFFFF", "#FFFFFF", "#0E86C9", "#12283A"),
			ThemeMode.Rose => ("#F8F1EF", "#FFFDFC", "#FFFDFC", "#D6457A", "#2B1D1F"),
			_ => ("#F3F4F6", "#191C23", "#20242D", "#6D5EF5", "#1D2330"),
		};
	}

	[ObservableProperty]
	private bool _isSelected;

	public ThemeMode Mode { get; }

	public string Title => Loc[$"Theme_{Mode}"];

	public string Background { get; }

	public string Panel { get; }

	public string Node { get; }

	public string Accent { get; }

	public string Text { get; }

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(Title));
}
