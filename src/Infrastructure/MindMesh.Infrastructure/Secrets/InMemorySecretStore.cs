using System.Collections.Concurrent;
using MindMesh.Application.Secrets;

namespace MindMesh.Infrastructure.Secrets;

/// <summary>
/// Keeps secrets in memory for the lifetime of the process.
/// Encrypted persistence (system keychain) arrives together with project saving.
/// </summary>
public sealed class InMemorySecretStore : ISecretStore
{
	private const string Prefix = "secret:";

	private readonly ConcurrentDictionary<string, string> _values = new(StringComparer.Ordinal);

	public string CreateReference() => Prefix + Guid.CreateVersion7().ToString("N");

	public bool Has(string reference) => !string.IsNullOrEmpty(reference) && _values.ContainsKey(reference);

	public string? Get(string reference) =>
		!string.IsNullOrEmpty(reference) && _values.TryGetValue(reference, out var value) ? value : null;

	public void Set(string reference, string value)
	{
		ArgumentException.ThrowIfNullOrEmpty(reference);
		ArgumentNullException.ThrowIfNull(value);

		_values[reference] = value;
	}

	public void Remove(string reference)
	{
		if (!string.IsNullOrEmpty(reference))
			_values.TryRemove(reference, out _);
	}
}
