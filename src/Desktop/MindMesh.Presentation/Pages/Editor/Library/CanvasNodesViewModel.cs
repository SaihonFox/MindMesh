using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Canvas;

namespace MindMesh.Presentation.Pages.Editor.Library;

/// <summary>
/// The nodes that are on the canvas, as a list that can be searched: pick one to show it, or manage it from here.
/// The rows are the node view models themselves, so titles, selection and state follow the canvas by themselves.
/// </summary>
public sealed partial class CanvasNodesViewModel : ViewModelBase
{
	private readonly GraphEditorViewModel _editor;

	[ObservableProperty]
	private string _searchText = string.Empty;

	public CanvasNodesViewModel(ILocalizer localizer, GraphEditorViewModel editor)
		: base(localizer)
	{
		_editor = editor;
		_editor.Nodes.CollectionChanged += OnNodesChanged;
		Refresh();
	}

	public ObservableCollection<NodeViewModel> Items { get; } = [];

	public bool HasItems => Items.Count > 0;

	public bool IsEmpty => _editor.Nodes.Count == 0;

	/// <summary>Selects the node and brings it into the middle of the view.</summary>
	[RelayCommand]
	private void Show(NodeViewModel? node)
	{
		if (node is null)
			return;

		_editor.SelectNode(node, additive: false);
		_editor.BringToFront(node);
		_editor.Viewport.CenterOn(new Point2D(node.X + (node.Width / 2), node.Y + (node.Height / 2)));
	}

	[RelayCommand]
	private void ToggleDisabled(NodeViewModel? node)
	{
		if (node is null)
			return;

		_editor.SelectNode(node, additive: false);
		_editor.ToggleDisabled();
	}

	[RelayCommand]
	private void ToggleCollapsed(NodeViewModel? node)
	{
		if (node is null)
			return;

		_editor.SelectNode(node, additive: false);
		_editor.ToggleCollapsed();
	}

	[RelayCommand]
	private void Delete(NodeViewModel? node)
	{
		if (node is null)
			return;

		_editor.SelectNode(node, additive: false);
		_editor.DeleteSelection();
	}

	partial void OnSearchTextChanged(string value) => Refresh();

	protected override void OnLanguageChanged() => Refresh();

	private void OnNodesChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

	private void Refresh()
	{
		string query = SearchText.Trim();

		IEnumerable<NodeViewModel> matches = _editor.Nodes.Where(node =>
			query.Length == 0
			|| node.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase)
			|| node.CategoryTitle.Contains(query, StringComparison.CurrentCultureIgnoreCase));

		Items.Clear();

		foreach (NodeViewModel node in matches.OrderBy(node => node.Y).ThenBy(node => node.X))
			Items.Add(node);

		OnPropertyChanged(nameof(HasItems));
		OnPropertyChanged(nameof(IsEmpty));
	}
}
