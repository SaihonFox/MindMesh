using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using MindMesh.Application.Extensions;
using MindMesh.Application.Settings;
using MindMesh.Graph.Validation;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Pages.Editor.Nodes;
using MindMesh.Presentation.Tests.Support;

namespace MindMesh.Presentation.Tests.Localization;

public partial class ResourceCompletenessTests
{
	private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");

	private static readonly ResourceManager Manager = new(
		Localizer.ResourceBaseName,
		typeof(Localizer).Assembly);

	private static Dictionary<string, string> Load(CultureInfo culture, bool tryParents)
	{
		var set = Manager.GetResourceSet(culture, createIfNotExists: true, tryParents: tryParents)
				  ?? throw new InvalidOperationException($"No resources for '{culture.Name}'.");

		var result = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (DictionaryEntry entry in set)
		{
			result[(string)entry.Key] = entry.Value as string ?? string.Empty;
		}

		return result;
	}

	private static Dictionary<string, string> English() => Load(CultureInfo.InvariantCulture, tryParents: true);

	[Fact]
	public void Russian_HasExactlyTheKeysOfTheNeutralResources()
	{
		var english = English().Keys.Order().ToList();
		var russian = Load(Russian, tryParents: false).Keys.Order().ToList();

		Assert.Empty(english.Except(russian));
		Assert.Empty(russian.Except(english));
	}

	[Fact]
	public void NoTextIsEmpty()
	{
		foreach (var (key, value) in English())
		{
			Assert.False(string.IsNullOrWhiteSpace(value), $"English text of '{key}' is empty.");
		}

		foreach (var (key, value) in Load(Russian, tryParents: false))
		{
			Assert.False(string.IsNullOrWhiteSpace(value), $"Russian text of '{key}' is empty.");
		}
	}

	[Fact]
	public void RussianTexts_ContainTheSamePlaceholdersAsEnglish()
	{
		var english = English();
		var russian = Load(Russian, tryParents: false);

		foreach (var (key, englishText) in english)
		{
			var expected = PlaceholderPattern().Matches(englishText).Select(m => m.Value).Order().ToList();
			var actual = PlaceholderPattern().Matches(russian[key]).Select(m => m.Value).Order().ToList();

			Assert.True(expected.SequenceEqual(actual), $"Placeholders of '{key}' differ between languages.");
		}
	}

	[Fact]
	public void EveryKeyUsedInXaml_Exists()
	{
		var keys = English().Keys.ToHashSet(StringComparer.Ordinal);
		var viewsDirectory = Path.Combine(RepositoryPaths.RepositoryRoot, "src", "Desktop", "MindMesh.UI");
		var files = Directory.GetFiles(viewsDirectory, "*.axaml", SearchOption.AllDirectories);

		Assert.NotEmpty(files);

		var used = 0;

		foreach (var file in files)
		{
			foreach (Match match in XamlKeyPattern().Matches(File.ReadAllText(file)))
			{
				used++;
				var key = match.Groups[1].Value;
				Assert.True(keys.Contains(key), $"{Path.GetFileName(file)} uses missing key '{key}'.");
			}
		}

		Assert.True(used > 0, "No Loc[...] usages found: the scan is broken.");
	}

	[Fact]
	public void EveryKeyBuiltInCode_Exists()
	{
		var keys = English().Keys.ToHashSet(StringComparer.Ordinal);

		using var app = new TestApp();
		var catalog = app.Get<IExtensionCatalog>();

		var built = new List<string>();
		built.AddRange(catalog.Categories.Select(category => $"Category_{category.Id}"));
		built.AddRange(catalog.ProjectKinds.SelectMany(kind => kind.Modes).Select(mode => $"Mode_{mode.Id}"));
		built.AddRange(catalog.Nodes.SelectMany(node => new[]
		{
			$"Node_{NodeText.KeyPart(node.Id)}_Name",
			$"Node_{NodeText.KeyPart(node.Id)}_Description",
		}));
		built.AddRange(Enum.GetValues<ThemeMode>().Select(value => $"Theme_{value}"));
		built.AddRange(Enum.GetValues<ValidationCode>().Select(value => $"Validation_{value}"));
		built.AddRange(Enum.GetValues<SaveStatus>().Select(value => $"SaveStatus_{value}"));
		built.AddRange(Enum.GetValues<ConsoleTabId>().Select(value => $"Console_{value}"));
		built.AddRange(Enum.GetValues<ConsoleTabId>().Select(value => $"ConsoleEmpty_{value}"));
		built.AddRange(Enum.GetValues<PageKey>().Select(value => $"Sidebar_{value}")
			.Where(key => key is not ("Sidebar_Welcome" or "Sidebar_Editor" or "Sidebar_About")));
		built.AddRange(["Sidebar_Workflows", "Workspace_UntitledProject", "Workspace_UntitledWorkflow"]);

		foreach (var key in built)
		{
			Assert.True(keys.Contains(key), $"Resource key '{key}' is built in code but missing.");
		}
	}

	[GeneratedRegex(@"\{[0-9]+(?::[^}]*)?\}")]
	private static partial Regex PlaceholderPattern();

	[GeneratedRegex(@"Loc\[([A-Za-z0-9_]+)\]")]
	private static partial Regex XamlKeyPattern();
}
