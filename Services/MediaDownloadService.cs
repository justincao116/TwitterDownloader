using System.Buffers;
using System.Net.Http;
using X.Desktop.Models;

namespace X.Desktop.Services;

public sealed class MediaDownloadService(HttpClient httpClient) : IMediaDownloadService
{
    public async Task DownloadAsync(string sourceUrl, string destinationPath,
        IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
    {
        string temporaryPath = $"{destinationPath}.{Guid.NewGuid():N}.part";
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, sourceUrl);
            request.Headers.TryAddWithoutValidation("user-agent", TwitterRequestConstants.UserAgent);
            using var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? contentLength = response.Content.Headers.ContentLength;
            await using Stream input = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
            long totalRead = 0;
            try
            {
                while (true)
                {
                    int read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                    if (read == 0) break;
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    totalRead += read;
                    double? percentage = contentLength > 0 ? totalRead * 100d / contentLength.Value : null;
                    progress.Report(new DownloadProgress(percentage, totalRead));
                }
            }
            finally { ArrayPool<byte>.Shared.Return(buffer); }

            await output.FlushAsync(cancellationToken);
            output.Close();
            File.Move(temporaryPath, destinationPath, true);
        }
        catch
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            throw;
        }
    }
}
