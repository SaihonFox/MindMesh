namespace MindMesh.Graph.Layout;

/// <summary>
/// A size the user gave to a node card, in canvas units. A node without one has the default size.
/// </summary>
public readonly record struct NodeSize(double Width, double Height);
