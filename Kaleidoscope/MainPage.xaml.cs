using Kaleidoscope.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Kaleidoscope
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = new MainViewModel(new Controllers.ComputerVisionController(new Services.ComputerVisionService(new APIKeyReader())), new Services.WatermarkService(), new Services.ToastService(), new Services.MessageBoxService());

            Window.Current.Activated += CurrentWindow_Activated;
        }

        private void CurrentWindow_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                ((MainViewModel)this.DataContext).IsInForeground = false;
            }
            else
            {
                ((MainViewModel)this.DataContext).IsInForeground = true;
            }
        }
    }
}