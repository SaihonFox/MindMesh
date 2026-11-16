using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

internal static class HttpNodes
{
	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("http.request", "HTTP Request", BuiltInCategories.Http)
			.Describe("Sends an HTTP request and returns the response.")
			.Icon("globe")
			.Keywords("api", "fetch", "get", "post", "rest", "call")
			.In("in", "In", PortType.Execution)
			.In("body", "Body", PortType.Json, description: "Overrides the body setting.")
			.Out("out", "Out", PortType.Execution)
			.Out("response", "Response", PortType.Json, description: "Parsed response body.")
			.Out("status", "Status", PortType.Integer, description: "HTTP status code.")
			.Out("headers", "Headers", PortType.Object, description: "Response headers.")
			.Param(Param.Select("method", "Method", ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD"], "GET"))
			.Param(Param.Text("url", "URL", required: true, placeholder: "https://api.example.com/items"))
			.Param(Param.Fields("headers", "Headers"))
			.Param(Param.Fields("query", "Query parameters"))
			.Param(Param.Json("body", "Body", "{}").When("method", "POST", "PUT", "PATCH"))
			.Param(Param.Select("authentication", "Authentication", ["None", "Bearer token", "Basic", "API key"], "None"))
			.Param(Param.Secret("credential", "Credential", required: true).When("authentication", "Bearer token", "Basic", "API key"))
			.Param(Param.Number("timeout", "Timeout (ms)", 30000, minimum: 1, maximum: 600000))
			.Param(Param.Boolean("followRedirects", "Follow redirects", true))
			.Build();

		yield return NodeDefinition.Create("http.response", "HTTP Response", BuiltInCategories.Http)
			.Describe("Sends the reply to the request that started the workflow.")
			.Icon("reply")
			.Keywords("reply", "answer", "return")
			.In("in", "In", PortType.Execution)
			.In("body", "Body", PortType.Json)
			.Param(Param.Number("statusCode", "Status code", 200, required: true, minimum: 100, maximum: 599))
			.Param(Param.Select("contentType", "Content type", ["application/json", "text/plain", "text/html"], "application/json"))
			.Param(Param.Fields("headers", "Headers"))
			.Param(Param.Expression("template", "Body (when no body is connected)"))
			.Build();

		yield return NodeDefinition.Create("http.rest-client", "REST Client", BuiltInCategories.Http)
			.Describe("Calls a REST resource of a base URL, with retries.")
			.Icon("server")
			.Keywords("api", "resource", "retry", "crud")
			.In("in", "In", PortType.Execution)
			.In("payload", "Payload", PortType.Json)
			.Out("out", "Out", PortType.Execution)
			.Out("result", "Result", PortType.Json)
			.Out("status", "Status", PortType.Integer)
			.Param(Param.Text("baseUrl", "Base URL", required: true, placeholder: "https://api.example.com"))
			.Param(Param.Select("method", "Method", ["GET", "POST", "PUT", "PATCH", "DELETE"], "GET"))
			.Param(Param.Text("resource", "Resource", required: true, placeholder: "/items/{id}"))
			.Param(Param.Fields("headers", "Headers"))
			.Param(Param.Secret("token", "Bearer token"))
			.Param(Param.Number("retries", "Retries", 0, minimum: 0, maximum: 10))
			.Build();

		yield return NodeDefinition.Create("http.graphql", "GraphQL Request", BuiltInCategories.Http)
			.Describe("Runs a GraphQL query or mutation.")
			.Icon("braces")
			.Keywords("query", "mutation", "api")
			.In("in", "In", PortType.Execution)
			.In("variables", "Variables", PortType.Object)
			.Out("out", "Out", PortType.Execution)
			.Out("data", "Data", PortType.Json)
			.Out("errors", "Errors", PortType.Array)
			.Param(Param.Text("endpoint", "Endpoint", required: true, placeholder: "https://api.example.com/graphql"))
			.Param(Param.Code("query", "Query", "graphql", "query {\n  __typename\n}", required: true))
			.Param(Param.Json("variables", "Variables", "{}"))
			.Param(Param.Fields("headers", "Headers"))
			.Param(Param.Secret("token", "Bearer token"))
			.Build();
	}
}
