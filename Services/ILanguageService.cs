namespace X.Desktop.Services;
public interface ILanguageService
{
    event EventHandler? CultureChanged;
    string CurrentLanguage { get; }
    void Initialize();
    void SetLanguage(string languageCode);
    string Get(string resourceKey);
}
