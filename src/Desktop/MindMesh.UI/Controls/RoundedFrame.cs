using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MindMesh.UI.Controls;

/// <summary>
/// A floating panel: rounded, clipped to its corners, with the border drawn on top of the content
/// so the corners keep their line even when the content paints a background to the very edge.
/// The look comes from the template in the styles.
/// </summary>
public sealed class RoundedFrame : ContentControl
{
	public static readonly StyledProperty<BoxShadows> ShadowProperty =
		AvaloniaProperty.Register<RoundedFrame, BoxShadows>(nameof(Shadow));

	/// <summary>An optional drop shadow, drawn outside the rounded shape.</summary>
	public BoxShadows Shadow
	{
		get => GetValue(ShadowProperty);
		set => SetValue(ShadowProperty, value);
	}
}
