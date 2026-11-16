using System.ComponentModel;
using System.Globalization;

namespace MindMesh.Presentation.Localization;

/// <summary>
/// Provides localized UI text. Raises <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// with <c>Item[]</c> when the language changes, so indexer bindings refresh by themselves.
/// </summary>
public interface ILocalizer : INotifyPropertyChanged
{
	/// <summary>Text for a resource key; <c>[key]</c> if the key does not exist.</summary>
	string this[string key] { get; }

	/// <summary>Looks a key up without the <c>[key]</c> fallback, so callers can use their own default text.</summary>
	bool TryGet(string key, out string value);

	/// <summary>The culture of the texts currently returned.</summary>
	CultureInfo Culture { get; }
}
