namespace MindMesh.Graph.Serialization;

/// <summary>Constants of the workflow file format.</summary>
public static class WorkflowFormat
{
	/// <summary>Value of the "format" field; tells a workflow file from any other JSON.</summary>
	public const string Name = "mindmesh.workflow";

	/// <summary>The newest layout of the file that this build writes and understands.</summary>
	public const int CurrentVersion = 1;

	/// <summary>Conventional extension of workflow files.</summary>
	public const string FileExtension = ".mmflow";

	/// <summary>
	/// Upgrades from older layouts. Empty while the file layout is at version 1;
	/// the migration from version N to N + 1 is added here together with the change that needs it.
	/// </summary>
	public static IReadOnlyList<IWorkflowMigration> Migrations { get; } = [];
}
