using System.Collections.ObjectModel;
using System.Net;
using X.Desktop.Infrastructure;
using X.Desktop.Models;
using X.Desktop.Services;

namespace X.Desktop.ViewModels;

public sealed class MainViewModel : ObservableObject, IDisposable
{
    private readonly ITwitterService _twitterService;
    private readonly IMediaDownloadService _downloadService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IBrowserService _browserService;
    private readonly ISettingsDialogService _settingsDialogService;
    private readonly ILanguageService _languageService;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private string _tweetUrl = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public MainViewModel(ITwitterService twitterService, IMediaDownloadService downloadService,
        IFileDialogService fileDialogService, IBrowserService browserService,
        ISettingsDialogService settingsDialogService, ILanguageService languageService)
    {
        _twitterService = twitterService;
        _downloadService = downloadService;
        _fileDialogService = fileDialogService;
        _browserService = browserService;
        _settingsDialogService = settingsDialogService;
        _languageService = languageService;
        SearchCommand = new AsyncRelayCommand(SearchAsync, CanSearch);
        ClearCommand = new RelayCommand(Clear, () => HasTweetUrl && !IsBusy);
        OpenSettingsCommand = new RelayCommand(_settingsDialogService.Show);
    }

    public ObservableCollection<MediaItemViewModel> MediaItems { get; } = [];
    public AsyncRelayCommand SearchCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }

    public string TweetUrl
    {
        get => _tweetUrl;
        set
        {
            if (SetProperty(ref _tweetUrl, value))
            {
                OnPropertyChanged(nameof(HasTweetUrl));
                SearchCommand.NotifyCanExecuteChanged();
                ClearCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasTweetUrl => !string.IsNullOrWhiteSpace(TweetUrl);

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SearchCommand.NotifyCanExecuteChanged();
                ClearCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasResults => MediaItems.Count > 0;
    private bool CanSearch() => HasTweetUrl && !IsBusy;

    private async Task SearchAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        ClearMediaItems();

        try
        {
            TweetMediasResponse response = await _twitterService.ListTweetMediasAsync(
                new TweetMediasRequest { Url = TweetUrl.Trim() },
                _lifetimeCancellation.Token);
            PopulateMediaItems(response);
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested) { }
        catch (TwitterServiceException exception)
        {
            ErrorMessage = LocalizeFailure(exception.Error);
        }
        catch (HttpRequestException exception)
        {
            ErrorMessage = exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                ? _languageService.Get("ErrorCookieRejected")
                : _languageService.Get("ErrorNetwork");
        }
        catch { ErrorMessage = _languageService.Get("ErrorUnexpected"); }
        finally { IsBusy = false; }
    }

    private void PopulateMediaItems(TweetMediasResponse response)
    {
        string tweetId = TwitterRequestConstants.GetTweetId(TweetUrl) ?? "media";
        int index = 1;
        foreach (TweetMediaDto media in response.Medias)
            MediaItems.Add(new(media, index++, tweetId, _downloadService, _fileDialogService,
                _browserService, _languageService));

        if (MediaItems.Count == 0) ErrorMessage = _languageService.Get("ErrorNoMedia");
        OnPropertyChanged(nameof(HasResults));
    }

    private string LocalizeFailure(TwitterError error) => error switch
    {
        TwitterError.InvalidUrl => _languageService.Get("ErrorInvalidUrl"),
        TwitterError.Protected => _languageService.Get("ErrorProtected"),
        TwitterError.CookieRequired => _languageService.Get("ErrorCookieRequired"),
        TwitterError.CookieRejected => _languageService.Get("ErrorCookieRejected"),
        TwitterError.NoMedia => _languageService.Get("ErrorNoMedia"),
        TwitterError.TweetNotFound => _languageService.Get("ErrorTweetNotFound"),
        TwitterError.Unavailable => _languageService.Get("ErrorUnavailable"),
        TwitterError.TokenUnavailable => _languageService.Get("ErrorNetwork"),
        _ => _languageService.Get("ErrorUnexpected")
    };

    private void Clear()
    {
        TweetUrl = string.Empty;
        ErrorMessage = string.Empty;
        ClearMediaItems();
    }

    private void ClearMediaItems()
    {
        foreach (MediaItemViewModel item in MediaItems) item.Dispose();
        MediaItems.Clear();
        OnPropertyChanged(nameof(HasResults));
    }

    public void Dispose()
    {
        _lifetimeCancellation.Cancel();
        ClearMediaItems();
        _lifetimeCancellation.Dispose();
    }
}
