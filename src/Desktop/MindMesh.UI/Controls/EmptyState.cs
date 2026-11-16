using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace MindMesh.UI.Controls;

/// <summary>
/// Centered icon, title and description for panels that have nothing to show yet.
/// Built in code so it needs no template and no binding to itself.
/// </summary>
public sealed class EmptyState : UserControl
{
	public static readonly StyledProperty<string?> IconKeyProperty =
		AvaloniaProperty.Register<EmptyState, string?>(nameof(IconKey));

	public static readonly StyledProperty<string?> TitleProperty =
		AvaloniaProperty.Register<EmptyState, string?>(nameof(Title));

	public static readonly StyledProperty<string?> DescriptionProperty =
		AvaloniaProperty.Register<EmptyState, string?>(nameof(Description));

	private readonly Icon _icon;
	private readonly TextBlock _title;
	private readonly TextBlock _description;

	public EmptyState()
	{
		_icon = new Icon
		{
			Width = 36,
			Height = 36,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		_icon.Classes.Add("empty-icon");

		_title = new TextBlock
		{
			TextAlignment = TextAlignment.Center,
			TextWrapping = TextWrapping.Wrap,
		};
		_title.Classes.Add("h3");

		_description = new TextBlock
		{
			TextAlignment = TextAlignment.Center,
			TextWrapping = TextWrapping.Wrap,
		};
		_description.Classes.Add("secondary");

		Content = new StackPanel
		{
			Spacing = 8,
			MaxWidth = 340,
			Margin = new Thickness(16),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Children = { _icon, _title, _description },
		};
	}

	public string? IconKey
	{
		get => GetValue(IconKeyProperty);
		set => SetValue(IconKeyProperty, value);
	}

	public string? Title
	{
		get => GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string? Description
	{
		get => GetValue(DescriptionProperty);
		set => SetValue(DescriptionProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == IconKeyProperty)
		{
			_icon.Key = change.GetNewValue<string?>();
		}
		else if (change.Property == TitleProperty)
		{
			_title.Text = change.GetNewValue<string?>();
		}
		else if (change.Property == DescriptionProperty)
		{
			_description.Text = change.GetNewValue<string?>();
		}
	}
}
