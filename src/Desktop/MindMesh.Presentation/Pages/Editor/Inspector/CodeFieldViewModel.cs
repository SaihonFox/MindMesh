using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A setting edited in the code editor: source code, an expression or JSON.
/// </summary>
public sealed partial class CodeFieldViewModel : ParameterFieldViewModel
{
	[ObservableProperty]
	private string _text = string.Empty;

	public CodeFieldViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(owner, definition, localizer)
	{
	}

	public bool IsExpression => Definition.Type == ParameterType.Expression;

	/// <summary>Language for syntax highlighting (<c>json</c>, <c>sql</c>, …); <c>null</c> for plain text.</summary>
	public string? Language => Definition.Language;

	/// <summary>Visible height of the editor in screen units: expressions need little room, code and JSON more.</summary>
	public double EditorHeight => IsExpression ? 72 : 168;

	partial void OnTextChanged(string value) => Commit(ParameterValue.FromString(value));

	protected override void LoadValue() => Text = CurrentValue.AsString();
}
