using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace MindMesh.Presentation.Navigation;

/// <inheritdoc cref="INavigationService" />
public sealed class NavigationService : ObservableObject, INavigationService
{
	private readonly IPageFactory _pageFactory;
	private readonly ILogger<NavigationService> _logger;

	private PageViewModel? _currentPage;
	private PageKey? _previousKey;

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
				OnPropertyChanged(nameof(CurrentPageKey));
		}
	}

	public PageKey? CurrentPageKey => _currentPage?.Key;

	public bool CanGoBack => _previousKey is not null;

	public void GoBack() => NavigateTo(_previousKey ?? PageKey.Welcome);

	public void NavigateTo(PageKey key)
	{
		if (_currentPage?.Key == key)
			return;

		_previousKey = _currentPage?.Key;

		var next = _pageFactory.Create(key);

		_currentPage?.OnNavigatedFrom();
		CurrentPage = next;
		OnPropertyChanged(nameof(CanGoBack));
		next.OnNavigatedTo();

		_logger.LogInformation("Navigated to {PageKey}.", key);
	}
}
