using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;

namespace MindMesh.UI.Controls;

/// <summary>
/// A code editor for the Code, Expression and JSON settings of nodes. It wraps the AvaloniaEdit editor
/// in a bordered box and exposes the text and language as bindable properties.
/// </summary>
public sealed class CodeEditorField : Border
{
	public static readonly StyledProperty<string?> TextProperty =
		AvaloniaProperty.Register<CodeEditorField, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

	public static readonly StyledProperty<string?> LanguageProperty =
		AvaloniaProperty.Register<CodeEditorField, string?>(nameof(Language));

	private readonly TextEditor _editor;
	private bool _isUpdating;

	static CodeEditorField()
	{
		TextProperty.Changed.AddClassHandler<CodeEditorField>((field, e) => field.OnTextPropertyChanged(e.NewValue as string));
		LanguageProperty.Changed.AddClassHandler<CodeEditorField>((field, e) => field.ApplyLanguage(e.NewValue as string));
	}

	public CodeEditorField()
	{
		_editor = new TextEditor
		{
			FontFamily = new FontFamily("Cascadia Code, Consolas, Menlo, monospace"),
			FontSize = 12,
			ShowLineNumbers = false,
			WordWrap = true,
			Background = Brushes.Transparent,
			Padding = new Thickness(8, 6),
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
		};

		_editor.TextChanged += OnEditorTextChanged;
		_editor.Bind(TemplatedControl.ForegroundProperty, _editor.GetResourceObservable("MM.Brush.TextPrimary"));

		Child = _editor;
		ClipToBounds = true;
		CornerRadius = new CornerRadius(6);
		BorderThickness = new Thickness(1);

		this.Bind(BackgroundProperty, this.GetResourceObservable("MM.Brush.PanelAlt"));
		this.Bind(BorderBrushProperty, this.GetResourceObservable("MM.Brush.Border"));
	}

	public string? Text
	{
		get => GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	/// <summary>Language for syntax highlighting; unknown languages are shown as plain text.</summary>
	public string? Language
	{
		get => GetValue(LanguageProperty);
		set => SetValue(LanguageProperty, value);
	}

	private void OnTextPropertyChanged(string? value)
	{
		if (_isUpdating)
			return;

		var text = value ?? string.Empty;

		if (_editor.Text != text)
		{
			_isUpdating = true;

			try
			{
				_editor.Text = text;
			}
			finally
			{
				_isUpdating = false;
			}
		}
	}

	private void OnEditorTextChanged(object? sender, EventArgs e)
	{
		if (_isUpdating)
			return;

		_isUpdating = true;

		try
		{
			SetCurrentValue(TextProperty, _editor.Text);
		}
		finally
		{
			_isUpdating = false;
		}
	}

	private void ApplyLanguage(string? language)
	{
		var name = language?.ToLowerInvariant() switch
		{
			"json" => "Json",
			"javascript" or "js" => "JavaScript",
			"csharp" or "c#" => "C#",
			"xml" => "XML",
			"html" => "HTML",
			"sql" => "SQL",
			_ => null,
		};

		try
		{
			_editor.SyntaxHighlighting = name is null ? null : HighlightingManager.Instance.GetDefinition(name);
		}
		catch (Exception)
		{
			// A missing highlighting definition only costs the colors.
			_editor.SyntaxHighlighting = null;
		}
	}
}
