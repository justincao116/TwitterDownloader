using X.Desktop.Infrastructure;
using X.Desktop.Models;
using X.Desktop.Services;

namespace X.Desktop.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly ICredentialStore _credentialStore;
    private readonly ILanguageService _languageService;
    private string _cookieInput = string.Empty;
    private bool _isCookieConfigured;
    private string _statusMessage = string.Empty;
    private LanguageOption _selectedLanguage;

    public SettingsViewModel(ICredentialStore credentialStore, ILanguageService languageService)
    {
        _credentialStore = credentialStore;
        _languageService = languageService;
        LanguageOptions = [new("zh-CN", "简体中文"), new("en-US", "English")];
        _selectedLanguage = LanguageOptions.First(option =>
            option.Code.Equals(languageService.CurrentLanguage, StringComparison.OrdinalIgnoreCase));
        SaveCookieCommand = new RelayCommand(SaveCookie, () => HasCookieInput);
        ClearCookieCommand = new RelayCommand(ClearCookie, () => IsCookieConfigured);
        RefreshCookieState();
    }

    public IReadOnlyList<LanguageOption> LanguageOptions { get; }
    public RelayCommand SaveCookieCommand { get; }
    public RelayCommand ClearCookieCommand { get; }

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set { if (SetProperty(ref _selectedLanguage, value)) _languageService.SetLanguage(value.Code); }
    }

    public string CookieInput
    {
        get => _cookieInput;
        set
        {
            if (SetProperty(ref _cookieInput, value))
            {
                OnPropertyChanged(nameof(HasCookieInput));
                SaveCookieCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasCookieInput => !string.IsNullOrWhiteSpace(CookieInput);

    public bool IsCookieConfigured
    {
        get => _isCookieConfigured;
        private set
        {
            if (SetProperty(ref _isCookieConfigured, value))
                ClearCookieCommand.NotifyCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value)) OnPropertyChanged(nameof(HasStatus));
        }
    }

    public bool HasStatus => !string.IsNullOrWhiteSpace(StatusMessage);

    private void SaveCookie()
    {
        if (XCookieParser.Parse(CookieInput) is null)
        {
            StatusMessage = _languageService.Get("CookieInvalid");
            return;
        }

        try
        {
            _credentialStore.SaveCookie(CookieInput.Trim());
            CookieInput = string.Empty;
            IsCookieConfigured = true;
            StatusMessage = _languageService.Get("CookieSaved");
        }
        catch { StatusMessage = _languageService.Get("CookieSaveFailed"); }
    }

    private void ClearCookie()
    {
        try
        {
            _credentialStore.DeleteCookie();
            CookieInput = string.Empty;
            IsCookieConfigured = false;
            StatusMessage = _languageService.Get("CookieCleared");
        }
        catch { StatusMessage = _languageService.Get("CookieClearFailed"); }
    }

    private void RefreshCookieState()
    {
        try { IsCookieConfigured = !string.IsNullOrWhiteSpace(_credentialStore.ReadCookie()); }
        catch
        {
            IsCookieConfigured = false;
            StatusMessage = _languageService.Get("CookieReadFailed");
        }
    }
}
