using System.Net.Http;
using System.Windows;
using X.Desktop.Services;
using X.Desktop.ViewModels;

namespace X.Desktop;

public partial class App : System.Windows.Application
{
    private HttpClient? _twitterHttpClient;
    private HttpClient? _downloadHttpClient;
    private MainViewModel? _mainViewModel;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var settingsService = new AppSettingsService();
        var languageService = new LanguageService(settingsService);
        languageService.Initialize();

        var credentialStore = new WindowsCredentialStore();
        var cookieProvider = new DesktopXCookieProvider(credentialStore);
        _twitterHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
        _downloadHttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

        var twitterService = new TwitterService(_twitterHttpClient, cookieProvider);
        var downloadService = new MediaDownloadService(_downloadHttpClient);
        var fileDialogService = new FileDialogService();
        var browserService = new BrowserService();
        var settingsDialogService = new SettingsDialogService(credentialStore, languageService);

        _mainViewModel = new MainViewModel(twitterService, downloadService, fileDialogService,
            browserService, settingsDialogService, languageService);
        var window = new MainWindow { DataContext = _mainViewModel };
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mainViewModel?.Dispose();
        _twitterHttpClient?.Dispose();
        _downloadHttpClient?.Dispose();
        base.OnExit(e);
    }
}
