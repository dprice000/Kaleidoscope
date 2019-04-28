using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kaleidoscope.Services
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly string apiKey;
        private ComputerVisionClient visionClient;
        private readonly string serviceEndpoint;

        public ComputerVisionService(IAPIKeyReader keyReader)
        {
            apiKey = keyReader.ReadKey();
            serviceEndpoint = ConfigManager.Settings.GetValue("VisionEndpoint");

            visionClient = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(apiKey),
                new System.Net.Http.DelegatingHandler[] { }
            );

            visionClient.Endpoint = serviceEndpoint;
        }

        public async Task<string> GenerateDescriptionAsnyc(FileStream fileStream)
        {
            VisualFeatureTypes[] features = new VisualFeatureTypes[] { VisualFeatureTypes.Description };
            var results = await visionClient.AnalyzeImageInStreamAsync(fileStream, features);

            return results.Description.Captions?[0].Text;
        }

        public async Task<IEnumerable<string>> GenerateTagsAsync(FileStream fileStream)
        {
            VisualFeatureTypes[] features = new VisualFeatureTypes[] { VisualFeatureTypes.Tags };
            var results = await visionClient.AnalyzeImageInStreamAsync(fileStream, features);

            return results.Tags.Select(x => x.Name);
        }

        public async Task<Stream> GenerateThumbnailAsync(FileStream fileStream)
        {
            return await visionClient.GenerateThumbnailInStreamAsync(180, 180, fileStream, true);
        }
    }
}