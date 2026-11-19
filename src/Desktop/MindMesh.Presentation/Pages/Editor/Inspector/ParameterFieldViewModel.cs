using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Validation;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// An editor for one setting of the selected node. Subclasses exist for each kind of setting; the view picks
/// a template by the subclass.
/// </summary>
public abstract partial class ParameterFieldViewModel : ViewModelBase
{
	[ObservableProperty]
	private bool _isVisible = true;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasError))]
	private string? _errorMessage;

	protected ParameterFieldViewModel(NodeInspectorViewModel owner, ParameterDefinition definition, ILocalizer localizer)
		: base(localizer)
	{
		Owner = owner;
		Definition = definition;
	}

	public NodeInspectorViewModel Owner { get; }

	public ParameterDefinition Definition { get; }

	public string Key => Definition.Key;

	/// <summary>Setting names come from the node definition and are not translated yet.</summary>
	public string Label => Definition.Name;

	public string? Description => Definition.Description;

	public bool HasDescription => !string.IsNullOrEmpty(Definition.Description);

	public string? Placeholder => Definition.Placeholder;

	public bool IsRequired => Definition.IsRequired;

	public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

	/// <summary>True while the field loads a value from the node, so the change is not written back.</summary>
	protected bool IsReloading { get; private set; }

	protected ParameterValue CurrentValue => Owner.Node.Node.GetParameter(Key);

	/// <summary>Shows the value that is stored in the node.</summary>
	public void Reload()
	{
		IsReloading = true;

		try
		{
			LoadValue();
		}
		finally
		{
			IsReloading = false;
		}
	}

	/// <summary>Updates visibility (settings can depend on other settings) and the error text.</summary>
	public void Revalidate()
	{
		IsVisible = Definition.IsVisible(Owner.Node.Node.Parameters);

		if (!IsVisible || ParameterValidator.Validate(Definition, CurrentValue) is not { } problem)
		{
			ErrorMessage = null;
			return;
		}

		ErrorMessage = ValidationMessages.Format(Loc, problem.Code, Label, problem.Detail);
	}

	protected abstract void LoadValue();

	/// <summary>Writes a value into the node as an undoable change.</summary>
	protected void Commit(ParameterValue value)
	{
		if (!IsReloading)
			Owner.CommitParameter(Key, value);
	}

	protected override void OnLanguageChanged()
	{
		OnPropertyChanged(nameof(Label));
		Revalidate();
	}
}
