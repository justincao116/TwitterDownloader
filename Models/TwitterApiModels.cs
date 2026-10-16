using System.Text.Json.Serialization;

namespace X.Desktop.Models;

internal sealed class TweetRequestEnvelope
{
    [JsonPropertyName("data")]
    public TweetResponse? Data { get; set; }
}

internal sealed class TweetResponse
{
    [JsonPropertyName("tweetResult")]
    public TweetResult? TweetResult { get; set; }
}

internal sealed class TweetResult
{
    [JsonPropertyName("result")]
    public Tweet? Result { get; set; }
}

internal sealed class Tweet
{
    [JsonPropertyName("__typename")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("legacy")]
    public TweetLegacy? Legacy { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

internal sealed class TweetLegacy
{
    [JsonPropertyName("extended_entities")]
    public TweetExtendedEntities? ExtendedEntities { get; set; }
}

internal sealed class TweetExtendedEntities
{
    [JsonPropertyName("media")]
    public List<TweetApiMedia>? Media { get; set; }
}

internal sealed class TweetApiMedia
{
    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; } = string.Empty;

    [JsonPropertyName("expanded_url")]
    public string ExpandedUrl { get; set; } = string.Empty;

    [JsonPropertyName("id_str")]
    public string IdStr { get; set; } = string.Empty;

    [JsonPropertyName("media_url_https")]
    public string MediaUrlHttps { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("video_info")]
    public TweetVideoInfo? VideoInfo { get; set; }
}

internal sealed class TweetVideoInfo
{
    [JsonPropertyName("variants")]
    public List<TweetVideoVariant> Variants { get; set; } = [];
}
