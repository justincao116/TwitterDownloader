using X.Desktop.Infrastructure;
using X.Desktop.Models;
using X.Desktop.Services;

namespace X.Desktop.ViewModels;

public sealed class MediaItemViewModel : ObservableObject, IDisposable
{
    private readonly IMediaDownloadService _downloadService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IBrowserService _browserService;
    private readonly ILanguageService _languageService;
    private readonly string _tweetId;
    private MediaVariantOption? _selectedVariant;
    private bool _isDownloading;
    private double _downloadPercentage;
    private bool _isProgressIndeterminate;
    private string _statusMessage = string.Empty;
    private CancellationTokenSource? _downloadCancellation;

    public MediaItemViewModel(TweetMediaDto media, int index, string tweetId,
        IMediaDownloadService downloadService, IFileDialogService fileDialogService,
        IBrowserService browserService, ILanguageService languageService)
    {
        _downloadService = downloadService;
        _fileDialogService = fileDialogService;
        _browserService = browserService;
        _languageService = languageService;
        _tweetId = tweetId;
        Index = index;
        MediaType = media.Type;
        PreviewUrl = media.MediaUrlHttps;
        Variants = BuildVariants(media);
        _selectedVariant = Variants.FirstOrDefault();
        DownloadCommand = new AsyncRelayCommand(DownloadAsync, CanUseSelectedVariant);
        OpenCommand = new RelayCommand(OpenInBrowser, CanUseSelectedVariant);
        _languageService.CultureChanged += HandleCultureChanged;
        if (_selectedVariant is null) StatusMessage = _languageService.Get("NoDownloadableVariant");
    }

    public int Index { get; }
    public string MediaType { get; }
    public string PreviewUrl { get; }
    public IReadOnlyList<MediaVariantOption> Variants { get; }
    public AsyncRelayCommand DownloadCommand { get; }
    public RelayCommand OpenCommand { get; }

    public string Title => string.Format(_languageService.Get("MediaTitleFormat"), Index, LocalizedMediaType);
    public string LocalizedMediaType => MediaType switch
    {
        "video" => _languageService.Get("MediaTypeVideo"),
        "animated_gif" => _languageService.Get("MediaTypeGif"),
        "photo" => _languageService.Get("MediaTypePhoto"),
        _ => MediaType
    };

    public MediaVariantOption? SelectedVariant
    {
        get => _selectedVariant;
        set
        {
            if (SetProperty(ref _selectedVariant, value))
            {
                DownloadCommand.NotifyCanExecuteChanged();
                OpenCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        private set
        {
            if (SetProperty(ref _isDownloading, value))
            {
                DownloadCommand.NotifyCanExecuteChanged();
                OpenCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public double DownloadPercentage
    {
        get => _downloadPercentage;
        private set => SetProperty(ref _downloadPercentage, value);
    }

    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        private set => SetProperty(ref _isProgressIndeterminate, value);
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

    private IReadOnlyList<MediaVariantOption> BuildVariants(TweetMediaDto media)
    {
        if (media.Type == "photo")
            return [new(_languageService.Get("OriginalImage"), media.MediaUrlHttps, GetImageExtension(media.MediaUrlHttps))];

        return media.VideoVariants?
            .Where(v => v.ContentType.Equals("video/mp4", StringComparison.OrdinalIgnoreCase)
                && Uri.IsWellFormedUriString(v.Url, UriKind.Absolute))
            .GroupBy(v => v.Url, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderByDescending(v => v.Bitrate ?? 0)
            .Select(v => new MediaVariantOption(v.Bitrate is > 0
                    ? $"{v.Bitrate.Value / 1000:N0} kbps · MP4"
                    : _languageService.Get("OriginalGif"), v.Url, "mp4"))
            .ToList() ?? [];
    }

    private async Task DownloadAsync()
    {
        if (SelectedVariant is null) return;
        string defaultName = $"x-{_tweetId}-{Index}.{SelectedVariant.FileExtension}";
        string? destination = _fileDialogService.ShowSaveFile(defaultName, SelectedVariant.FileExtension);
        if (destination is null) return;

        _downloadCancellation?.Dispose();
        _downloadCancellation = new CancellationTokenSource();
        IsDownloading = true;
        IsProgressIndeterminate = true;
        DownloadPercentage = 0;
        StatusMessage = _languageService.Get("Downloading");

        var progress = new Progress<DownloadProgress>(value =>
        {
            IsProgressIndeterminate = value.Percentage is null;
            DownloadPercentage = value.Percentage ?? 0;
        });

        try
        {
            await _downloadService.DownloadAsync(
                SelectedVariant.Url, destination, progress, _downloadCancellation.Token);
            IsProgressIndeterminate = false;
            DownloadPercentage = 100;
            StatusMessage = _languageService.Get("DownloadComplete");
        }
        catch (OperationCanceledException) { StatusMessage = _languageService.Get("DownloadCanceled"); }
        catch { StatusMessage = _languageService.Get("DownloadFailed"); }
        finally { IsDownloading = false; }
    }

    private void OpenInBrowser()
    {
        if (SelectedVariant is null) return;
        try { _browserService.Open(SelectedVariant.Url); }
        catch { StatusMessage = _languageService.Get("OpenFailed"); }
    }

    private bool CanUseSelectedVariant() => SelectedVariant is not null && !IsDownloading;

    private void HandleCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(LocalizedMediaType));
    }

    private static string GetImageExtension(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            string extension = Path.GetExtension(uri.AbsolutePath).TrimStart('.').ToLowerInvariant();
            if (extension is "jpg" or "jpeg" or "png" or "webp") return extension;
        }
        return "jpg";
    }

    public void Dispose()
    {
        _languageService.CultureChanged -= HandleCultureChanged;
        _downloadCancellation?.Cancel();
        _downloadCancellation?.Dispose();
    }
}
