using Microsoft.Win32;

namespace X.Desktop.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? ShowSaveFile(string defaultFileName, string extension)
    {
        string ext = extension.TrimStart('.');
        var dialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            DefaultExt = "." + ext,
            AddExtension = true,
            OverwritePrompt = true,
            Filter = ext.Equals("mp4", StringComparison.OrdinalIgnoreCase)
                ? "MP4 video (*.mp4)|*.mp4|All files (*.*)|*.*"
                : "Image files|*.jpg;*.jpeg;*.png;*.webp|All files (*.*)|*.*"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
