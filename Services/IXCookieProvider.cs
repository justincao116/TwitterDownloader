using X.Desktop.Models;

namespace X.Desktop.Services;

public interface IXCookieProvider
{
    XCookie? GetCookie();
}
