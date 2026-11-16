using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A number setting. The text is kept as typed; text that is not a number is stored as it is and reported as an error.
/// </summary>
public sealed partial class NumberFieldViewModel : ParameterFieldViewModel
{
	[ObservableProperty]
	private string _text = string.Empty;

	public NumberFieldViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(owner, definition, localizer)
	{
	}

	/// <summary>Allowed range as text for the hint under the field, or <c>null</c> if unlimited.</summary>
	public string? RangeHint => (Definition.Minimum, Definition.Maximum) switch
	{
		({ } minimum, { } maximum) => $"{minimum.ToString(CultureInfo.InvariantCulture)} … {maximum.ToString(CultureInfo.InvariantCulture)}",
		({ } minimum, null) => $"≥ {minimum.ToString(CultureInfo.InvariantCulture)}",
		(null, { } maximum) => $"≤ {maximum.ToString(CultureInfo.InvariantCulture)}",
		_ => null,
	};

	public bool HasRangeHint => RangeHint is not null;

	/// <summary>Turns typed text into a setting value. Both <c>.</c> and <c>,</c> work as the decimal separator.</summary>
	public static ParameterValue Parse(string text)
	{
		var trimmed = text.Trim();

		if (trimmed.Length == 0)
		{
			return ParameterValue.Null;
		}

		return decimal.TryParse(trimmed.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
			? ParameterValue.FromNumber(number)
			: ParameterValue.FromString(trimmed);
	}

	partial void OnTextChanged(string value) => Commit(Parse(value));

	protected override void LoadValue()
	{
		var value = CurrentValue;

		Text = value.AsDecimal() is { } number
			? number.ToString(CultureInfo.InvariantCulture)
			: value.AsString();
	}
}
