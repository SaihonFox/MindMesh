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
/// Inside the editor it is always compact, so the canvas gets the room.
/// </summary>
public sealed partial class SidebarViewModel : ViewModelBase
{
	public const double ExpandedWidth = 184;

	public const double CompactWidth = 52;

	private readonly ISettingsService _settings;
	private readonly INavigationService _navigation;
	private bool _isEditorOpen;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Width))]
	[NotifyPropertyChangedFor(nameof(IsShownCompact))]
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

		_isCompact = settings.Current.Layout.IsSidebarCompact;
		UpdateSelection();

		navigation.PropertyChanged += OnNavigationPropertyChanged;

		void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(INavigationService.CurrentPageKey))
				UpdateSelection();
		}
	}

	public IReadOnlyList<NavigationItemViewModel> Items { get; }

	public IReadOnlyList<NavigationItemViewModel> FooterItems { get; }

	/// <summary>The sidebar shows icons only: by the user's choice, or because the editor is open.</summary>
	public bool IsShownCompact => IsCompact || _isEditorOpen;

	public double Width => IsShownCompact ? CompactWidth : ExpandedWidth;

	[RelayCommand]
	private void ToggleCompact() => IsCompact = !IsCompact;

	partial void OnIsCompactChanged(bool value)
	{
		ApplyCompactToItems(IsShownCompact);

		_settings.Update(settings => settings with
		{
			Layout = settings.Layout with { IsSidebarCompact = value },
		});
	}

	private NavigationItemViewModel CreateItem(PageKey key, string titleKey, string iconKey) =>
		new(Loc, key, titleKey, iconKey, _navigation.NavigateTo);

	private void ApplyCompactToItems(bool isCompact)
	{
		foreach (var item in Items.Concat(FooterItems))
			item.IsCompact = isCompact;
	}

	private void UpdateSelection()
	{
		var current = _navigation.CurrentPageKey;

		_isEditorOpen = current == PageKey.Editor;
		ApplyCompactToItems(IsShownCompact);
		OnPropertyChanged(nameof(IsShownCompact));
		OnPropertyChanged(nameof(Width));

		foreach (var item in Items.Concat(FooterItems))
			item.IsSelected = item.PageKey == current;
	}
}
