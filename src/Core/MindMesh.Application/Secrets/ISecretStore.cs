namespace MindMesh.Application.Secrets;

/// <summary>
/// Keeps secrets (passwords, tokens, connection strings) apart from the graph.
/// A node parameter of type Secret holds only a reference; the value lives here.
/// </summary>
public interface ISecretStore
{
	/// <summary>Creates a new, empty reference. The graph stores this reference.</summary>
	string CreateReference();

	bool Has(string reference);

	string? Get(string reference);

	void Set(string reference, string value);

	void Remove(string reference);
}
