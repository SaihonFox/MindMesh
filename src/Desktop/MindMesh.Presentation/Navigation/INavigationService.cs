using System.ComponentModel;

namespace MindMesh.Presentation.Navigation;

/// <summary>
/// Controls which page is shown in the main content area.
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
	PageViewModel? CurrentPage { get; }

	PageKey? CurrentPageKey { get; }

	void NavigateTo(PageKey key);

	/// <summary>True when there is a page to return to.</summary>
	bool CanGoBack { get; }

	/// <summary>Returns to the page that was open before the current one, or to the welcome page.</summary>
	void GoBack();
}
