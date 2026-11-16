using System.Text.Json.Serialization;
using MindMesh.Application.Localization;

namespace MindMesh.Application.Settings;

/// <summary>
/// User settings stored on the local machine.
/// </summary>
public sealed record AppSettings
{
	/// <summary>Version of the settings file format. Increase when the format changes.</summary>
	public const int CurrentSchemaVersion = 1;

	[JsonConstructor]
	public AppSettings()
	{
	}

	public int SchemaVersion { get; init; } = CurrentSchemaVersion;

	/// <summary><see cref="AppLanguages.Auto"/> or a supported language code.</summary>
	public string Language { get; init; } = AppLanguages.Auto;

	public ThemeMode Theme { get; init; } = ThemeMode.Light;

	public bool ShowWelcomeOnStartup { get; init; } = true;

	public LayoutSettings Layout { get; init; } = new();

	/// <summary>
	/// Returns a copy in which every value is valid. Used after loading and after every update.
	/// </summary>
	public AppSettings Normalize() => this with
	{
		SchemaVersion = CurrentSchemaVersion,
		Language = AppLanguages.IsValidSetting(Language) ? Language : AppLanguages.Auto,
		Theme = Enum.IsDefined(Theme) ? Theme : ThemeMode.Light,
		Layout = (Layout ?? new LayoutSettings()).Normalize(),
	};
}
