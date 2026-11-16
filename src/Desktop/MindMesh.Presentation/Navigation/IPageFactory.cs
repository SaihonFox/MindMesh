namespace MindMesh.Presentation.Navigation;

/// <summary>
/// Creates (or returns the shared instance of) the view model of a page.
/// </summary>
public interface IPageFactory
{
	PageViewModel Create(PageKey key);
}
