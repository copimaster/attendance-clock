using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.IO;
using System.Threading.Tasks;
using VTACheckClock.Services.Libs;

namespace VTACheckClock.Services
{
    public interface IFilePickerService
    {
        Task<IStorageFile?> OpenFilePickerAsync();
    }

    public class FilePickerService : IFilePickerService
    {
        public async Task<IStorageFile?> OpenFilePickerAsync()
        {
            var MainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            var windows = MainWindow?.OwnedWindows;

            IStorageProvider? provider = MainWindow?.StorageProvider;

            if (windows?.Count > 0) {
                provider = windows[0].StorageProvider;
            }

            if(provider == null) return null;

            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions() {
                Title = "Seleccionar archivo de conexión",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[] {
                    new FilePickerFileType("Archivos Hash") { Patterns = new string[] {"*.hash" } }
                }
            });

            return files?.Count >= 1 ? files[0] : null;
        }
    }
}
