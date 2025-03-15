namespace ASecureNoteMaker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            // Get the screen size
            //var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            //// Calculate a quarter of the screen size
            //var screenWidth = displayInfo.Width / displayInfo.Density;
            //var screenHeight = displayInfo.Height / displayInfo.Density;

            //var windowWidth = screenWidth / 4;
            //var windowHeight = screenHeight / 4;

            //// Set the window size
            //window.Width = windowWidth;
            //window.Height = windowHeight;

            //// Optionally, center the window
            //window.X = (screenWidth - windowWidth) / 2;
            //window.Y = (screenHeight - windowHeight) / 2;

            return window;
        }


    }
}
