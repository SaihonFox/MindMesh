using MindMesh.Graph.Validation;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// Turns validation codes into text in the user's language.
/// </summary>
public static class ValidationMessages
{
	/// <param name="localizer">Source of the texts.</param>
	/// <param name="code">What is wrong.</param>
	/// <param name="subject">Name of the setting or port.</param>
	/// <param name="detail">Extra value, such as the allowed minimum.</param>
	public static string Format(ILocalizer localizer, ValidationCode code, string subject, string? detail) =>
		string.Format(localizer.Culture, localizer[$"Validation_{code}"], subject, detail ?? string.Empty);
}
