namespace MindMesh.Application.Localization;

/// <summary>
/// A language the user interface can be shown in.
/// </summary>
public sealed record AppLanguage(string Code, string NativeName);

/// <summary>
/// Supported UI languages and the rules for resolving the effective language.
/// </summary>
public static class AppLanguages
{
	/// <summary>Follow the operating system language.</summary>
	public const string Auto = "auto";

	public const string English = "en";

	public const string Russian = "ru";

	/// <summary>Language used when nothing else matches.</summary>
	public const string Fallback = English;

	/// <summary>Concrete languages with translations (without <see cref="Auto"/>).</summary>
	public static IReadOnlyList<AppLanguage> Supported { get; } =
	[
		new AppLanguage(English, "English"),
		new AppLanguage(Russian, "Русский"),
	];

	/// <summary>
	/// True if the code is <see cref="Auto"/> or a supported language code.
	/// </summary>
	public static bool IsValidSetting(string? code) =>
		code is Auto || IsSupported(code);

	public static bool IsSupported(string? code) =>
		code is English or Russian;

	/// <summary>
	/// Resolves a stored setting to a concrete language code.
	/// </summary>
	/// <param name="setting">Stored language setting (<see cref="Auto"/> or a language code).</param>
	/// <param name="systemTwoLetterCode">Two-letter ISO code of the operating system language.</param>
	public static string Resolve(string? setting, string? systemTwoLetterCode)
	{
		if (IsSupported(setting))
		{
			return setting!;
		}

		return IsSupported(systemTwoLetterCode) ? systemTwoLetterCode! : Fallback;
	}
}
