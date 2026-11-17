using System.Text.Json.Nodes;
using MindMesh.Graph.Serialization;
using MindMesh.Graph.Tests.Support;

namespace MindMesh.Graph.Tests.Serialization;

public class WorkflowMigrationTests
{
	/// <summary>Pretends that version 1 called the workflow name "title".</summary>
	private sealed class RenameTitleToName : IWorkflowMigration
	{
		public int FromVersion => 1;

		public void Migrate(JsonObject root)
		{
			root[WorkflowKeys.Name] = root["title"]?.DeepClone();
			root.Remove("title");
		}
	}

	/// <summary>Pretends that version 2 stored the description in uppercase.</summary>
	private sealed class UppercaseDescription : IWorkflowMigration
	{
		public int FromVersion => 2;

		public void Migrate(JsonObject root) =>
			root[WorkflowKeys.Description] = root[WorkflowKeys.Description]?.GetValue<string>().ToUpperInvariant();
	}

	private sealed class Failing : IWorkflowMigration
	{
		public int FromVersion => 1;

		public void Migrate(JsonObject root) => throw new InvalidOperationException("boom");
	}

	private static WorkflowSerializer Serializer(WorkflowMigrator migrator) =>
		new(_ => TestNodes.Producer, migrator);

	private const string VersionOneFile = """
		{
			"format": "mindmesh.workflow",
			"formatVersion": 1,
			"id": "0192f6a0-0000-7000-8000-000000000001",
			"title": "Old name",
			"description": "old text",
			"nodes": [],
			"connections": []
		}
		""";

	[Fact]
	public void TheDefaultMigrator_EndsAtTheCurrentVersion()
	{
		Assert.Equal(WorkflowFormat.CurrentVersion, WorkflowMigrator.Default.CurrentVersion);
		Assert.Equal(WorkflowFormat.CurrentVersion - 1, WorkflowFormat.Migrations.Select(migration => migration.FromVersion).Distinct().Count());
	}

	[Fact]
	public void AFileOfTheCurrentVersion_IsNotMigrated()
	{
		var serializer = new WorkflowSerializer(_ => null);

		var result = serializer.Deserialize(serializer.Serialize(WorkflowContent.CreateEmpty("Now")));

		Assert.False(result.WasMigrated);
		Assert.Equal(WorkflowFormat.CurrentVersion, result.SourceFormatVersion);
	}

	[Fact]
	public void AnOldFile_IsUpgradedThroughEveryStep()
	{
		var migrator = new WorkflowMigrator(3, [new RenameTitleToName(), new UppercaseDescription()]);

		var result = Serializer(migrator).Deserialize(VersionOneFile);

		Assert.True(result.Succeeded);
		Assert.True(result.WasMigrated);
		Assert.Equal(1, result.SourceFormatVersion);
		Assert.Equal("Old name", result.Content!.Name);
		Assert.Equal("OLD TEXT", result.Content.Description);
	}

	[Fact]
	public void OnlyTheStepsAfterTheFileVersion_Run()
	{
		var migrator = new WorkflowMigrator(3, [new RenameTitleToName(), new UppercaseDescription()]);
		var file = VersionOneFile.Replace("\"formatVersion\": 1", "\"formatVersion\": 2").Replace("\"title\"", "\"name\"");

		var result = Serializer(migrator).Deserialize(file);

		Assert.Equal("Old name", result.Content!.Name);
		Assert.Equal("OLD TEXT", result.Content.Description);
	}

	[Fact]
	public void SavingAfterAMigration_WritesTheCurrentVersion()
	{
		var migrator = new WorkflowMigrator(3, [new RenameTitleToName(), new UppercaseDescription()]);
		var serializer = Serializer(migrator);

		var content = serializer.Deserialize(VersionOneFile).Content!;
		var written = JsonNode.Parse(serializer.Serialize(content))!;

		Assert.Equal(3, written["formatVersion"]!.GetValue<int>());
		Assert.Null(written["title"]);
	}

	[Fact]
	public void AFileFromTheFuture_IsRefused_NotGuessedAt()
	{
		var migrator = new WorkflowMigrator(2, [new RenameTitleToName()]);

		var result = Serializer(migrator).Deserialize(VersionOneFile.Replace("\"formatVersion\": 1", "\"formatVersion\": 3"));

		Assert.False(result.Succeeded);
		Assert.Equal(WorkflowProblemCode.NewerFormat, Assert.Single(result.Problems).Code);
		Assert.Equal(3, result.SourceFormatVersion);
	}

	[Fact]
	public void AMigrationThatThrows_IsReportedAsAFailure()
	{
		var migrator = new WorkflowMigrator(2, [new Failing()]);

		var result = Serializer(migrator).Deserialize(VersionOneFile);

		Assert.False(result.Succeeded);
		var problem = Assert.Single(result.Problems);
		Assert.Equal(WorkflowProblemCode.MigrationFailed, problem.Code);
		Assert.Contains("boom", problem.Detail);
	}

	[Fact]
	public void AChainWithAGap_IsRejected()
	{
		Assert.Throws<ArgumentException>(() => new WorkflowMigrator(3, [new UppercaseDescription()]));
	}

	[Fact]
	public void TwoMigrationsFromOneVersion_AreRejected()
	{
		Assert.Throws<ArgumentException>(() => new WorkflowMigrator(2, [new RenameTitleToName(), new Failing()]));
	}

	[Fact]
	public void AMigrationBeyondTheChain_IsRejected()
	{
		Assert.Throws<ArgumentException>(() => new WorkflowMigrator(2, [new RenameTitleToName(), new UppercaseDescription()]));
	}

	[Fact]
	public void Version1_NeedsNoMigrations()
	{
		var migrator = new WorkflowMigrator(1, []);

		Assert.Equal(1, migrator.CurrentVersion);
		Assert.False(migrator.Migrate(new JsonObject(), 1));
	}
}
