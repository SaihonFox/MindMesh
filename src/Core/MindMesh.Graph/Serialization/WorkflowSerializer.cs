using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Serialization;

/// <summary>
/// Writes a workflow to JSON text and reads it back. The output is deterministic: the same workflow
/// always produces the same text, so a file diffs cleanly in version control.
/// Reading never throws on bad content; everything it could not use is reported as a problem.
/// </summary>
public sealed class WorkflowSerializer
{
	private static readonly JsonWriterOptions WriterOptions = new()
	{
		Indented = true,
		IndentCharacter = '\t',
		IndentSize = 1,
		NewLine = "\n",
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	private static readonly JsonDocumentOptions ReaderOptions = new()
	{
		CommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
	};

	private readonly Func<NodeDefinitionId, INodeDefinition?> _findDefinition;
	private readonly WorkflowMigrator _migrator;

	/// <param name="findDefinition">Resolves a node type of the file to its definition; <see langword="null"/> when it is unknown.</param>
	/// <param name="migrator">Upgrades older files; the migrator of this build when omitted.</param>
	public WorkflowSerializer(Func<NodeDefinitionId, INodeDefinition?> findDefinition, WorkflowMigrator? migrator = null)
	{
		ArgumentNullException.ThrowIfNull(findDefinition);

		_findDefinition = findDefinition;
		_migrator = migrator ?? WorkflowMigrator.Default;
	}

	public string Serialize(WorkflowContent content)
	{
		ArgumentNullException.ThrowIfNull(content);

		using var stream = new MemoryStream();

		using (var writer = new Utf8JsonWriter(stream, WriterOptions))
		{
			writer.WriteStartObject();
			writer.WriteString(WorkflowKeys.Format, WorkflowFormat.Name);
			writer.WriteNumber(WorkflowKeys.FormatVersion, _migrator.CurrentVersion);
			writer.WriteString(WorkflowKeys.Id, content.Id.ToString("D"));
			writer.WriteString(WorkflowKeys.Name, content.Name);

			if (content.Description is null)
			{
				writer.WriteNull(WorkflowKeys.Description);
			}
			else
			{
				writer.WriteString(WorkflowKeys.Description, content.Description);
			}

			writer.WriteStartArray(WorkflowKeys.Nodes);

			foreach (var node in content.Graph.Nodes)
			{
				WriteNode(writer, node, content.Layout);
			}

			writer.WriteEndArray();
			writer.WriteStartArray(WorkflowKeys.Connections);

			foreach (var connection in content.Graph.Connections)
			{
				writer.WriteStartObject();
				writer.WriteString(WorkflowKeys.Id, connection.Id.ToString());
				WritePort(writer, WorkflowKeys.From, connection.Source);
				WritePort(writer, WorkflowKeys.To, connection.Target);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		return Encoding.UTF8.GetString(stream.ToArray()) + "\n";
	}

	public WorkflowLoadResult Deserialize(string text)
	{
		ArgumentNullException.ThrowIfNull(text);

		JsonNode? parsed;

		try
		{
			parsed = JsonNode.Parse(text, documentOptions: ReaderOptions);
		}
		catch (JsonException exception)
		{
			return WorkflowLoadResult.Failure(Fatal(WorkflowProblemCode.NotJson, detail: exception.Message));
		}

		if (parsed is not JsonObject root || GetString(root, WorkflowKeys.Format) != WorkflowFormat.Name)
		{
			return WorkflowLoadResult.Failure(Fatal(WorkflowProblemCode.NotAWorkflow));
		}

		if (root[WorkflowKeys.FormatVersion] is not JsonValue versionValue
			|| !versionValue.TryGetValue<int>(out var version)
			|| version < 1)
		{
			return WorkflowLoadResult.Failure(Fatal(WorkflowProblemCode.MissingFormatVersion, WorkflowKeys.FormatVersion));
		}

		if (version > _migrator.CurrentVersion)
		{
			return WorkflowLoadResult.Failure(
				Fatal(WorkflowProblemCode.NewerFormat, detail: $"{version} > {_migrator.CurrentVersion}"),
				version);
		}

		bool migrated;

		try
		{
			migrated = _migrator.Migrate(root, version);
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			return WorkflowLoadResult.Failure(Fatal(WorkflowProblemCode.MigrationFailed, detail: exception.Message), version);
		}

		var problems = new List<WorkflowProblem>();
		var graph = new NodeGraph();
		var layout = new GraphLayout();

		var id = Guid.Empty;

		if (!Guid.TryParse(GetString(root, WorkflowKeys.Id), out id) || id == Guid.Empty)
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.InvalidWorkflowId, WorkflowProblemSeverity.Warning, WorkflowKeys.Id));
			id = Guid.CreateVersion7();
		}

		var skippedNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (root[WorkflowKeys.Nodes] is JsonArray nodes)
		{
			foreach (var item in nodes)
			{
				ReadNode(item as JsonObject, graph, layout, skippedNodes, problems);
			}
		}

		if (root[WorkflowKeys.Connections] is JsonArray connections)
		{
			var seen = new HashSet<ConnectionId>();

			foreach (var item in connections)
			{
				ReadConnection(item as JsonObject, graph, seen, skippedNodes, problems);
			}
		}

		var content = new WorkflowContent(
			id,
			GetString(root, WorkflowKeys.Name) ?? string.Empty,
			GetString(root, WorkflowKeys.Description),
			graph,
			layout);

		return new WorkflowLoadResult(content, problems, version, migrated);
	}

