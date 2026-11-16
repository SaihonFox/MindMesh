using System.ComponentModel;
using System.Globalization;
using System.Resources;
using MindMesh.Application.Localization;
using MindMesh.Application.Settings;

namespace MindMesh.Presentation.Localization;

/// <summary>
/// Resource based localizer. Follows <see cref="AppSettings.Language"/>.
/// </summary>
public sealed class Localizer : ILocalizer
{
	/// <summary>Base name of the embedded resources (<c>Localization/Resources/Strings*.resx</c>).</summary>
	public const string ResourceBaseName = "MindMesh.Presentation.Localization.Resources.Strings";

	private static readonly ResourceManager ResourceManager = new(ResourceBaseName, typeof(Localizer).Assembly);

	private CultureInfo _culture;

	public Localizer(ISettingsService settings)
	{
		_culture = ResolveCulture(settings.Current.Language, CultureInfo.CurrentUICulture);
		settings.Changed += OnSettingsChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public CultureInfo Culture => _culture;

	public string this[string key] => ResourceManager.GetString(key, _culture) ?? $"[{key}]";

	public bool TryGet(string key, out string value)
	{
		var text = ResourceManager.GetString(key, _culture);
		value = text ?? string.Empty;

		return text is not null;
	}

	/// <summary>
	/// Maps the stored language setting to a culture.
	/// </summary>
	public static CultureInfo ResolveCulture(string? languageSetting, CultureInfo systemCulture)
	{
		var code = AppLanguages.Resolve(languageSetting, systemCulture.TwoLetterISOLanguageName);
		return CultureInfo.GetCultureInfo(code);
	}

	private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
	{
		if (e.Previous.Language == e.Current.Language)
		{
			return;
		}

		var culture = ResolveCulture(e.Current.Language, CultureInfo.CurrentUICulture);

		if (culture.Name == _culture.Name)
		{
			return;
		}

		_culture = culture;

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
	}
}
