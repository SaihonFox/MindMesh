using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;

namespace MindMesh.Presentation.Pages.Placeholder;

/// <summary>
/// A section that is part of the product plan but not built yet. It states which development phase delivers it.
/// The title is the sidebar title of the section, so no extra texts are needed.
/// </summary>
public sealed class PlaceholderPageViewModel : PageViewModel
{
	public PlaceholderPageViewModel(PageKey key, int phase, ILocalizer localizer)
		: base(key, localizer)
	{
		Phase = phase;
	}

	/// <summary>Development phase (1–10) in which the section is built.</summary>
	public int Phase { get; }

	public string Title => Loc[$"Sidebar_{Key}"];

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(Title));
}
