using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Secrets;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// A secret setting (password, token, connection string). The node stores only a reference;
/// the secret itself goes to the <see cref="ISecretStore"/>.
/// </summary>
public sealed partial class SecretFieldViewModel : ParameterFieldViewModel
{
	private readonly ISecretStore _secrets;

	[ObservableProperty]
	private string _text = string.Empty;

	[ObservableProperty]
	private bool _isRevealed;

	public SecretFieldViewModel(
		NodeInspectorViewModel owner,
		ParameterDefinition definition,
		ISecretStore secrets,
		ILocalizer localizer)
		: base(owner, definition, localizer)
	{
		_secrets = secrets;
	}

	/// <summary>The character that hides the text; <c>'\0'</c> (no mask) while the secret is shown.</summary>
	public char PasswordChar => IsRevealed ? '\0' : '•';

	partial void OnIsRevealedChanged(bool value) => OnPropertyChanged(nameof(PasswordChar));

	partial void OnTextChanged(string value)
	{
		if (IsReloading)
			return;

		var reference = CurrentValue.AsString();

		if (string.IsNullOrEmpty(value))
		{
			if (reference.Length > 0)
				_secrets.Remove(reference);

			Commit(ParameterValue.Null);
			return;
		}

		if (reference.Length == 0 || !_secrets.Has(reference))
		{
			// The first character creates the reference and records one undo step; later typing only updates the store.
			reference = _secrets.CreateReference();
			_secrets.Set(reference, value);
			Commit(ParameterValue.FromString(reference));
			return;
		}

		_secrets.Set(reference, value);
	}

	protected override void LoadValue()
	{
		var reference = CurrentValue.AsString();
		Text = reference.Length > 0 ? _secrets.Get(reference) ?? string.Empty : string.Empty;
	}
}
