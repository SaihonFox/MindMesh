using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// Search among the nodes of the open graph: matches are highlighted and the canvas jumps from one to the next.
/// </summary>
public sealed partial class CanvasFindViewModel : ObservableObject
{
	private readonly GraphEditorViewModel _editor;
	private readonly NodeText _text;
	private List<NodeViewModel> _matches = [];
	private int _index = -1;

	[ObservableProperty]
	private bool _isOpen;

	[ObservableProperty]
	private string _searchText = string.Empty;

	public CanvasFindViewModel(GraphEditorViewModel editor, NodeText text)
	{
		_editor = editor;
		_text = text;
	}

	public int MatchCount => _matches.Count;

	/// <summary>1-based number of the current match; 0 when there is none.</summary>
	public int CurrentNumber => _index + 1;

	public bool HasMatches => _matches.Count > 0;

	public void Open() => IsOpen = true;

	public void Close()
	{
		IsOpen = false;
		SearchText = string.Empty;
	}

	public void Next() => Move(1);

	public void Previous() => Move(-1);

	partial void OnSearchTextChanged(string value) => Refresh();

	private void Refresh()
	{
		foreach (var node in _matches)
			node.IsFindMatch = false;

		var query = SearchText.Trim();

		_matches = query.Length == 0
			? []
			: [.. _editor.Nodes.Where(node =>
				node.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase)
				|| _text.Matches(node.Definition, query))];

		foreach (var node in _matches)
			node.IsFindMatch = true;

		_index = -1;
		Notify();

		if (_matches.Count > 0)
			Move(1);
	}

	private void Move(int delta)
	{
		if (_matches.Count == 0)
			return;

		_index = (_index + delta + _matches.Count) % _matches.Count;

		var node = _matches[_index];
		_editor.SelectNode(node, additive: false);
		_editor.Viewport.CenterOn(node.Bounds.Center);
		Notify();
	}

	private void Notify()
	{
		OnPropertyChanged(nameof(MatchCount));
		OnPropertyChanged(nameof(CurrentNumber));
		OnPropertyChanged(nameof(HasMatches));
	}
}
