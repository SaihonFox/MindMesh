using System.Diagnostics;
using MindMesh.Presentation.Platform;

namespace MindMesh.UI.Platform;

/// <summary>
/// Opens folders in the file manager of the operating system.
/// </summary>
public sealed class ProcessShellService : IShellService
{
	public bool OpenFolder(string path)
	{
		try
		{
			Directory.CreateDirectory(path);

			string program = OperatingSystem.IsWindows() ? "explorer.exe" : OperatingSystem.IsMacOS() ? "open" : "xdg-open";

			using Process? process = Process.Start(new ProcessStartInfo(program) { ArgumentList = { path }, UseShellExecute = false });

			return process is not null;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception or InvalidOperationException)
		{
			return false;
		}
	}
}
