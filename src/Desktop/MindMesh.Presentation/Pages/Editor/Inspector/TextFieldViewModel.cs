using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A single-line text setting.
/// </summary>
public sealed partial class TextFieldViewModel : ParameterFieldViewModel
{
	[ObservableProperty]
	private string _text = string.Empty;

	public TextFieldViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(owner, definition, localizer)
	{
	}

	partial void OnTextChanged(string value) => Commit(ParameterValue.FromString(value));

	protected override void LoadValue() => Text = CurrentValue.AsString();
}
