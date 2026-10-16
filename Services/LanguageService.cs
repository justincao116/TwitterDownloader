using System.Globalization;
using System.Windows;
using X.Desktop.Models;

namespace X.Desktop.Services;

public sealed class LanguageService(IAppSettingsService settingsService) : ILanguageService
{
    private readonly IAppSettingsService _settingsService = settingsService;
    private AppSettings _settings = new();

    public event EventHandler? CultureChanged;
    public string CurrentLanguage { get; private set; } = "en-US";

    public void Initialize()
    {
        _settings = _settingsService.Load();
        string fallback = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals("zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en-US";
        ApplyLanguage(Normalize(_settings.Language ?? fallback), false);
    }

    public void SetLanguage(string languageCode) => ApplyLanguage(Normalize(languageCode), true);

    public string Get(string resourceKey) =>
        System.Windows.Application.Current.TryFindResource(resourceKey)?.ToString() ?? resourceKey;

    private void ApplyLanguage(string languageCode, bool persist)
    {
        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("Strings.", StringComparison.OrdinalIgnoreCase) == true);
        if (current is not null) dictionaries.Remove(current);

        dictionaries.Add(new ResourceDictionary
        {
            Source = new Uri($"Resources/Strings.{languageCode}.xaml", UriKind.Relative)
        });

        CurrentLanguage = languageCode;
        var culture = CultureInfo.GetCultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (persist)
        {
            _settings.Language = languageCode;
            _settingsService.Save(_settings);
        }

        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string Normalize(string code) =>
        code.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en-US";
}
