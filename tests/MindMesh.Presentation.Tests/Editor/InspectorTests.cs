using MindMesh.Application.Secrets;
using MindMesh.Graph.Definitions;
using MindMesh.Presentation.Pages.Editor.Inspector;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Editor;

public class InspectorTests
{
	private static string NodeWith(EditorFixture fixture, Func<ParameterDefinition, bool> predicate) =>
		fixture.Catalog.Nodes.First(node => node.Parameters.Any(predicate)).Id.Value;

	[Fact]
	public void TheInspector_FollowsTheSelection()
	{
		using var fixture = new EditorFixture();
		var inspector = fixture.App.Get<InspectorViewModel>();

		Assert.True(inspector.IsEmpty);
		Assert.Null(inspector.Node);

		var first = fixture.Add("http.request", 100, 100);
		Assert.True(inspector.HasNode);
		Assert.False(inspector.IsEmpty);
		Assert.Equal(first.Definition.Parameters.Count, inspector.Node!.Fields.Count);

		var second = fixture.Add("logic.if", 500, 100);
		fixture.Editor.SelectNode(first, additive: true);

		Assert.True(inspector.IsMultipleSelection);
		Assert.Null(inspector.Node);
		Assert.Contains("2", inspector.MultipleSelectionText);

		fixture.Editor.ClearSelection();
		Assert.True(inspector.IsEmpty);
		Assert.NotNull(second);
	}

	[Fact]
	public void EditingATextSetting_WritesItToTheNode_AndCanBeUndone()
	{
		using var fixture = new EditorFixture();
		var inspector = fixture.App.Get<InspectorViewModel>();
		var node = fixture.Add("http.request", 100, 100);
		var field = Assert.IsType<TextFieldViewModel>(inspector.Node!.Fields.First(item => item is TextFieldViewModel));

		field.Text = "https://example.com";

		Assert.Equal("https://example.com", node.Node.GetParameter(field.Key).AsString());

		fixture.Editor.UndoCommand.Execute(null);

		Assert.NotEqual("https://example.com", node.Node.GetParameter(field.Key).AsString());
	}

	[Fact]
	public void TypingSeveralCharacters_IsOneUndoStep()
	{
		using var fixture = new EditorFixture();
		var inspector = fixture.App.Get<InspectorViewModel>();
		var node = fixture.Add("http.request", 100, 100);
		var field = Assert.IsType<TextFieldViewModel>(inspector.Node!.Fields.First(item => item is TextFieldViewModel));
		var before = node.Node.GetParameter(field.Key);

		field.Text = "h";
		field.Text = "ht";
		field.Text = "htt";
		fixture.Editor.UndoCommand.Execute(null);

		Assert.Equal(before, node.Node.GetParameter(field.Key));
	}

	[Fact]
	public void ANumberOutsideItsRange_ShowsAnErrorNextToTheField()
	{
		using var fixture = new EditorFixture();
		var id = NodeWith(fixture, parameter => parameter.Type == ParameterType.Number && parameter.Maximum is not null);
		var inspector = fixture.App.Get<InspectorViewModel>();
		var node = fixture.Add(id, 100, 100);
		var field = inspector.Node!.Fields.OfType<NumberFieldViewModel>().First(item => item.Definition.Maximum is not null);

		field.Text = (field.Definition.Maximum!.Value + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

		Assert.True(field.HasError);
		Assert.Contains(field.Definition.Maximum.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), field.ErrorMessage);
		Assert.NotNull(node);

		field.Text = field.Definition.Maximum.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

		Assert.False(field.HasError);
	}

	[Fact]
	public void ATextThatIsNotANumber_IsReportedAsInvalid()
	{
		using var fixture = new EditorFixture();
		var id = NodeWith(fixture, parameter => parameter.Type == ParameterType.Number);
		var inspector = fixture.App.Get<InspectorViewModel>();
		fixture.Add(id, 100, 100);
		var field = inspector.Node!.Fields.OfType<NumberFieldViewModel>().First();

		field.Text = "abc";

		Assert.True(field.HasError);
	}

	[Fact]
	public void ASecret_IsKeptInTheSecretStore_AndTheGraphHoldsOnlyAReference()
	{
		using var fixture = new EditorFixture();
		var id = NodeWith(fixture, parameter => parameter.Type == ParameterType.Secret);
		var inspector = fixture.App.Get<InspectorViewModel>();
		var store = fixture.App.Get<ISecretStore>();
		var node = fixture.Add(id, 100, 100);
		var field = inspector.Node!.Fields.OfType<SecretFieldViewModel>().First();

		field.Text = "hunter2";

		var stored = node.Node.GetParameter(field.Key).AsString();
		Assert.DoesNotContain("hunter2", stored, StringComparison.Ordinal);
		Assert.StartsWith("secret:", stored, StringComparison.Ordinal);
		Assert.Equal("hunter2", store.Get(stored));
	}

