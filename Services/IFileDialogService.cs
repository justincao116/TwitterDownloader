namespace X.Desktop.Services;
public interface IFileDialogService
{
    string? ShowSaveFile(string defaultFileName, string extension);
}
