using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A yes/no setting.
/// </summary>
public sealed partial class BooleanFieldViewModel : ParameterFieldViewModel
{
	[ObservableProperty]
	private bool _isChecked;

	public BooleanFieldViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(owner, definition, localizer)
	{
	}

	partial void OnIsCheckedChanged(bool value) => Commit(ParameterValue.FromBoolean(value));

	protected override void LoadValue() => IsChecked = CurrentValue.AsBoolean();
}
