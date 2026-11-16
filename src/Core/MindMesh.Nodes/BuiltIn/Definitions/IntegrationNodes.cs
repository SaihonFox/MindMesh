using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

internal static class IntegrationNodes
{
	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("integration.telegram", "Telegram", BuiltInCategories.Integrations)
			.Describe("Sends a message with a Telegram bot.")
			.Icon("send")
			.Keywords("bot", "message", "chat", "notify")
			.In("in", "In", PortType.Execution)
			.In("text", "Text", PortType.String)
			.Out("out", "Out", PortType.Execution)
			.Out("messageId", "Message id", PortType.Integer)
			.Param(Param.Secret("token", "Bot token", required: true))
			.Param(Param.Text("chatId", "Chat id", required: true))
			.Param(Param.Expression("text", "Message", required: true))
			.Param(Param.Select("parseMode", "Format", ["Plain", "Markdown", "HTML"], "Plain"))
			.Build();

		yield return NodeDefinition.Create("integration.discord", "Discord", BuiltInCategories.Integrations)
			.Describe("Posts a message to a Discord channel.")
			.Icon("message")
			.Keywords("bot", "message", "channel", "notify", "webhook")
			.In("in", "In", PortType.Execution)
			.In("text", "Text", PortType.String)
			.Out("out", "Out", PortType.Execution)
			.Param(Param.Secret("webhookUrl", "Webhook URL", required: true))
			.Param(Param.Expression("text", "Message", required: true))
			.Param(Param.Text("username", "Display name"))
			.Build();

		yield return NodeDefinition.Create("integration.github", "GitHub", BuiltInCategories.Integrations)
			.Describe("Works with issues, pull requests and repositories on GitHub.")
			.Icon("github")
			.Keywords("git", "issue", "pull request", "repository", "commit")
			.In("in", "In", PortType.Execution)
			.In("payload", "Payload", PortType.Json)
			.Out("out", "Out", PortType.Execution)
			.Out("result", "Result", PortType.Json)
			.Param(Param.Secret("token", "Access token", required: true))
			.Param(Param.Text("repository", "Repository", required: true, placeholder: "owner/name"))
			.Param(Param.Select("operation", "Operation", ["Create issue", "Comment on issue", "Get issue", "List issues", "Create pull request"], "Create issue"))
			.Param(Param.Text("title", "Title").When("operation", "Create issue", "Create pull request"))
			.Param(Param.Expression("body", "Text").When("operation", "Create issue", "Comment on issue", "Create pull request"))
			.Build();

		yield return NodeDefinition.Create("integration.email", "Email", BuiltInCategories.Integrations)
			.Describe("Sends an email through an SMTP server.")
			.Icon("mail")
			.Keywords("smtp", "mail", "send", "message", "notify")
			.In("in", "In", PortType.Execution)
			.In("body", "Body", PortType.String)
			.In("attachment", "Attachment", PortType.Binary)
			.Out("out", "Out", PortType.Execution)
			.Param(Param.Text("host", "SMTP server", required: true, placeholder: "smtp.example.com"))
			.Param(Param.Number("port", "Port", 587, required: true, minimum: 1, maximum: 65535))
			.Param(Param.Text("username", "User name"))
			.Param(Param.Secret("password", "Password"))
			.Param(Param.Text("from", "From", required: true))
			.Param(Param.Text("to", "To", required: true))
			.Param(Param.Text("subject", "Subject", required: true))
			.Param(Param.Expression("body", "Body"))
			.Param(Param.Boolean("html", "Send as HTML"))
			.Build();

		yield return NodeDefinition.Create("integration.generic-webhook", "Generic Webhook", BuiltInCategories.Integrations)
			.Describe("Posts a JSON payload to any webhook address.")
			.Icon("webhook")
			.Keywords("post", "callback", "notify", "zapier", "make")
			.In("in", "In", PortType.Execution)
			.In("payload", "Payload", PortType.Json)
			.Out("out", "Out", PortType.Execution)
			.Out("status", "Status", PortType.Integer)
			.Param(Param.Secret("url", "Webhook URL", required: true, description: "Often contains a secret key, so it is stored separately."))
			.Param(Param.Json("payload", "Payload", "{}"))
			.Param(Param.Fields("headers", "Headers"))
			.Param(Param.Number("retries", "Retries", 0, minimum: 0, maximum: 10))
			.Build();
	}
}
