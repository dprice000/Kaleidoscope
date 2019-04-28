using Kaleidoscope.Controllers;
using Kaleidoscope.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Kaleidoscope.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IComputerVisionController visionController;
        private Services.IWatermarkService watermarkService;
        private Services.IToastService toastService;
        private Services.IMessageBoxService messageBoxService;

        private bool watermarkEnabled;
        private bool thumbnailEnabled;
        private bool smartTitleEnabled;
        private bool taggingEnabled;
        private bool isProcessing;
        private string photoDirPath;
        private int picturesTotal;
        private int processedCount;
        private bool isInForeGround;
        private string applicationName = "Kaleidocope";

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

                if(!taggingEnabled)
                {
                    SmartTitleEnabled = false;
                }

                this.OnPropertyChanged();
            }
        }

        public bool SmartTitleEnabled
        {
            get
            {
                return smartTitleEnabled;
            }
            set
            {
                smartTitleEnabled = value;
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

        public bool IsInForeground
        {
            get
            {
                return isInForeGround;
            }
            set
            {
                isInForeGround = value;
                this.OnPropertyChanged();
            }
        }

        #endregion Properties

        public MainViewModel(IComputerVisionController visionController, Services.IWatermarkService watermarkService, Services.IToastService toastService, Services.IMessageBoxService messageBoxService)
        {
            InitializeTitleBar();
            InitializeStatusBar();
            InitializeCommands();

            this.visionController = visionController;
            this.watermarkService = watermarkService;
            this.toastService = toastService;
            this.messageBoxService = messageBoxService;
        }

        private async void BrowseClick()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
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
            IsProcessing = true;

            var progress = new Progress<ProcessFilesProgress>();
            progress.ProgressChanged += Progress_ProgressChanged;
            ProcessFilesAsync(progress);
        }

        private void Progress_ProgressChanged(object sender, ProcessFilesProgress e)
        {
            PicturesTotal = e.MaximumPictures;
            ProcessedCount = e.ProcessedCount;
        }

        private async void ProcessFilesAsync(IProgress<ProcessFilesProgress> progress)
        {
            string processedDirPath = Path.Combine(PhotoDirPath, applicationName);
            StorageFolder processedFolder = await CreateFolder(PhotoDirPath, applicationName);

            var photos = await GetPhotosAsync(PhotoDirPath);
            int processedCount = 0;

            foreach (var photo in photos)
            {
                progress?.Report(new ProcessFilesProgress()
                {
                    MaximumPictures = photos.Count(),
                    ProcessedCount = processedCount
                });

                if (TaggingEnabled)
                {
                    await visionController.TagFile(processedFolder, photo, SmartTitleEnabled);
                }

                if (ThumbnailEnabled)
                {
                    await visionController.CreateThumbnail(processedFolder, photo);
                }

                if (WatermarkEnabled)
                {
                    watermarkService.AddTextWatermark(photo);
                }

                processedCount++;
            }

            IsProcessing = false;

            await messageBoxService.ShowMessageAsync("Processing Complete", "Done processing images.");

            if (!IsInForeground)
            {
                toastService.ShowToastNotification("Processing Complete", "Done processing images.");
            }
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

        private async Task<StorageFolder> CreateFolder(string baseFolderPath, string subFolderName)
        {
                StorageFolder baseFolder = await StorageFolder.GetFolderFromPathAsync(baseFolderPath);
                return await baseFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.OpenIfExists);
        }

        private async Task<IReadOnlyList<StorageFile>> GetPhotosAsync(string photoDirPath)
        {
            var photoFolder = await StorageFolder.GetFolderFromPathAsync(photoDirPath);
            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);
            photoFolder.CreateFileQueryWithOptions(queryOptions);
            return await photoFolder.GetFilesAsync();
        }
    }
}