using System.Threading.Tasks;
using Windows.Storage;

namespace Kaleidoscope.Controllers
{
    public interface IComputerVisionController
    {
        Task TagFile(StorageFolder processedFolder, StorageFile photo, bool autoGenerateTitle = false);

        Task CreateThumbnail(StorageFolder processedFolder, StorageFile photo);
    }
}