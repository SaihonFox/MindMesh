using MindMesh.Application.Settings;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Settings;

/// <summary>
/// One entry of the theme selector.
/// </summary>
public sealed class ThemeOptionViewModel : ViewModelBase
{
	public ThemeOptionViewModel(ThemeMode mode, ILocalizer localizer)
		: base(localizer)
	{
		Mode = mode;
	}

	public ThemeMode Mode { get; }

	public string Title => Loc[$"Theme_{Mode}"];

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(Title));
}
