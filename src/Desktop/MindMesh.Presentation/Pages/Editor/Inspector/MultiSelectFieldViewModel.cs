using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A setting with any number of choices ticked.
/// </summary>
public sealed class MultiSelectFieldViewModel : ParameterFieldViewModel
{
	public MultiSelectFieldViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(owner, definition, localizer)
	{
		Items = [.. definition.Options.Select(option => new MultiSelectItemViewModel(option, this))];
	}

	public IReadOnlyList<MultiSelectItemViewModel> Items { get; }

	internal void OnItemChanged() =>
		Commit(ParameterValue.FromStringList(Items.Where(item => item.IsChecked).Select(item => item.Option.Value)));

	protected override void LoadValue()
	{
		var selected = CurrentValue.AsStringList();

		foreach (var item in Items)
		{
			item.SetCheckedSilently(selected.Contains(item.Option.Value));
		}
	}
}

/// <summary>
/// One tick box of a <see cref="MultiSelectFieldViewModel"/>.
/// </summary>
public sealed partial class MultiSelectItemViewModel : ObservableObject
{
	private readonly MultiSelectFieldViewModel _owner;
	private bool _silent;

	[ObservableProperty]
	private bool _isChecked;

	public MultiSelectItemViewModel(ParameterOption option, MultiSelectFieldViewModel owner)
	{
		Option = option;
		_owner = owner;
	}

	public ParameterOption Option { get; }

	public string Label => Option.Label;

	internal void SetCheckedSilently(bool value)
	{
		_silent = true;

		try
		{
			IsChecked = value;
		}
		finally
		{
			_silent = false;
		}
	}

	partial void OnIsCheckedChanged(bool value)
	{
		if (!_silent)
		{
			_owner.OnItemChanged();
		}
	}
}