	[Fact]
	public void ASecret_IsMaskedUntilRevealed()
	{
		using var fixture = new EditorFixture();
		var id = NodeWith(fixture, parameter => parameter.Type == ParameterType.Secret);
		var inspector = fixture.App.Get<InspectorViewModel>();
		fixture.Add(id, 100, 100);
		var field = inspector.Node!.Fields.OfType<SecretFieldViewModel>().First();

		Assert.NotEqual('\0', field.PasswordChar);

		field.IsRevealed = true;

		Assert.Equal('\0', field.PasswordChar);
	}

	[Fact]
	public void CopiedNodes_DoNotCarryTheSecret()
	{
		using var fixture = new EditorFixture();
		var id = NodeWith(fixture, parameter => parameter.Type == ParameterType.Secret);
		var inspector = fixture.App.Get<InspectorViewModel>();
		var node = fixture.Add(id, 100, 100);
		var field = inspector.Node!.Fields.OfType<SecretFieldViewModel>().First();
		field.Text = "hunter2";

		fixture.Editor.Duplicate();

		var copy = Assert.Single(fixture.Editor.SelectedNodes);
		Assert.NotSame(node, copy);
		Assert.True(copy.Node.GetParameter(field.Key).IsNull);
	}

	[Fact]
	public void ASettingThatDependsOnAnother_AppearsOnlyWhenItsConditionHolds()
	{
		using var fixture = new EditorFixture();
		var id = fixture.Catalog.Nodes
			.First(node => node.Parameters.Any(parameter => parameter.VisibleWhen is not null
				&& node.Parameters.Any(driver => driver.Key == parameter.VisibleWhen.Key && driver.Type == ParameterType.Select)))
			.Id.Value;
		var definition = fixture.Definition(id);
		var dependent = definition.Parameters.First(parameter => parameter.VisibleWhen is not null);
		var condition = dependent.VisibleWhen!;
		var driver = definition.Parameters.First(parameter => parameter.Key == condition.Key);
		var inspector = fixture.App.Get<InspectorViewModel>();
		fixture.Add(id, 100, 100);
		var driverField = Assert.IsType<SelectFieldViewModel>(inspector.Node!.Fields.First(item => item.Key == driver.Key));
		var dependentField = inspector.Node.Fields.First(item => item.Key == dependent.Key);

		driverField.SelectedOption = driver.Options.First(option => condition.Values.Contains(option.Value));
		Assert.True(dependentField.IsVisible);

		driverField.SelectedOption = driver.Options.First(option => !condition.Values.Contains(option.Value));
		Assert.False(dependentField.IsVisible);
	}

	[Fact]
	public void KeyValueSettings_AddAndRemoveRows()
	{
		using var fixture = new EditorFixture();
		var id = NodeWith(fixture, parameter => parameter.Type == ParameterType.DynamicFields);
		var inspector = fixture.App.Get<InspectorViewModel>();
		var node = fixture.Add(id, 100, 100);
		var field = inspector.Node!.Fields.OfType<DynamicFieldsViewModel>().First();

		field.AddRowCommand.Execute(null);
		var row = Assert.Single(field.Rows);
		row.Key = "Accept";
		row.Value = "json";

		var pair = Assert.Single(node.Node.GetParameter(field.Key).AsPairs());
		Assert.Equal("Accept", pair.Key);
		Assert.Equal("json", pair.Value);

		row.RemoveCommand.Execute(null);

		Assert.Empty(field.Rows);
		Assert.Empty(node.Node.GetParameter(field.Key).AsPairs());
	}

	[Fact]
	public void TheTitleAndTheDisabledSwitch_GoThroughTheDocument()
	{
		using var fixture = new EditorFixture();
		var inspector = fixture.App.Get<InspectorViewModel>();
		var node = fixture.Add("logic.if", 100, 100);

		inspector.Node!.Title = "Is it big?";
		inspector.Node.IsDisabled = true;

		Assert.Equal("Is it big?", node.Title);
		Assert.True(node.IsDisabled);

		fixture.Editor.UndoCommand.Execute(null);
		Assert.False(node.IsDisabled);

		fixture.Editor.UndoCommand.Execute(null);
		Assert.NotEqual("Is it big?", node.Title);
	}

	[Fact]
	public void ChangesMadeOnTheCanvas_ReachTheOpenInspector()
	{
		using var fixture = new EditorFixture();
		var inspector = fixture.App.Get<InspectorViewModel>();
		var node = fixture.Add("logic.if", 100, 100);

		fixture.Editor.ToggleDisabled();

		Assert.True(inspector.Node!.IsDisabled);
		Assert.NotNull(node);
	}

	[Fact]
	public void ValidationMessages_AreWrittenInTheCurrentLanguage()
	{
		using var fixture = new EditorFixture(new Application.Settings.AppSettings { Language = "ru" });
		var id = NodeWith(fixture, parameter => parameter.Type == ParameterType.Number && parameter.Maximum is not null);
		var inspector = fixture.App.Get<InspectorViewModel>();
		fixture.Add(id, 100, 100);
		var field = inspector.Node!.Fields.OfType<NumberFieldViewModel>().First(item => item.Definition.Maximum is not null);

		field.Text = (field.Definition.Maximum!.Value + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

		Assert.Contains("не может быть больше", field.ErrorMessage);
	}
}
