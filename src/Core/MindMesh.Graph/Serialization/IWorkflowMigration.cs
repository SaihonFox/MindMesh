using System.Text.Json.Nodes;

namespace MindMesh.Graph.Serialization;

/// <summary>Upgrades the raw JSON of a workflow file by exactly one format version.</summary>
public interface IWorkflowMigration
{
	/// <summary>The version this migration reads. The result is <c>FromVersion + 1</c>.</summary>
	int FromVersion { get; }

	/// <summary>Rewrites the document in place. The version field is updated by the migrator.</summary>
	void Migrate(JsonObject root);
}
