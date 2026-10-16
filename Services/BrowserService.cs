using System.Diagnostics;
namespace X.Desktop.Services;
public sealed class BrowserService : IBrowserService
{
    public void Open(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}
