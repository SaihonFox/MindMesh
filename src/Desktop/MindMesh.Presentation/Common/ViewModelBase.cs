using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Common;

/// <summary>
/// Base class of all view models. Gives views access to localized text through <see cref="Loc"/>:
/// <c>{Binding Loc[Some_Key]}</c>.
/// <para>
/// When the language changes, <c>PropertyChanged("Loc")</c> is raised, which makes every
/// <c>Loc[...]</c> binding re-read its text, and <see cref="OnLanguageChanged"/> is called so view models that
/// expose localized text through their own properties can refresh them. The subscription is weak,
/// so short-lived view models (pages) are never kept alive by the localizer.
/// </para>
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
	protected ViewModelBase(ILocalizer localizer)
	{
		Loc = localizer;
		_ = new LocalizerSubscription(this, localizer);
	}

	public ILocalizer Loc { get; }

	/// <summary>Raise <c>PropertyChanged</c> for every property that returns localized text.</summary>
	protected virtual void OnLanguageChanged()
	{
	}

	private void HandleLocalizerChanged()
	{
		OnPropertyChanged(nameof(Loc));
		OnLanguageChanged();
	}

	private sealed class LocalizerSubscription
	{
		private readonly WeakReference<ViewModelBase> _target;
		private readonly ILocalizer _localizer;

		public LocalizerSubscription(ViewModelBase target, ILocalizer localizer)
		{
			_target = new WeakReference<ViewModelBase>(target);
			_localizer = localizer;
			localizer.PropertyChanged += OnLocalizerPropertyChanged;
		}

		private void OnLocalizerPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Item[]")
			{
				return;
			}

			if (_target.TryGetTarget(out var target))
			{
				target.HandleLocalizerChanged();
			}
			else
			{
				_localizer.PropertyChanged -= OnLocalizerPropertyChanged;
			}
		}
	}
}
