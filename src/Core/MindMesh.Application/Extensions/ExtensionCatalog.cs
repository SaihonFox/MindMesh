using Microsoft.Extensions.Logging;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Plugins.Abstractions;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Plugins.Abstractions.Projects;

namespace MindMesh.Application.Extensions;

/// <summary>
/// Collects the registrations of all extensions and checks them for consistency.
/// An invalid registration is rejected and reported in <see cref="Problems"/>; it never stops the other extensions.
/// </summary>
public sealed class ExtensionCatalog : IExtensionCatalog
{
	private readonly Dictionary<NodeCategoryId, NodeCategoryDefinition> _categoriesById = [];
	private readonly Dictionary<NodeDefinitionId, INodeDefinition> _nodesById = [];
	private readonly Dictionary<ProjectKindId, ProjectKindDefinition> _kindsById = [];
	private readonly Dictionary<EditorModeId, EditorModeDefinition> _modesById = [];
	private readonly Dictionary<NodeCategoryId, List<INodeDefinition>> _nodesByCategory = [];
	private readonly List<ExtensionInfo> _extensions = [];
	private readonly List<NodeCategoryDefinition> _categories = [];
	private readonly List<INodeDefinition> _nodes = [];
	private readonly List<ProjectKindDefinition> _kinds = [];
	private readonly List<ExtensionProblem> _problems = [];

	public ExtensionCatalog(IEnumerable<IMindMeshExtension> extensions, ILogger<ExtensionCatalog> logger)
	{
		ArgumentNullException.ThrowIfNull(extensions);
		ArgumentNullException.ThrowIfNull(logger);

		var pendingNodes = new List<(string Owner, INodeDefinition Node)>();
		var pendingKinds = new List<(string Owner, ProjectKindDefinition Kind)>();

		foreach (var extension in extensions)
		{
			var info = extension.Info;

			if (_extensions.Exists(known => known.Id == info.Id))
			{
				Reject(logger, info.Id, $"Extension '{info.Id}' is registered twice.");
				continue;
			}

			_extensions.Add(info);

			try
			{
				extension.Register(new Registrar(this, info.Id, pendingNodes, pendingKinds, logger));
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "Extension {ExtensionId} failed while registering.", info.Id);
				_problems.Add(new ExtensionProblem(info.Id, $"Registration failed: {exception.Message}"));
			}
		}

		// Nodes and kinds are checked after all categories are known, so registration order does not matter.
		foreach (var (owner, node) in pendingNodes)
		{
			AcceptNode(logger, owner, node);
		}

		foreach (var (owner, kind) in pendingKinds)
		{
			AcceptKind(logger, owner, kind);
		}
	}

	public IReadOnlyList<ExtensionInfo> Extensions => _extensions;

	public IReadOnlyList<ProjectKindDefinition> ProjectKinds => _kinds;

	public IReadOnlyList<NodeCategoryDefinition> Categories => _categories;

	public IReadOnlyList<INodeDefinition> Nodes => _nodes;

	public IReadOnlyList<ExtensionProblem> Problems => _problems;

	public ProjectKindDefinition? FindProjectKind(ProjectKindId id) => _kindsById.GetValueOrDefault(id);

	public EditorModeDefinition? FindMode(EditorModeId id) => _modesById.GetValueOrDefault(id);

	public NodeCategoryDefinition? FindCategory(NodeCategoryId id) => _categoriesById.GetValueOrDefault(id);

	public INodeDefinition? FindNode(NodeDefinitionId id) => _nodesById.GetValueOrDefault(id);

	public IReadOnlyList<INodeDefinition> GetNodes(NodeCategoryId category) =>
		_nodesByCategory.TryGetValue(category, out var nodes) ? nodes : [];

	private void Reject(ILogger logger, string owner, string message)
	{
		logger.LogWarning("Extension {ExtensionId} registration rejected: {Message}", owner, message);
		_problems.Add(new ExtensionProblem(owner, message));
	}

	private void AcceptCategory(ILogger logger, string owner, NodeCategoryDefinition category)
	{
		if (category.Id.IsEmpty)
		{
			Reject(logger, owner, "A category has no identifier.");
		}
		else if (!_categoriesById.TryAdd(category.Id, category))
		{
			Reject(logger, owner, $"Category '{category.Id}' already exists.");
		}
		else
		{
			_categories.Add(category);
			_nodesByCategory[category.Id] = [];
		}
	}

	private void AcceptNode(ILogger logger, string owner, INodeDefinition node)
	{
		if (node.Id.IsEmpty)
		{
			Reject(logger, owner, "A node has no identifier.");
			return;
		}

		if (node.Categories.Count == 0)
		{
			Reject(logger, owner, $"Node '{node.Id}' has no category.");
			return;
		}

		var unknown = node.Categories.FirstOrDefault(category => !_categoriesById.ContainsKey(category));

		if (!unknown.IsEmpty)
		{
			Reject(logger, owner, $"Node '{node.Id}' uses unknown category '{unknown}'.");
			return;
		}

		if (!_nodesById.TryAdd(node.Id, node))
		{
			Reject(logger, owner, $"Node '{node.Id}' already exists.");
			return;
		}

		_nodes.Add(node);

		foreach (var category in node.Categories.Distinct())
		{
			_nodesByCategory[category].Add(node);
		}
	}

	private void AcceptKind(ILogger logger, string owner, ProjectKindDefinition kind)
	{
		if (kind.Id.IsEmpty)
		{
			Reject(logger, owner, "A project kind has no identifier.");
			return;
		}

		if (kind.Modes.Count == 0)
		{
			Reject(logger, owner, $"Project kind '{kind.Id}' has no editor modes.");
			return;
		}

		if (_kindsById.ContainsKey(kind.Id))
		{
			Reject(logger, owner, $"Project kind '{kind.Id}' already exists.");
			return;
		}

		foreach (var mode in kind.Modes)
		{
			if (mode.Id.IsEmpty || _modesById.ContainsKey(mode.Id) || kind.Modes.Count(other => other.Id == mode.Id) > 1)
			{
				Reject(logger, owner, $"Project kind '{kind.Id}' has an empty or duplicate editor mode '{mode.Id}'.");
				return;
			}

			var unknown = mode.Categories.FirstOrDefault(category => !_categoriesById.ContainsKey(category));

			if (!unknown.IsEmpty)
			{
				Reject(logger, owner, $"Mode '{mode.Id}' uses unknown category '{unknown}'.");
				return;
			}
		}

		_kindsById[kind.Id] = kind;
		_kinds.Add(kind);

		foreach (var mode in kind.Modes)
		{
			_modesById[mode.Id] = mode;
		}
	}

	private sealed class Registrar(
		ExtensionCatalog catalog,
		string owner,
		List<(string Owner, INodeDefinition Node)> nodes,
		List<(string Owner, ProjectKindDefinition Kind)> kinds,
		ILogger logger) : IExtensionRegistrar
	{
		public void AddNodeCategory(NodeCategoryDefinition category)
		{
			ArgumentNullException.ThrowIfNull(category);
			catalog.AcceptCategory(logger, owner, category);
		}

		public void AddNode(INodeDefinition node)
		{
			ArgumentNullException.ThrowIfNull(node);
			nodes.Add((owner, node));
		}

		public void AddProjectKind(ProjectKindDefinition kind)
		{
			ArgumentNullException.ThrowIfNull(kind);
			kinds.Add((owner, kind));
		}
	}
}
