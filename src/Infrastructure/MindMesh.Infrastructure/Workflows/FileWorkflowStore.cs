using System.Text;
using MindMesh.Application.Workflows;
using MindMesh.Graph.Serialization;

namespace MindMesh.Infrastructure.Workflows;

/// <summary>Stores workflows as <c>.mmflow</c> files on disk.</summary>
public sealed class FileWorkflowStore(WorkflowSerializer serializer) : IWorkflowStore
{
	/// <summary>A workflow file larger than this is refused instead of being read into memory.</summary>
	public const long MaxFileBytes = 64L * 1024 * 1024;

	private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

	public async Task<WorkflowLoadResult> LoadAsync(string path, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);

		var info = new FileInfo(path);

		if (!info.Exists)
		{
			throw new FileNotFoundException("The workflow file does not exist.", path);
		}

		if (info.Length > MaxFileBytes)
		{
			return WorkflowLoadResult.Failure(new WorkflowProblem(
				WorkflowProblemCode.FileTooLarge,
				WorkflowProblemSeverity.Fatal,
				Detail: $"{info.Length} > {MaxFileBytes}"));
		}

		var text = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

		return serializer.Deserialize(text);
	}

	public async Task SaveAsync(string path, WorkflowContent content, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(content);

		var fullPath = Path.GetFullPath(path);
		var directory = Path.GetDirectoryName(fullPath)!;
		Directory.CreateDirectory(directory);

		var text = serializer.Serialize(content);
		var temporary = fullPath + ".tmp";

		try
		{
			await File.WriteAllTextAsync(temporary, text, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

			if (File.Exists(fullPath))
			{
				File.Replace(temporary, fullPath, fullPath + ".bak");
			}
			else
			{
				File.Move(temporary, fullPath);
			}
		}
		finally
		{
			if (File.Exists(temporary))
			{
				File.Delete(temporary);
			}
		}
	}
}
