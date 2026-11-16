namespace MindMesh.Graph.Definitions;

/// <summary>
/// Makes a setting visible only while another setting has one of the given values.
/// </summary>
public sealed record ParameterCondition(string Key, IReadOnlyList<string> Values);
