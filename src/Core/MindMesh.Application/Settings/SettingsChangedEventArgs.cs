namespace MindMesh.Application.Settings;

public sealed class SettingsChangedEventArgs(AppSettings previous, AppSettings current) : EventArgs
{
	public AppSettings Previous { get; } = previous;

	public AppSettings Current { get; } = current;
}
