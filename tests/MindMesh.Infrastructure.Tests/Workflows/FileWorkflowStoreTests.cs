using MindMesh.Graph.Serialization;
using MindMesh.Infrastructure.Tests.Support;
using MindMesh.Infrastructure.Workflows;

namespace MindMesh.Infrastructure.Tests.Workflows;

public class FileWorkflowStoreTests : IDisposable
{
	private readonly TemporaryDirectory _directory = new();
	private readonly FileWorkflowStore _store = new(new WorkflowSerializer(_ => null));

	public void Dispose() => _directory.Dispose();

	private string PathOf(string name) => Path.Combine(_directory.Path, name);

	[Fact]
	public async Task ASavedWorkflow_CanBeLoadedAgain()
	{
		var content = WorkflowContent.CreateEmpty("Round trip");
		var path = PathOf("flow.mmflow");

		await _store.SaveAsync(path, content, TestContext.Current.CancellationToken);
		var result = await _store.LoadAsync(path, TestContext.Current.CancellationToken);

		Assert.True(result.Succeeded);
		Assert.Equal(content.Id, result.Content!.Id);
		Assert.Equal("Round trip", result.Content.Name);
	}

	[Fact]
	public async Task Saving_CreatesMissingFolders()
	{
		var path = PathOf(Path.Combine("projects", "one", "flow.mmflow"));

		await _store.SaveAsync(path, WorkflowContent.CreateEmpty("Nested"), TestContext.Current.CancellationToken);

		Assert.True(File.Exists(path));
	}

	[Fact]
	public async Task TheFile_HasNoByteOrderMark_AndNoLeftoverTemporaryFile()
	{
		var path = PathOf("flow.mmflow");

		await _store.SaveAsync(path, WorkflowContent.CreateEmpty("Clean"), TestContext.Current.CancellationToken);

		var bytes = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
		Assert.Equal((byte)'{', bytes[0]);
		Assert.False(File.Exists(path + ".tmp"));
	}

	[Fact]
	public async Task Overwriting_KeepsThePreviousVersionAsABackup()
	{
		var path = PathOf("flow.mmflow");
		await _store.SaveAsync(path, WorkflowContent.CreateEmpty("First"), TestContext.Current.CancellationToken);

		await _store.SaveAsync(path, WorkflowContent.CreateEmpty("Second"), TestContext.Current.CancellationToken);

		var current = await _store.LoadAsync(path, TestContext.Current.CancellationToken);
		var backup = await _store.LoadAsync(path + ".bak", TestContext.Current.CancellationToken);
		Assert.Equal("Second", current.Content!.Name);
		Assert.Equal("First", backup.Content!.Name);
		Assert.False(File.Exists(path + ".tmp"));
	}

	[Fact]
	public async Task LoadingAMissingFile_Throws()
	{
		await Assert.ThrowsAsync<FileNotFoundException>(
			() => _store.LoadAsync(PathOf("nothing.mmflow"), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task LoadingAFileThatIsNotAWorkflow_GivesAProblemNotAnException()
	{
		var path = PathOf("notes.mmflow");
		await File.WriteAllTextAsync(path, "just some text", TestContext.Current.CancellationToken);

		var result = await _store.LoadAsync(path, TestContext.Current.CancellationToken);

		Assert.False(result.Succeeded);
		Assert.Equal(WorkflowProblemCode.NotJson, Assert.Single(result.Problems).Code);
	}

	[Fact]
	public async Task AFileWithAByteOrderMark_IsReadAnyway()
	{
		var path = PathOf("bom.mmflow");
		var text = new WorkflowSerializer(_ => null).Serialize(WorkflowContent.CreateEmpty("With BOM"));
		await File.WriteAllTextAsync(path, text, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true), TestContext.Current.CancellationToken);

		var result = await _store.LoadAsync(path, TestContext.Current.CancellationToken);

		Assert.Equal("With BOM", result.Content!.Name);
	}

	[Fact]
	public async Task ANodeTheCatalogDoesNotKnow_IsReportedAsLossy()
	{
		var path = PathOf("ghost.mmflow");
		var id = Guid.NewGuid();
		await File.WriteAllTextAsync(
			path,
			"{ \"format\": \"mindmesh.workflow\", \"formatVersion\": 1, \"id\": \"" + id + "\", \"name\": \"Ghost\", "
			+ "\"nodes\": [ { \"id\": \"" + Guid.NewGuid() + "\", \"type\": \"plugin.gone\" } ], \"connections\": [] }",
			TestContext.Current.CancellationToken);

		var result = await _store.LoadAsync(path, TestContext.Current.CancellationToken);

		Assert.True(result.Succeeded);
		Assert.True(result.IsLossy);
		Assert.Empty(result.Content!.Graph.Nodes);
	}
}
