using X.Desktop.Models;
namespace X.Desktop.Services;
public interface IMediaDownloadService
{
    Task DownloadAsync(string sourceUrl, string destinationPath,
        IProgress<DownloadProgress> progress, CancellationToken cancellationToken);
}
