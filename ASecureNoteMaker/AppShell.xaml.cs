using System.Reflection;

namespace ASecureNoteMaker
{
    public partial class AppShell : Shell
    {
        // Get the current version number
        string version = VersionTracking.CurrentVersion;

        // Get the current build number
        string build = VersionTracking.CurrentBuild;

        string appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public AppShell()
        {
            InitializeComponent();

            this.Title = $"{Assembly.GetExecutingAssembly().GetName().Name} - {appVersion}";
        }



    }
}
