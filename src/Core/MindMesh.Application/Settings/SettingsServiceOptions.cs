namespace MindMesh.Application.Settings;

public sealed record SettingsServiceOptions
{
	/// <summary>How long to wait after the last change before writing the settings file.</summary>
	public TimeSpan SaveDelay { get; init; } = TimeSpan.FromMilliseconds(500);
}
