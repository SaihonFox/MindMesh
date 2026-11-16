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
}
