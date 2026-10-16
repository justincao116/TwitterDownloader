using X.Desktop.Models;

namespace X.Desktop.Services;

public static class XCookieParser
{
    public static XCookie? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string? ct0 = value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(claim => claim.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2)
            .FirstOrDefault(parts => parts[0].Equals("ct0", StringComparison.OrdinalIgnoreCase))?[1];

        return string.IsNullOrWhiteSpace(ct0)
            ? null
            : new XCookie { CookieString = value.Trim(), Ct0 = ct0 };
    }
}
