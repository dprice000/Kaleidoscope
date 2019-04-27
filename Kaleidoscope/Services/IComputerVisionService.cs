using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Kaleidoscope.Services
{
    public interface IComputerVisionService
    {
        Task<string> GenerateDescriptionAsnyc(FileStream stream);

        Task<IEnumerable<string>> GenerateTagsAsync(FileStream stream);
    }
}