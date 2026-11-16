using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Secrets;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Model;
using MindMesh.Graph.Validation;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Pages.Editor.Nodes;
using MindMesh.Presentation.Platform;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// The inspector content for one node: its name, on/off switch, problems and a field for every setting.
/// Fields are built from the node's definition, so new node types need no inspector code.
/// </summary>
public sealed partial class NodeInspectorViewModel : ViewModelBase, IDisposable
{
	private readonly GraphEditorViewModel _editor;
	private readonly NodeText _text;
	private bool _isCommitting;
	private bool _isReloading;

	[ObservableProperty]
	private string _title = string.Empty;

	[ObservableProperty]
	private bool _isDisabled;

	public NodeInspectorViewModel(
		NodeViewModel node,
		GraphEditorViewModel editor,
		NodeText text,
		ISecretStore secrets,
		IFilePickerService picker,
		ILocalizer localizer)
		: base(localizer)
	{
		Node = node;
		_editor = editor;
		_text = text;

		Fields = [.. node.Definition.Parameters.Select(parameter => CreateField(parameter, secrets, picker))];

		_title = node.Node.Title ?? string.Empty;
		_isDisabled = node.Node.IsDisabled;

		foreach (var field in Fields)
		{
			field.Reload();
		}

		RefreshValidation();
		editor.DocumentChanged += OnDocumentChanged;
	}

	public NodeViewModel Node { get; }

	public IReadOnlyList<ParameterFieldViewModel> Fields { get; }

	/// <summary>Problems that are not tied to a setting, such as a required input without a connection.</summary>
	public ObservableCollection<string> Problems { get; } = [];

	public bool HasProblems => Problems.Count > 0;

	public bool HasFields => Fields.Count > 0;

	public string NodeName => _text.GetName(Node.Definition);

	public string NodeDescription => _text.GetDescription(Node.Definition);

	public string CategoryTitle => _text.GetPrimaryCategoryTitle(Node.Definition);

	public string IconKey => Node.Definition.IconKey;

	public AccentTone Tone => _text.GetTone(Node.Definition);

	/// <summary>Shown in the empty name box: the name the node has by default.</summary>
	public string TitlePlaceholder => NodeName;

	/// <summary>Writes a setting into the node as an undoable change. Called by the fields.</summary>
	internal void CommitParameter(string key, ParameterValue value)
	{
		_isCommitting = true;

		try
		{
			_editor.SetParameter(Node, key, value);
		}
		finally
		{
			_isCommitting = false;
		}
	}

	partial void OnTitleChanged(string value)
	{
		if (_isReloading)
		{
			return;
		}

		_isCommitting = true;

		try
		{
			_editor.SetTitle(Node, value);
		}
		finally
		{
			_isCommitting = false;
		}
	}

	partial void OnIsDisabledChanged(bool value)
	{
		if (_isReloading)
		{
			return;
		}

		_isCommitting = true;

		try
		{
			_editor.SetDisabled(Node, value);
		}
		finally
		{
			_isCommitting = false;
		}
	}

	public void Dispose() => _editor.DocumentChanged -= OnDocumentChanged;

	protected override void OnLanguageChanged()
	{
		OnPropertyChanged(nameof(NodeName));
		OnPropertyChanged(nameof(NodeDescription));
		OnPropertyChanged(nameof(CategoryTitle));
		OnPropertyChanged(nameof(TitlePlaceholder));
		RefreshValidation();
	}

	private void OnDocumentChanged(object? sender, GraphChangedEventArgs e)
	{
		switch (e.Change)
		{
			case ParameterChanged changed when changed.Node == Node.Id:
				if (!_isCommitting)
				{
					Fields.FirstOrDefault(field => field.Key == changed.Key)?.Reload();
				}

				RefreshValidation();
				break;

			case TitleChanged changed when changed.Node == Node.Id && !_isCommitting:
				Reload(() => Title = changed.NewTitle ?? string.Empty);
				break;

			case DisabledChanged changed when changed.Node == Node.Id && !_isCommitting:
				Reload(() => IsDisabled = changed.IsDisabled);
				RefreshValidation();
				break;

			case ConnectionAdded or ConnectionRemoved:
				RefreshValidation();
				break;
		}
	}

	private void Reload(Action change)
	{
		_isReloading = true;

		try
		{
			change();
		}
		finally
		{
			_isReloading = false;
		}
	}

	private void RefreshValidation()
	{
		foreach (var field in Fields)
		{
			field.Revalidate();
		}

		Problems.Clear();

		foreach (var issue in Node.Issues.Where(issue => issue.Code == ValidationCode.RequiredInputNotConnected))
		{
			var port = issue.Key is null ? null : Node.FindPort(issue.Key);
			Problems.Add(ValidationMessages.Format(Loc, issue.Code, port?.Name ?? issue.Key ?? string.Empty, issue.Detail));
		}

		OnPropertyChanged(nameof(HasProblems));
	}

	private ParameterFieldViewModel CreateField(ParameterDefinition parameter, ISecretStore secrets, IFilePickerService picker) =>
		parameter.Type switch
		{
			ParameterType.Number => new NumberFieldViewModel(this, parameter, Loc),
			ParameterType.Boolean => new BooleanFieldViewModel(this, parameter, Loc),
			ParameterType.Select => new SelectFieldViewModel(this, parameter, Loc),
			ParameterType.MultiSelect => new MultiSelectFieldViewModel(this, parameter, Loc),
			ParameterType.Code or ParameterType.Expression or ParameterType.Json => new CodeFieldViewModel(this, parameter, Loc),
			ParameterType.Secret => new SecretFieldViewModel(this, parameter, secrets, Loc),
			ParameterType.FilePicker => new FilePathFieldViewModel(this, parameter, picker, Loc),
			ParameterType.DynamicFields => new DynamicFieldsViewModel(this, parameter, Loc),
			_ => new TextFieldViewModel(this, parameter, Loc),
		};
}
