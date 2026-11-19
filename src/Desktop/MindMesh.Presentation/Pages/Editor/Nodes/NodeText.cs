using MindMesh.Application.Extensions;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Plugins.Abstractions.Projects;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor.Nodes;

/// <summary>
/// Display text and appearance of nodes, categories and editor modes. Built-in content has translations in the
/// resource files; content from extensions falls back to the names the extension supplied.
/// </summary>
public sealed class NodeText(ILocalizer localizer, IExtensionCatalog catalog)
{
	/// <summary>Resource key part for a node: <c>logic.try-catch</c> becomes <c>logic_try_catch</c>.</summary>
	public static string KeyPart(NodeDefinitionId id) => id.Value.Replace('.', '_').Replace('-', '_');

	public string GetName(INodeDefinition definition) =>
		localizer.TryGet($"Node_{KeyPart(definition.Id)}_Name", out var text) ? text : definition.Name;

	public string GetDescription(INodeDefinition definition) =>
		localizer.TryGet($"Node_{KeyPart(definition.Id)}_Description", out var text) ? text : definition.Description;

	public string GetCategoryTitle(NodeCategoryId id)
	{
		if (localizer.TryGet($"Category_{id}", out var text))
			return text;

		return catalog.FindCategory(id)?.Name ?? id.Value;
	}

	public string GetModeTitle(EditorModeDefinition mode) =>
		localizer.TryGet($"Mode_{mode.Id}", out var text) ? text : mode.Name;

	/// <summary>The category that decides the color and icon of a node: the first one it lists.</summary>
	public NodeCategoryDefinition? GetPrimaryCategory(INodeDefinition definition) =>
		definition.Categories.Count == 0 ? null : catalog.FindCategory(definition.Categories[0]);

	public AccentTone GetTone(INodeDefinition definition) => GetPrimaryCategory(definition)?.Tone ?? AccentTone.Gray;

	public string GetPrimaryCategoryTitle(INodeDefinition definition) =>
		definition.Categories.Count == 0 ? string.Empty : GetCategoryTitle(definition.Categories[0]);

	/// <summary>
	/// True if the query appears in the localized name or description, the English name, the keywords or the identifier.
	/// An empty query matches everything.
	/// </summary>
	public bool Matches(INodeDefinition definition, string query)
	{
		var text = query.Trim();

		if (text.Length == 0)
			return true;

		var comparison = StringComparison.CurrentCultureIgnoreCase;

		return GetName(definition).Contains(text, comparison)
			|| GetDescription(definition).Contains(text, comparison)
			|| definition.Name.Contains(text, comparison)
			|| definition.Id.Value.Contains(text, comparison)
			|| definition.Keywords.Any(keyword => keyword.Contains(text, comparison));
	}

	/// <summary>
	/// Ranks a match for sorting: names that start with the query come first, then names that contain it, then the rest.
	/// </summary>
	public int Rank(INodeDefinition definition, string query)
	{
		var text = query.Trim();

		if (text.Length == 0)
			return 0;

		var comparison = StringComparison.CurrentCultureIgnoreCase;
		var name = GetName(definition);

		if (name.StartsWith(text, comparison) || definition.Name.StartsWith(text, comparison))
			return 0;

		return name.Contains(text, comparison) || definition.Name.Contains(text, comparison) ? 1 : 2;
	}
}
