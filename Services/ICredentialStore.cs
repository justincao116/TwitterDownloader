namespace X.Desktop.Services;
public interface ICredentialStore
{
    string? ReadCookie();
    void SaveCookie(string cookie);
    void DeleteCookie();
}
