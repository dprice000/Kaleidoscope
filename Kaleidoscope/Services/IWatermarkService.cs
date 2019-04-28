using Windows.Storage;

namespace Kaleidoscope.Services
{
    public interface IWatermarkService
    {
        void AddTextWatermark(StorageFile photo);
    }
}