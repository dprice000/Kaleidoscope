using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Kaleidoscope.Adapters
{
    public class JpgMetadataAdapter
    {
        private StorageFile photo;
        public readonly ImageProperties MetaData;

        public JpgMetadataAdapter(StorageFile photo)
        {
            this.photo = photo;
            var task = ReadProperties();
            task.Wait();

            MetaData = task.Result;          
        }

        public async Task SaveAsync()
        {
            await MetaData.SavePropertiesAsync();
        }

        public async Task SaveTags(IEnumerable<string> tags)
        {
            List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
            var t = new BitmapTypedValue(string.Join(",", tags), Windows.Foundation.PropertyType.String);
            list.Add(new KeyValuePair<string, object>(SystemProperties.Comment, t));
            await MetaData.SavePropertiesAsync(list.AsEnumerable());
        }

        private async Task<ImageProperties> ReadProperties()
        {
            return await photo.Properties.GetImagePropertiesAsync();
        }
    }
}