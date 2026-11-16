using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MindMesh.Plugins.Abstractions.Nodes;

namespace MindMesh.UI.Controls;

/// <summary>
/// A rounded box in the color family of a category: a soft background, and a strong foreground for the icon or text inside.
/// Colors come from theme resources, so the pill follows the light and dark themes by itself.
/// </summary>
public sealed class TonePill : Border
{
	public static readonly StyledProperty<AccentTone> ToneProperty =
		AvaloniaProperty.Register<TonePill, AccentTone>(nameof(Tone), AccentTone.Gray);

	private IDisposable? _backgroundBinding;
	private IDisposable? _foregroundBinding;

	static TonePill()
	{
		ToneProperty.Changed.AddClassHandler<TonePill>((pill, _) => pill.ApplyTone());
	}

	public AccentTone Tone
	{
		get => GetValue(ToneProperty);
		set => SetValue(ToneProperty, value);
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);
		ApplyTone();
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		_backgroundBinding?.Dispose();
		_foregroundBinding?.Dispose();
		_backgroundBinding = null;
		_foregroundBinding = null;
	}

	private void ApplyTone()
	{
		if (TopLevel.GetTopLevel(this) is null)
		{
			return;
		}

		_backgroundBinding?.Dispose();
		_foregroundBinding?.Dispose();

		_backgroundBinding = this.Bind(BackgroundProperty, this.GetResourceObservable($"MM.Brush.Tone.{Tone}.Soft"));
		_foregroundBinding = this.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable($"MM.Brush.Tone.{Tone}"));
	}
}
