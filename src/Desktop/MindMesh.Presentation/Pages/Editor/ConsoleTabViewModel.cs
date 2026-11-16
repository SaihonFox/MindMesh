using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// One tab of the execution console.
/// </summary>
public sealed class ConsoleTabViewModel : ViewModelBase
{
	public ConsoleTabViewModel(ConsoleTabId id, string iconKey, ILocalizer localizer)
		: base(localizer)
	{
		Id = id;
		IconKey = iconKey;
	}

	public ConsoleTabId Id { get; }

	public string IconKey { get; }

	public string Title => Loc[$"Console_{Id}"];

	public string EmptyDescription => Loc[$"ConsoleEmpty_{Id}"];

	protected override void OnLanguageChanged()
	{
		OnPropertyChanged(nameof(Title));
		OnPropertyChanged(nameof(EmptyDescription));
	}
}
