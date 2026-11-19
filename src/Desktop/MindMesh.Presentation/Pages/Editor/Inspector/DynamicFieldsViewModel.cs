using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A setting made of any number of name and value pairs (headers, query parameters, field mappings).
/// </summary>
public sealed partial class DynamicFieldsViewModel : ParameterFieldViewModel
{
	public DynamicFieldsViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(owner, definition, localizer)
	{
	}

	public ObservableCollection<KeyValueRowViewModel> Rows { get; } = [];

	public bool HasRows => Rows.Count > 0;

	[RelayCommand]
	private void AddRow()
	{
		Rows.Add(new KeyValueRowViewModel(this, string.Empty, string.Empty));
		OnPropertyChanged(nameof(HasRows));
	}

	[RelayCommand]
	private void RemoveRow(KeyValueRowViewModel? row)
	{
		if (row is null || !Rows.Remove(row))
			return;

		OnPropertyChanged(nameof(HasRows));
		Save();
	}

	internal void Save() =>
		Commit(ParameterValue.FromPairs(Rows
			.Where(row => row.Key.Length > 0)
			.Select(row => new KeyValuePair<string, string>(row.Key, row.Value))));

	protected override void LoadValue()
	{
		Rows.Clear();

		foreach (var pair in CurrentValue.AsPairs())
			Rows.Add(new KeyValueRowViewModel(this, pair.Key, pair.Value));

		OnPropertyChanged(nameof(HasRows));
	}
}

/// <summary>
/// One name and value pair of a <see cref="DynamicFieldsViewModel"/>.
/// </summary>
public sealed partial class KeyValueRowViewModel : ObservableObject
{
	private readonly DynamicFieldsViewModel _owner;

	[ObservableProperty]
	private string _key;

	[ObservableProperty]
	private string _value;

	public KeyValueRowViewModel(DynamicFieldsViewModel owner, string key, string value)
	{
		_owner = owner;
		_key = key;
		_value = value;
	}

	[RelayCommand]
	private void Remove() => _owner.RemoveRowCommand.Execute(this);

	partial void OnKeyChanged(string value) => _owner.Save();

	partial void OnValueChanged(string value) => _owner.Save();
}
