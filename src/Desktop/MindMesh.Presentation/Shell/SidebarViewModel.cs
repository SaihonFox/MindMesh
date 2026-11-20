using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.Settings;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;

namespace MindMesh.Presentation.Shell;

/// <summary>
/// Left sidebar: navigation between the main sections, with an expanded and a compact mode.
/// The editor opens it compact, so the canvas gets the room; the user can still expand it there.
/// </summary>
public sealed partial class SidebarViewModel : ViewModelBase
{
	public const double ExpandedWidth = 184;

	public const double CompactWidth = 52;

	private readonly ISettingsService _settings;
	private readonly INavigationService _navigation;
	private readonly bool _isConstructed;
	private bool _isEditorOpen;
	private bool _preferredCompact;
	private bool _isRestoring;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Width))]
	[NotifyPropertyChangedFor(nameof(ToggleTitle))]
	private bool _isCompact;

	public SidebarViewModel(
		ILocalizer localizer,
		INavigationService navigation,
		ISettingsService settings)
		: base(localizer)
	{
		_navigation = navigation;
		_settings = settings;

		Items =
		[
			CreateItem(PageKey.Projects, "Sidebar_Projects", IconKeys.Projects),
			CreateItem(PageKey.Editor, "Sidebar_Workflows", IconKeys.Workflows),
			CreateItem(PageKey.Templates, "Sidebar_Templates", IconKeys.Templates),
			CreateItem(PageKey.Marketplace, "Sidebar_Marketplace", IconKeys.Marketplace),
			CreateItem(PageKey.Executions, "Sidebar_Executions", IconKeys.Executions),
			CreateItem(PageKey.Plugins, "Sidebar_Plugins", IconKeys.Plugins),
		];

		FooterItems =
		[
			CreateItem(PageKey.Settings, "Sidebar_Settings", IconKeys.Settings),
		];

		_preferredCompact = settings.Current.Layout.IsSidebarCompact;
		_isCompact = _preferredCompact;
		UpdateSelection();
		_isConstructed = true;

		navigation.PropertyChanged += OnNavigationPropertyChanged;

		void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(INavigationService.CurrentPageKey))
				UpdateSelection();
		}
	}

	public IReadOnlyList<NavigationItemViewModel> Items { get; }

	public IReadOnlyList<NavigationItemViewModel> FooterItems { get; }

	public double Width => IsCompact ? CompactWidth : ExpandedWidth;

	/// <summary>Text of the collapse button: what pressing it will do.</summary>
	public string ToggleTitle => Loc[IsCompact ? "Sidebar_Expand" : "Sidebar_Collapse"];

	[RelayCommand]
	private void ToggleCompact() => IsCompact = !IsCompact;

	partial void OnIsCompactChanged(bool value)
	{
		ApplyCompactToItems(value);

		if (!_isConstructed || _isRestoring)
			return;

		// Inside the editor the choice is temporary: the stored preference is for the other pages.
		if (_isEditorOpen)
			return;

		_preferredCompact = value;
		_settings.Update(settings => settings with
		{
			Layout = settings.Layout with { IsSidebarCompact = value },
		});
	}

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(ToggleTitle));

	private NavigationItemViewModel CreateItem(PageKey key, string titleKey, string iconKey) =>
		new(Loc, key, titleKey, iconKey, _navigation.NavigateTo);

	private void ApplyCompactToItems(bool isCompact)
	{
		foreach (var item in Items.Concat(FooterItems))
			item.IsCompact = isCompact;
	}

	private void UpdateSelection()
	{
		PageKey current = _navigation.CurrentPageKey;

		bool isEditor = current == PageKey.Editor;

		if (isEditor != _isEditorOpen)
		{
			_isEditorOpen = isEditor;
			_isRestoring = true;
			IsCompact = isEditor || _preferredCompact;
			_isRestoring = false;
		}

		ApplyCompactToItems(IsCompact);

		foreach (var item in Items.Concat(FooterItems))
			item.IsSelected = item.PageKey == current;
	}
}
