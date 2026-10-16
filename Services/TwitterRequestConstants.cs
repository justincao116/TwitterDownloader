using System.Text.Json;
using System.Text.RegularExpressions;

namespace X.Desktop.Services;

internal static partial class TwitterRequestConstants
{
    public const string GraphqlUrl =
        "https://api.x.com/graphql/I9GDzyCGZL2wSoYFFrrTVw/TweetResultByRestId";
    public const string TokenUrl = "https://api.x.com/1.1/guest/activate.json";
    public const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36";

    public static readonly string Features = JsonSerializer.Serialize(new
    {
        creator_subscriptions_tweet_preview_api_enabled = true,
        communities_web_enable_tweet_community_results_fetch = true,
        c9s_tweet_anatomy_moderator_badge_enabled = true,
        articles_preview_enabled = true,
        tweetypie_unmention_optimization_enabled = true,
        responsive_web_edit_tweet_api_enabled = true,
        graphql_is_translatable_rweb_tweet_is_translatable_enabled = true,
        view_counts_everywhere_api_enabled = true,
        longform_notetweets_consumption_enabled = true,
        responsive_web_twitter_article_tweet_consumption_enabled = true,
        tweet_awards_web_tipping_enabled = false,
        creator_subscriptions_quote_tweet_preview_enabled = false,
        freedom_of_speech_not_reach_fetch_enabled = true,
        standardized_nudges_misinfo = true,
        tweet_with_visibility_results_prefer_gql_limited_actions_policy_enabled = true,
        rweb_video_timestamps_enabled = true,
        longform_notetweets_rich_text_read_enabled = true,
        longform_notetweets_inline_media_enabled = true,
        rweb_tipjar_consumption_enabled = true,
        responsive_web_graphql_exclude_directive_enabled = true,
        verified_phone_label_enabled = false,
        responsive_web_graphql_skip_user_profile_image_extensions_enabled = false,
        responsive_web_graphql_timeline_navigation_enabled = true,
        responsive_web_enhance_cards_enabled = false
    });

    public static readonly string FieldToggles = JsonSerializer.Serialize(new
    {
        withArticleRichContentState = true,
        withArticlePlainText = false,
        withGrokAnalyze = false
    });

    public static string GetVariables(string tweetId) => JsonSerializer.Serialize(new
    {
        tweetId,
        withCommunity = false,
        includePromotedContent = false,
        withVoice = false
    });

    public static HttpRequestMessage ApplyHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("user-agent", UserAgent);
        request.Headers.TryAddWithoutValidation(
            "authorization",
            "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D" +
            "1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");
        request.Headers.TryAddWithoutValidation("x-twitter-client-language", "en");
        request.Headers.TryAddWithoutValidation("x-twitter-active-user", "yes");
        request.Headers.TryAddWithoutValidation("accept-language", "en");
        request.Headers.TryAddWithoutValidation("content-type", "application/json");
        return request;
    }

    public static string? GetTweetId(string url)
    {
        var match = TweetUrlRegex().Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex(
        @"(?:x|twitter)\.com\/[^\/]+\/status\/(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TweetUrlRegex();
}
