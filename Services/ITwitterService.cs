using X.Desktop.Models;

namespace X.Desktop.Services;

public interface ITwitterService
{
    Task<TweetMediasResponse> ListTweetMediasAsync(
        TweetMediasRequest request,
        CancellationToken cancellationToken = default);
}
