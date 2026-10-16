namespace X.Desktop.Services;

public enum TwitterError
{
    InvalidUrl,
    TokenUnavailable,
    TweetNotFound,
    Protected,
    CookieRequired,
    CookieRejected,
    Unavailable,
    NoMedia
}

public sealed class TwitterServiceException(TwitterError error) : Exception(error.ToString())
{
    public TwitterError Error { get; } = error;
}
