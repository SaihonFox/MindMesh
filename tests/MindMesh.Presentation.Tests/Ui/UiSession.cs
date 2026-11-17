using Avalonia.Headless;

namespace MindMesh.Presentation.Tests.Ui;

/// <summary>One headless Avalonia session shared by the UI tests; it owns the UI thread.</summary>
internal static class UiSession
{
	private static readonly Lazy<HeadlessUnitTestSession> Session = new(() => HeadlessUnitTestSession.StartNew(typeof(UiTestApp)));

	public static Task<T> Run<T>(Func<T> action) => Session.Value.Dispatch(action, CancellationToken.None);
}
