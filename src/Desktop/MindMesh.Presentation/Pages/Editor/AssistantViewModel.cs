using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// AI Assistant panel. The assistant itself arrives in Phase 6; until an AI provider exists the panel says so.
/// </summary>
public sealed class AssistantViewModel : ViewModelBase
{
	public AssistantViewModel(ILocalizer localizer)
		: base(localizer)
	{
	}

	/// <summary>True when an AI provider is configured. Always false until Phase 6.</summary>
	public bool IsAvailable => false;
}
