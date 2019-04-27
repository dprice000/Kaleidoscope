using Kaleidoscope.Adapters;
using Kaleidoscope.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Kaleidoscope.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComputerVisionService visionService;

        private bool watermarkEnabled;
        private bool thumbnailEnabled;
        private bool taggingEnabled;
        private bool isProcessing;
        private string photoDirPath;
        private int picturesTotal;
        private int processedCount;

        public DelegateCommand BrowseCommand { get; set; }
        public DelegateCommand ProcessFilesCommand { get; set; }

        #region Properties

        public bool WatermarkEnabled
        {
            get
            {
                return watermarkEnabled;
            }
            set
            {
                watermarkEnabled = value;
                this.OnPropertyChanged();
            }
        }

        public bool ThumbnailEnabled
        {
            get
            {
                return thumbnailEnabled;
            }
            set
            {
                thumbnailEnabled = value;
                this.OnPropertyChanged();
            }
        }

        public bool TaggingEnabled
        {
            get
            {
                return taggingEnabled;
            }
            set
            {
                taggingEnabled = value;
                this.OnPropertyChanged();
            }
        }

        public string PhotoDirPath
        {
            get
            {
                return photoDirPath;
            }
            set
            {
                photoDirPath = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                isProcessing = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(CanConfigureProcess));
                BrowseCommand.RaiseCanExecuteChanged();
                ProcessFilesCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanConfigureProcess
        {
            get
            {
                return !isProcessing;
            }
        }

        public int PicturesTotal
        {
            get
            {
                return picturesTotal;
            }
            set
            {
                picturesTotal = value;
                this.OnPropertyChanged();
            }
        }

        public int ProcessedCount
        {
            get
            {
                return processedCount;
            }
            set
            {
                processedCount = value;
                this.OnPropertyChanged();
            }
        }

        #endregion Properties

        public MainViewModel(IComputerVisionService visionService)
        {
            InitializeTitleBar();
            InitializeStatusBar();
            InitializeCommands();

            this.visionService = visionService;
        }

        private async void BrowseClick()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            //folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                PhotoDirPath = folder.Path;
            }
            else
            {
                PhotoDirPath = null;
            }
        }

        private void ProcessFilesClick()
        {
            ProcessFiles();
        }

        private async void ProcessFiles()
        {
            IsProcessing = true;

            var photoFolder = await StorageFolder.GetFolderFromPathAsync(PhotoDirPath);
            var photos = await photoFolder.GetFilesAsync();

            PicturesTotal = photos.Count;

            await Task.Run(async () =>
            {
                foreach (var photo in photos)
                {
                    if (TaggingEnabled)
                    {
                        using (FileStream stream = new FileStream(photo.Path,
                                    FileMode.Open, FileAccess.Read, FileShare.None,
                                    bufferSize: 4096, useAsync: true))
                        {
                            string desc = await visionService.GenerateDescriptionAsnyc(stream);
                            IEnumerable<string> tags = await visionService.GenerateTagsAsync(stream);

                            JpgMetadataAdapter jpeg = new JpgMetadataAdapter(photo);
                            jpeg.MetaData.Title = desc;
                            jpeg.Save();
                            jpeg.SaveTags(tags.ToList());
                        }
                    }

                    if (ThumbnailEnabled)
                    {
                            //TODO: Create Thumbnail
                        }

                    if (WatermarkEnabled)
                    {
                            //TODO: Do Watermarking
                        }

                    ProcessedCount++;
                }
            });

            IsProcessing = false;
        }

        #region Initialization Methods

        private void InitializeCommands()
        {
            BrowseCommand = new DelegateCommand(p => BrowseClick(), p => CanConfigureProcess);
            ProcessFilesCommand = new DelegateCommand(p => ProcessFilesClick(), p => CanConfigureProcess);
        }

        private void InitializeTitleBar()
        {
            //PC customization
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = Colors.DarkSlateBlue;
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.BackgroundColor = Colors.DarkBlue;
                    titleBar.ForegroundColor = Colors.White;
                }
            }
        }

        private void InitializeStatusBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = Colors.DarkBlue;
                    statusBar.ForegroundColor = Colors.White;
                }
            }
        }

        #endregion Initialization Methods
    }
}