	private static void WriteNode(Utf8JsonWriter writer, NodeInstance node, GraphLayout layout)
	{
		var placement = layout.GetPlacement(node.Id);

		writer.WriteStartObject();
		writer.WriteString(WorkflowKeys.Id, node.Id.ToString());
		writer.WriteString(WorkflowKeys.NodeType, node.Definition.Id.Value);
		writer.WriteString(WorkflowKeys.NodeTypeVersion, node.Definition.Version.ToString());

		if (node.Title is not null)
		{
			writer.WriteString(WorkflowKeys.NodeTitle, node.Title);
		}

		if (node.IsDisabled)
		{
			writer.WriteBoolean(WorkflowKeys.NodeDisabled, true);
		}

		writer.WriteNumber(WorkflowKeys.NodeX, placement.X);
		writer.WriteNumber(WorkflowKeys.NodeY, placement.Y);

		if (layout.IsCollapsed(node.Id))
		{
			writer.WriteBoolean(WorkflowKeys.NodeCollapsed, true);
		}

		if (node.Parameters.Count > 0)
		{
			writer.WriteStartObject(WorkflowKeys.NodeParameters);

			foreach (var (key, value) in node.Parameters.OrderBy(pair => pair.Key, StringComparer.Ordinal))
			{
				WriteParameter(writer, key, value);
			}

			writer.WriteEndObject();
		}

		writer.WriteEndObject();
	}

	private static void WriteParameter(Utf8JsonWriter writer, string key, ParameterValue value)
	{
		writer.WritePropertyName(key);

		if (IsValidJson(value.Json))
		{
			writer.WriteRawValue(value.Json);
		}
		else
		{
			// Text that is not JSON is kept as a string rather than breaking the file.
			writer.WriteStringValue(value.Json);
		}
	}

	private static bool IsValidJson(string json)
	{
		try
		{
			using var document = JsonDocument.Parse(json);

			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	private static void WritePort(Utf8JsonWriter writer, string name, PortReference port)
	{
		writer.WriteStartObject(name);
		writer.WriteString(WorkflowKeys.Node, port.Node.ToString());
		writer.WriteString(WorkflowKeys.Port, port.PortKey);
		writer.WriteEndObject();
	}

	private void ReadNode(
		JsonObject? item,
		NodeGraph graph,
		GraphLayout layout,
		HashSet<string> skippedNodes,
		List<WorkflowProblem> problems)
	{
		var rawId = item is null ? null : GetString(item, WorkflowKeys.Id);

		if (item is null || !Guid.TryParse(rawId, out var guid) || guid == Guid.Empty)
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.InvalidNodeId, WorkflowProblemSeverity.Error, rawId));

			return;
		}

		var nodeId = new NodeId(guid);

