using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;

namespace MindMesh.Presentation.Shell;

/// <summary>
/// One button of the left sidebar.
/// </summary>
public sealed partial class NavigationItemViewModel : ViewModelBase
{
	private readonly string _titleKey;

	[ObservableProperty]
	private bool _isSelected;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ShowLabel))]
	private bool _isCompact;

	public NavigationItemViewModel(
		ILocalizer localizer,
		PageKey pageKey,
		string titleKey,
		string iconKey,
		Action<PageKey> navigate)
		: base(localizer)
	{
		PageKey = pageKey;
		IconKey = iconKey;
		_titleKey = titleKey;
		ActivateCommand = new RelayCommand(() => navigate(pageKey));
	}

	public PageKey PageKey { get; }

	public string IconKey { get; }

	public string Title => Loc[_titleKey];

	/// <summary>The text label is hidden in compact mode.</summary>
	public bool ShowLabel => !IsCompact;

	public IRelayCommand ActivateCommand { get; }

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(Title));
}
