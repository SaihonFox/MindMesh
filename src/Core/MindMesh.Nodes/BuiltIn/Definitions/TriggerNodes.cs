using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

internal static class TriggerNodes
{
	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("trigger.manual", "Manual Trigger", BuiltInCategories.Triggers)
			.Describe("Starts the workflow when you press Run.")
			.Icon("play")
			.Keywords("start", "run", "button")
			.Out("start", "Started", PortType.Execution)
			.Out("payload", "Payload", PortType.Object, description: "The test payload below.")
			.Param(Param.Json("payload", "Test payload", "{}"))
			.Build();

		yield return NodeDefinition.Create("trigger.webhook", "Webhook Trigger", BuiltInCategories.Triggers)
			.Describe("Starts the workflow when an HTTP request arrives.")
			.Icon("webhook")
			.Keywords("http", "endpoint", "callback")
			.Out("start", "Started", PortType.Execution)
			.Out("body", "Body", PortType.Json, description: "JSON body of the request.")
			.Out("headers", "Headers", PortType.Object, description: "Request headers.")
			.Out("query", "Query", PortType.Object, description: "Query string parameters.")
			.Param(Param.Select("method", "Method", ["GET", "POST", "PUT", "PATCH", "DELETE"], "POST"))
			.Param(Param.Text("path", "Path", required: true, placeholder: "/my-hook"))
			.Param(Param.Select("authentication", "Authentication", ["None", "Header token", "Basic"], "None"))
			.Param(Param.Secret("credential", "Token or password", required: true).When("authentication", "Header token", "Basic"))
			.Param(Param.Select("responseMode", "Respond", ["Immediately", "When the last node finishes"], "Immediately"))
			.Build();

		yield return NodeDefinition.Create("trigger.schedule", "Schedule Trigger", BuiltInCategories.Triggers)
			.Describe("Starts the workflow on a timer or a cron schedule.")
			.Icon("clock")
			.Keywords("cron", "timer", "interval", "periodic")
			.Out("start", "Started", PortType.Execution)
			.Out("timestamp", "Timestamp", PortType.DateTime)
			.Param(Param.Select("mode", "Schedule", ["Interval", "Cron"], "Interval"))
			.Param(Param.Number("minutes", "Every (minutes)", 15, required: true, minimum: 1, maximum: 525600).When("mode", "Interval"))
			.Param(Param.Text("cron", "Cron expression", "0 * * * *", required: true, placeholder: "0 * * * *").When("mode", "Cron"))
			.Param(Param.Text("timezone", "Time zone", "UTC"))
			.Build();

		yield return NodeDefinition.Create("trigger.event", "Event Trigger", BuiltInCategories.Triggers)
			.Describe("Starts the workflow when an application or plugin event is raised.")
			.Icon("bolt")
			.Keywords("listen", "subscribe", "signal")
			.Out("start", "Started", PortType.Execution)
			.Out("event", "Event", PortType.Event)
			.Out("payload", "Payload", PortType.Json)
			.Param(Param.Text("eventName", "Event name", required: true, placeholder: "order.created"))
			.Param(Param.Select("source", "Source", ["Application", "Plugin", "System"], "Application"))
			.Build();
	}
}
