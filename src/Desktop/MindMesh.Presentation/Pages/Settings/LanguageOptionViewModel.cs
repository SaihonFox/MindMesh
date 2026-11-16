namespace MindMesh.Presentation.Pages.Settings;

/// <summary>
/// One entry of the language selector. Names are shown in their own language on purpose,
/// so a user can always find their language.
/// </summary>
public sealed class LanguageOptionViewModel
{
	public LanguageOptionViewModel(string code, string displayName)
	{
		Code = code;
		DisplayName = displayName;
	}

	public string Code { get; }

	public string DisplayName { get; }
}
