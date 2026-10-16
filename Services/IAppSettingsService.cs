using X.Desktop.Models;
namespace X.Desktop.Services;
public interface IAppSettingsService { AppSettings Load(); void Save(AppSettings settings); }
