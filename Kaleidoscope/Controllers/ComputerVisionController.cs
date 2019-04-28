using Kaleidoscope.Adapters;
using Kaleidoscope.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Kaleidoscope.Controllers
{
    public class ComputerVisionController : IComputerVisionController
    {
        private IComputerVisionService visionService;

        public ComputerVisionController(IComputerVisionService visionService)
        {
            this.visionService = visionService;
        }

        public async Task TagFile(StorageFolder processedFolder, StorageFile photo, bool autoGenerateTitle = false)
        {
            await Task.Run(async () =>
            {
                var processedPhoto = await photo.CopyAsync(processedFolder, photo.Name, NameCollisionOption.ReplaceExisting);

                if (autoGenerateTitle)
                {
                    await AddGeneratedTitleToPhoto(processedPhoto);
                }

                //await AddGeneratedTagsToPhoto(processedPhoto);
            });
        }

        public async Task CreateThumbnail(StorageFolder processedFolder, StorageFile photo)
        {
            await Task.Run(async () =>
            {
                using (FileStream stream = new FileStream(photo.Path,
                        FileMode.Open, FileAccess.Read, FileShare.None,
                        bufferSize: 4096, useAsync: true))
                {
                    using (var thumbnailStream = await visionService.GenerateThumbnailAsync(stream))
                    {
                        var thumbnailPath = Path.Combine(processedFolder.Path, $"{photo.DisplayName}_thumbnail.jpg");

                        using (var outputStream = File.Open(thumbnailPath, FileMode.Create, FileAccess.Write))
                        {
                            thumbnailStream.CopyTo(outputStream);
                        }
                    }
                }
            });
        }

        private async Task AddGeneratedTitleToPhoto(StorageFile photo)
        {
            JpgMetadataAdapter jpeg = new JpgMetadataAdapter(photo);

            using (FileStream stream = new FileStream(photo.Path,
                FileMode.Open, FileAccess.Read, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                jpeg.MetaData.Title = await visionService.GenerateDescriptionAsnyc(stream);
                await jpeg.SaveAsync();
            }
        }

        private async Task AddGeneratedTagsToPhoto(StorageFile photo)
        {
            JpgMetadataAdapter jpeg = new JpgMetadataAdapter(photo);

            using (FileStream stream = new FileStream(photo.Path,
                FileMode.Open, FileAccess.Read, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                IEnumerable<string> tags = await visionService.GenerateTagsAsync(stream);
                await jpeg.SaveTags(tags);
            }
        }
    }
}