using MindMesh.Graph.Serialization;

namespace MindMesh.Application.Workflows;

/// <summary>Reads and writes workflow files.</summary>
public interface IWorkflowStore
{
	/// <summary>
	/// Reads a workflow file. A file with bad content gives a result with problems rather than an exception;
	/// a file that cannot be opened throws the usual I/O exception.
	/// </summary>
	Task<WorkflowLoadResult> LoadAsync(string path, CancellationToken cancellationToken = default);

	/// <summary>
	/// Writes a workflow so that a failure never leaves a half-written file behind.
	/// The previous version of the file is kept next to it with the extension <c>.bak</c>.
	/// </summary>
	Task SaveAsync(string path, WorkflowContent content, CancellationToken cancellationToken = default);
}
