using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Navigation;

/// <summary>
/// Base class of screens shown in the main content area.
/// </summary>
public abstract class PageViewModel : ViewModelBase
{
	protected PageViewModel(PageKey key, ILocalizer localizer)
		: base(localizer)
	{
		Key = key;
	}

	public PageKey Key { get; }

	/// <summary>Called after the page became the current page.</summary>
	public virtual void OnNavigatedTo()
	{
	}

	/// <summary>Called before another page replaces this one.</summary>
	public virtual void OnNavigatedFrom()
	{
	}
}
