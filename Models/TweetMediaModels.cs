using System.Text.Json.Serialization;

namespace X.Desktop.Models;

public sealed class TweetMediasRequest
{
    public string Url { get; set; } = string.Empty;
}

public sealed class TweetMediasResponse
{
    public List<TweetMediaDto> Medias { get; set; } = [];
}

public sealed class TweetMediaDto
{
    public string DisplayUrl { get; set; } = string.Empty;
    public string ExpandedUrl { get; set; } = string.Empty;
    public string IdStr { get; set; } = string.Empty;
    public string MediaUrlHttps { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<TweetVideoVariant> VideoVariants { get; set; } = [];
}

public sealed class TweetVideoVariant
{
    [JsonPropertyName("bitrate")]
    public int? Bitrate { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public sealed class XCookie
{
    public string CookieString { get; set; } = string.Empty;
    public string Ct0 { get; set; } = string.Empty;
}
