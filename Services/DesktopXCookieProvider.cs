using X.Desktop.Models;

namespace X.Desktop.Services;

public sealed class DesktopXCookieProvider(ICredentialStore credentialStore) : IXCookieProvider
{
    public XCookie? GetCookie() => XCookieParser.Parse(credentialStore.ReadCookie());
}
