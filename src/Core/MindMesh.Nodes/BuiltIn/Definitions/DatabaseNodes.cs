using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

internal static class DatabaseNodes
{
	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("db.postgres", "PostgreSQL Query", BuiltInCategories.Database)
			.Describe("Runs an SQL statement on a PostgreSQL database.")
			.Icon("database")
			.Keywords("sql", "postgres", "select", "insert", "update")
			.In("in", "In", PortType.Execution)
			.In("parameters", "Parameters", PortType.Array, description: "Values for $1, $2, …")
			.Out("out", "Out", PortType.Execution)
			.Out("rows", "Rows", PortType.Array)
			.Out("rowCount", "Row count", PortType.Integer)
			.Param(Param.Secret("connection", "Connection string", required: true, description: "Stored separately from the workflow."))
			.Param(Param.Code("query", "Query", "sql", "SELECT 1;", required: true))
			.Param(Param.Number("timeout", "Timeout (ms)", 30000, minimum: 1, maximum: 600000))
			.Build();

		yield return NodeDefinition.Create("db.sqlite", "SQLite Query", BuiltInCategories.Database)
			.Describe("Runs an SQL statement on a SQLite database file.")
			.Icon("database")
			.Keywords("sql", "file", "local")
			.In("in", "In", PortType.Execution)
			.In("parameters", "Parameters", PortType.Array)
			.Out("out", "Out", PortType.Execution)
			.Out("rows", "Rows", PortType.Array)
			.Out("rowCount", "Row count", PortType.Integer)
			.Param(Param.File("database", "Database file", required: true))
			.Param(Param.Code("query", "Query", "sql", "SELECT 1;", required: true))
			.Build();

		yield return NodeDefinition.Create("db.redis", "Redis Command", BuiltInCategories.Database)
			.Describe("Runs a command on a Redis server.")
			.Icon("database")
			.Keywords("cache", "key", "value", "queue")
			.In("in", "In", PortType.Execution)
			.Out("out", "Out", PortType.Execution)
			.Out("result", "Result", PortType.Any)
			.Param(Param.Secret("connection", "Connection string", required: true, description: "Stored separately from the workflow."))
			.Param(Param.Select("command", "Command", ["GET", "SET", "DEL", "INCR", "EXPIRE", "HGET", "HSET", "LPUSH", "RPOP"], "GET"))
			.Param(Param.Text("key", "Key", required: true))
			.Param(Param.Expression("value", "Value").When("command", "SET", "HSET", "LPUSH"))
			.Param(Param.Number("ttl", "Time to live (s)", minimum: 0, maximum: 31536000).When("command", "SET", "EXPIRE"))
			.Build();
	}
}
