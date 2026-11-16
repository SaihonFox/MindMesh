using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Platform;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A setting that holds the path of a file, typed or chosen in a dialog.
/// </summary>
public sealed partial class FilePathFieldViewModel : ParameterFieldViewModel
{
	private readonly IFilePickerService _picker;

	[ObservableProperty]
	private string _text = string.Empty;

	public FilePathFieldViewModel(
		NodeInspectorViewModel owner,
		ParameterDefinition definition,
		IFilePickerService picker,
		ILocalizer localizer)
		: base(owner, definition, localizer)
	{
		_picker = picker;
	}

	partial void OnTextChanged(string value) => Commit(ParameterValue.FromString(value));

	[RelayCommand]
	private async Task BrowseAsync()
	{
		var path = await _picker.PickFileAsync(Label);

		if (!string.IsNullOrEmpty(path))
		{
			Text = path;
		}
	}

	protected override void LoadValue() => Text = CurrentValue.AsString();
}
