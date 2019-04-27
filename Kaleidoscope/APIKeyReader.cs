using System;
using System.Threading.Tasks;

namespace Kaleidoscope
{
    public class APIKeyReader : IAPIKeyReader
    {
        public string ReadKey()
        {
            var task = ReadFile();
            task.Wait();
            return task.Result;
        }

        private async Task<string> ReadFile()
        {
            var packageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var keyFile = await packageFolder.GetFileAsync("apiKey.txt");

            return (await Windows.Storage.FileIO.ReadTextAsync(keyFile)).Trim();
        }
    }
}