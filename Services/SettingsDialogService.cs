using System.Windows;
using X.Desktop.ViewModels;
using X.Desktop.Views;
namespace X.Desktop.Services;
public sealed class SettingsDialogService(ICredentialStore credentialStore, ILanguageService languageService)
    : ISettingsDialogService
{
    public void Show()
    {
        var window = new SettingsWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            DataContext = new SettingsViewModel(credentialStore, languageService)
        };
        window.ShowDialog();
    }
}
