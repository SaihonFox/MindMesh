using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace MindMesh.Presentation.Navigation;

/// <inheritdoc cref="INavigationService" />
public sealed class NavigationService : ObservableObject, INavigationService
{
	private readonly IPageFactory _pageFactory;
	private readonly ILogger<NavigationService> _logger;

	private PageViewModel? _currentPage;

	public NavigationService(IPageFactory pageFactory, ILogger<NavigationService> logger)
	{
		_pageFactory = pageFactory;
		_logger = logger;
	}

	public PageViewModel? CurrentPage
	{
		get => _currentPage;
		private set
		{
			if (SetProperty(ref _currentPage, value))
			{
				OnPropertyChanged(nameof(CurrentPageKey));
			}
		}
	}

	public PageKey? CurrentPageKey => _currentPage?.Key;

	public void NavigateTo(PageKey key)
	{
		if (_currentPage?.Key == key)
		{
			return;
		}

		var next = _pageFactory.Create(key);

		_currentPage?.OnNavigatedFrom();
		CurrentPage = next;
		next.OnNavigatedTo();

		_logger.LogInformation("Navigated to {PageKey}.", key);
	}
}
