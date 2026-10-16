using System.Text.Json;
using X.Desktop.Models;

namespace X.Desktop.Services;

public sealed class TwitterService(
    HttpClient httpClient,
    IXCookieProvider cookieProvider) : ITwitterService
{
    private static readonly string[] ValidTweetTypes = ["Tweet", "TweetWithVisibilityResults"];

    public async Task<TweetMediasResponse> ListTweetMediasAsync(
        TweetMediasRequest request,
        CancellationToken cancellationToken = default)
    {
        string? tweetId = TwitterRequestConstants.GetTweetId(request.Url);
        if (string.IsNullOrWhiteSpace(tweetId))
            throw new TwitterServiceException(TwitterError.InvalidUrl);

        string? token = await RequestGuestTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            throw new TwitterServiceException(TwitterError.TokenUnavailable);

        TweetResponse? response = await RequestTweetAsync(tweetId, token, cancellationToken);
        if (response?.TweetResult?.Result is null)
            throw new TwitterServiceException(TwitterError.TweetNotFound);

        Tweet tweet = response.TweetResult.Result;
        if (tweet.TypeName == "TweetUnavailable")
        {
            if (tweet.Reason == "Protected")
                throw new TwitterServiceException(TwitterError.Protected);

            if (tweet.Reason == "NsfwLoggedOut")
            {
                XCookie? cookie = cookieProvider.GetCookie();
                if (cookie is null)
                    throw new TwitterServiceException(TwitterError.CookieRequired);

                try
                {
                    response = await RequestTweetAsync(tweetId, cookie, cancellationToken);
                }
                catch (HttpRequestException exception) when (
                    exception.StatusCode is System.Net.HttpStatusCode.Unauthorized
                        or System.Net.HttpStatusCode.Forbidden)
                {
                    throw new TwitterServiceException(TwitterError.CookieRejected);
                }

                if (response?.TweetResult?.Result is null)
                    throw new TwitterServiceException(TwitterError.TweetNotFound);
                tweet = response.TweetResult.Result;
            }
        }

        if (!ValidTweetTypes.Contains(tweet.TypeName))
            throw new TwitterServiceException(TwitterError.Unavailable);

        List<TweetApiMedia>? mediaList = tweet.Legacy?.ExtendedEntities?.Media;
        if (mediaList is null || mediaList.Count == 0)
            throw new TwitterServiceException(TwitterError.NoMedia);

        return new TweetMediasResponse
        {
            Medias = mediaList.Select(media => new TweetMediaDto
            {
                DisplayUrl = media.DisplayUrl,
                ExpandedUrl = media.ExpandedUrl,
                IdStr = media.IdStr,
                MediaUrlHttps = media.MediaUrlHttps,
                Type = media.Type,
                VideoVariants = media.VideoInfo?.Variants ?? []
            }).ToList()
        };
    }

    private async Task<string?> RequestGuestTokenAsync(CancellationToken cancellationToken)
    {
        using var request = TwitterRequestConstants.ApplyHeaders(
            new HttpRequestMessage(HttpMethod.Post, TwitterRequestConstants.TokenUrl));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<JsonElement>(json)
            .GetProperty("guest_token")
            .GetString();
    }

    private async Task<TweetResponse?> RequestTweetAsync(
        string tweetId,
        string guestToken,
        CancellationToken cancellationToken)
    {
        using var request = TwitterRequestConstants.ApplyHeaders(
            new HttpRequestMessage(HttpMethod.Get, BuildRequestUri(tweetId)));
        request.Headers.TryAddWithoutValidation("x-guest-token", guestToken);
        request.Headers.TryAddWithoutValidation(
            "cookie",
            $"guest_id={Uri.EscapeDataString($"v1:{guestToken}")}");

        return await SendTweetRequestAsync(request, cancellationToken);
    }

    private async Task<TweetResponse?> RequestTweetAsync(
        string tweetId,
        XCookie cookie,
        CancellationToken cancellationToken)
    {
        using var request = TwitterRequestConstants.ApplyHeaders(
            new HttpRequestMessage(HttpMethod.Get, BuildRequestUri(tweetId)));
        request.Headers.TryAddWithoutValidation("X-Twitter-Auth-Type", "OAuth2Session");
        request.Headers.TryAddWithoutValidation("x-csrf-token", cookie.Ct0);
        request.Headers.TryAddWithoutValidation("cookie", cookie.CookieString);

        return await SendTweetRequestAsync(request, cancellationToken);
    }

    private async Task<TweetResponse?> SendTweetRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TweetRequestEnvelope>(json)?.Data;
    }

    private static Uri BuildRequestUri(string tweetId)
    {
        return new UriBuilder(TwitterRequestConstants.GraphqlUrl)
        {
            Query =
                $"variables={Uri.EscapeDataString(TwitterRequestConstants.GetVariables(tweetId))}&" +
                $"features={Uri.EscapeDataString(TwitterRequestConstants.Features)}&" +
                $"fieldToggles={Uri.EscapeDataString(TwitterRequestConstants.FieldToggles)}"
        }.Uri;
    }
}
