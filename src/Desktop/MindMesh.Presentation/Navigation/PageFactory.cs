namespace MindMesh.Presentation.Navigation;

/// <inheritdoc cref="IPageFactory" />
public sealed class PageFactory : IPageFactory
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IReadOnlyDictionary<PageKey, PageRegistration> _registrations;

	public PageFactory(IServiceProvider serviceProvider, IEnumerable<PageRegistration> registrations)
	{
		_serviceProvider = serviceProvider;

		var map = new Dictionary<PageKey, PageRegistration>();

		foreach (var registration in registrations)
		{
			if (!map.TryAdd(registration.Key, registration))
			{
				throw new InvalidOperationException($"Page '{registration.Key}' is registered more than once.");
			}
		}

		_registrations = map;
	}

	public PageViewModel Create(PageKey key)
	{
		if (!_registrations.TryGetValue(key, out var registration))
		{
			throw new InvalidOperationException($"Page '{key}' is not registered.");
		}

		return registration.Create(_serviceProvider);
	}
}
