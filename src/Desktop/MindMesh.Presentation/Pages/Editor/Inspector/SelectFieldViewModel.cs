using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A setting with one choice out of several.
/// </summary>
public sealed partial class SelectFieldViewModel : ParameterFieldViewModel
{
	[ObservableProperty]
	private ParameterOption? _selectedOption;

	public SelectFieldViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(owner, definition, localizer)
	{
	}

	public IReadOnlyList<ParameterOption> Options => Definition.Options;

	partial void OnSelectedOptionChanged(ParameterOption? value)
	{
		if (value is not null)
			Commit(ParameterValue.FromString(value.Value));
	}

	protected override void LoadValue()
	{
		var current = CurrentValue.AsString();
		SelectedOption = Definition.Options.FirstOrDefault(option => option.Value == current);
	}
}
