using Avalonia.Platform.Storage;
using System.IO;
using System.Threading.Tasks;
using VTACheckClock.Services.Libs;

namespace VTACheckClock.Services
{
    public class FolderPickerService
    {
        public static async Task<IStorageFolder?> GetStartLocationAsync(IStorageProvider storageProvider, string? PathTmp)
        {
            if (Directory.Exists(PathTmp))
            {
                return await storageProvider.TryGetFolderFromPathAsync(PathTmp);
            }
            else
            {
                return await storageProvider.TryGetFolderFromPathAsync(GlobalVars.DefWorkPath);
            }
        }
    }
}
