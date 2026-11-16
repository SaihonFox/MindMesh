namespace MindMesh.Plugins.Abstractions;

/// <summary>
/// An extension adds content to the application: project kinds, node categories and nodes.
/// The built-in content registers itself the same way, so plugins have no second-class status.
/// </summary>
/// <remarks>
/// Extension points deliberately do not include licensing, entitlements or account services:
/// extensions cannot replace or switch off the paid-feature checks of the host.
/// </remarks>
public interface IMindMeshExtension
{
	ExtensionInfo Info { get; }

	/// <summary>Called once at startup to register everything the extension contributes.</summary>
	void Register(IExtensionRegistrar registrar);
}
