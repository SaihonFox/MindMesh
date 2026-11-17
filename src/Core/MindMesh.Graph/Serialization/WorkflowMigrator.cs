using System.Text.Json.Nodes;

namespace MindMesh.Graph.Serialization;

/// <summary>Runs the chain of migrations that brings an old file up to the current version.</summary>
public sealed class WorkflowMigrator
{
	private readonly Dictionary<int, IWorkflowMigration> _migrations;

	/// <param name="currentVersion">The version the chain ends at.</param>
	/// <param name="migrations">One migration for every version below <paramref name="currentVersion"/>.</param>
	/// <exception cref="ArgumentException">A step of the chain is missing or given twice.</exception>
	public WorkflowMigrator(int currentVersion, IEnumerable<IWorkflowMigration> migrations)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(currentVersion, 1);
		ArgumentNullException.ThrowIfNull(migrations);

		_migrations = [];

		foreach (var migration in migrations)
		{
			if (!_migrations.TryAdd(migration.FromVersion, migration))
			{
				throw new ArgumentException($"Two migrations read version {migration.FromVersion}.", nameof(migrations));
			}
		}

		for (var version = 1; version < currentVersion; version++)
		{
			if (!_migrations.ContainsKey(version))
			{
				throw new ArgumentException($"There is no migration from version {version}.", nameof(migrations));
			}
		}

		if (_migrations.Keys.Any(version => version < 1 || version >= currentVersion))
		{
			throw new ArgumentException("A migration reads a version outside of the chain.", nameof(migrations));
		}

		CurrentVersion = currentVersion;
	}

	/// <summary>The migrator for the format of this build.</summary>
	public static WorkflowMigrator Default { get; } = new(WorkflowFormat.CurrentVersion, WorkflowFormat.Migrations);

	public int CurrentVersion { get; }

	/// <summary>Brings <paramref name="root"/> from <paramref name="fromVersion"/> to the current version.</summary>
	/// <returns><see langword="true"/> when at least one migration ran.</returns>
	public bool Migrate(JsonObject root, int fromVersion)
	{
		ArgumentNullException.ThrowIfNull(root);

		if (fromVersion >= CurrentVersion)
		{
			return false;
		}

		for (var version = fromVersion; version < CurrentVersion; version++)
		{
			_migrations[version].Migrate(root);
			root[WorkflowKeys.FormatVersion] = version + 1;
		}

		return true;
	}
}