		if (graph.FindNode(nodeId) is not null)
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.DuplicateNodeId, WorkflowProblemSeverity.Error, rawId));

			return;
		}

		var type = GetString(item, WorkflowKeys.NodeType) ?? string.Empty;

		if (_findDefinition(new NodeDefinitionId(type)) is not { } definition)
		{
			skippedNodes.Add(nodeId.ToString());
			problems.Add(new WorkflowProblem(WorkflowProblemCode.UnknownNodeType, WorkflowProblemSeverity.Error, rawId, type));

			return;
		}

		if (Version.TryParse(GetString(item, WorkflowKeys.NodeTypeVersion), out var fileVersion) && fileVersion > definition.Version)
		{
			problems.Add(new WorkflowProblem(
				WorkflowProblemCode.NodeVersionNewer,
				WorkflowProblemSeverity.Warning,
				rawId,
				$"{type} {fileVersion} > {definition.Version}"));
		}

		var parameters = new List<KeyValuePair<string, ParameterValue>>();

		if (item[WorkflowKeys.NodeParameters] is JsonObject parameterObject)
		{
			foreach (var (key, value) in parameterObject)
			{
				if (value is null)
				{
					continue;
				}

				if (definition.Parameters.All(parameter => parameter.Key != key))
				{
					problems.Add(new WorkflowProblem(WorkflowProblemCode.UnknownParameter, WorkflowProblemSeverity.Warning, rawId, key));
				}

				parameters.Add(new KeyValuePair<string, ParameterValue>(key, new ParameterValue(value.ToJsonString())));
			}
		}

		var node = new NodeInstance(
			nodeId,
			definition,
			GetString(item, WorkflowKeys.NodeTitle),
			GetBoolean(item, WorkflowKeys.NodeDisabled),
			parameters);

		graph.AddNode(node);
		layout.SetPlacement(nodeId, new NodePlacement(GetDouble(item, WorkflowKeys.NodeX), GetDouble(item, WorkflowKeys.NodeY)));

		if (GetBoolean(item, WorkflowKeys.NodeCollapsed))
		{
			layout.SetCollapsed(nodeId, true);
		}
	}

	private static void ReadConnection(
		JsonObject? item,
		NodeGraph graph,
		HashSet<ConnectionId> seen,
		HashSet<string> skippedNodes,
		List<WorkflowProblem> problems)
	{
		var rawId = item is null ? null : GetString(item, WorkflowKeys.Id);

		if (item is null
			|| !Guid.TryParse(rawId, out var guid)
			|| !TryReadPort(item[WorkflowKeys.From], out var source)
			|| !TryReadPort(item[WorkflowKeys.To], out var target))
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.InvalidConnection, WorkflowProblemSeverity.Error, rawId));

			return;
		}

		var id = new ConnectionId(guid);

		if (!seen.Add(id))
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.DuplicateConnectionId, WorkflowProblemSeverity.Error, rawId));

			return;
		}

		if (skippedNodes.Contains(source.Node.ToString()) || skippedNodes.Contains(target.Node.ToString()))
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.ConnectionDropped, WorkflowProblemSeverity.Error, rawId));

			return;
		}

		// The file stores the output as "from" and the input as "to"; anything else is a damaged file.
		var sourceIsOutput = graph.FindNode(source.Node)?.Definition.FindPort(source.PortKey, PortDirection.Output) is not null;
		var targetIsInput = graph.FindNode(target.Node)?.Definition.FindPort(target.PortKey, PortDirection.Input) is not null;

		if (!sourceIsOutput || !targetIsInput)
		{
			problems.Add(new WorkflowProblem(
				WorkflowProblemCode.ConnectionRejected,
				WorkflowProblemSeverity.Error,
				rawId,
				graph.FindNode(source.Node) is null || graph.FindNode(target.Node) is null
					? nameof(ConnectionRejection.UnknownPort)
					: nameof(ConnectionRejection.WrongDirection)));

			return;
		}

		var check = graph.CheckConnection(source, target);

		if (!check.IsAllowed)
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.ConnectionRejected, WorkflowProblemSeverity.Error, rawId, check.Rejection.ToString()));

			return;
		}

		// Reading must not silently pick a winner between two connections into one single-connection port.
		if (check.ReplacedConnections.Count > 0)
		{
			problems.Add(new WorkflowProblem(WorkflowProblemCode.ConnectionRejected, WorkflowProblemSeverity.Error, rawId, "PortOccupied"));

			return;
		}

		graph.AddConnection(new Connection(id, source, target));
	}

	private static bool TryReadPort(JsonNode? node, out PortReference port)
	{
		port = default;

		if (node is not JsonObject obj
			|| !Guid.TryParse(GetString(obj, WorkflowKeys.Node), out var guid)
			|| GetString(obj, WorkflowKeys.Port) is not { Length: > 0 } key)
		{
			return false;
		}

		port = new PortReference(new NodeId(guid), key);

		return true;
	}

	private static WorkflowProblem Fatal(WorkflowProblemCode code, string? subject = null, string? detail = null) =>
		new(code, WorkflowProblemSeverity.Fatal, subject, detail);

	private static string? GetString(JsonObject obj, string name) =>
		obj[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

	private static bool GetBoolean(JsonObject obj, string name) =>
		obj[name] is JsonValue value && value.TryGetValue<bool>(out var flag) && flag;

	private static double GetDouble(JsonObject obj, string name) =>
		obj[name] is JsonValue value && value.TryGetValue<double>(out var number) && double.IsFinite(number) ? number : 0;
}
