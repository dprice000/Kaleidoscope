using Kaleidoscope.Models;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Kaleidoscope
{
    public sealed class ConfigManager
    {
        private static ConfigManager instance = null;
        private readonly ConfigDocument configDocument;
        private static readonly object padlock = new object();

        private ConfigManager()
        {
            var task = ReadFile();
            task.Wait();

            configDocument = JsonConvert.DeserializeObject<ConfigDocument>(task.Result);
        }

        public static ConfigManager Settings
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ConfigManager();
                    }

                    return instance;
                }
            }
        }

        public T GetValue<T>(string propertyName)
        {
            return (T)configDocument.GetType().GetProperty(propertyName).GetValue(configDocument);
        }

        public string GetValue(string propertyName)
        {
            return configDocument.GetType().GetProperty(propertyName).GetValue(configDocument).ToString();
        }

        private async Task<string> ReadFile()
        {
            var packageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var configFile = await packageFolder.GetFileAsync("config.json").AsTask().ConfigureAwait(false);
            return await Windows.Storage.FileIO.ReadTextAsync(configFile);
        }
    }
}