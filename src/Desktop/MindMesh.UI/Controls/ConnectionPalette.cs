using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MindMesh.Graph.Ports;

namespace MindMesh.UI.Controls;

/// <summary>
/// Looks up the colors the canvas layers draw with, in the theme that is active at the moment.
/// </summary>
internal sealed class ConnectionPalette
{
	public IBrush? Data { get; private set; }

	public IBrush? Execution { get; private set; }

	public IBrush? Event { get; private set; }

	public IBrush? AiContext { get; private set; }

	public IBrush? Accent { get; private set; }

	public IBrush? PillBackground { get; private set; }

	public IBrush? PillText { get; private set; }

	public IBrush? PillBorder { get; private set; }

	public IBrush? Selection { get; private set; }

	public IBrush? SelectionBorder { get; private set; }

	public IBrush? ForKind(PortKind kind) => kind switch
	{
		PortKind.Execution => Execution,
		PortKind.Event => Event,
		PortKind.AIContext => AiContext,
		_ => Data,
	};

	public void Refresh(Control owner)
	{
		Data = Find(owner, "MM.Brush.PortData");
		Execution = Find(owner, "MM.Brush.PortExecution");
		Event = Find(owner, "MM.Brush.PortEvent");
		AiContext = Find(owner, "MM.Brush.PortAIContext");
		Accent = Find(owner, "MM.Brush.Accent");
		PillBackground = Find(owner, "MM.Brush.Overlay");
		PillText = Find(owner, "MM.Brush.TextSecondary");
		PillBorder = Find(owner, "MM.Brush.Border");
		Selection = Find(owner, "MM.Brush.Selection");
		SelectionBorder = Find(owner, "MM.Brush.Accent");
	}

	private static IBrush? Find(Control owner, string key) =>
		owner.TryFindResource(key, owner.ActualThemeVariant, out var resource) ? resource as IBrush : null;
}
