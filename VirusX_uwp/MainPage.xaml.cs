using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VirusX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        readonly VirusX game;

        public GamePage()
        {
            this.InitializeComponent();

            // Size restrictions.
            var currentView = ApplicationView.GetForCurrentView();
            currentView.SetPreferredMinSize(new Size(Settings.MINIMUM_SCREEN_WIDTH, Settings.MINIMUM_SCREEN_HEIGHT));

            // Create the game.
            System.Diagnostics.Debug.WriteLine("Launching game...");
            var launchArguments = string.Empty;
            game = MonoGame.Framework.XamlGame<VirusX>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }
    }
}
