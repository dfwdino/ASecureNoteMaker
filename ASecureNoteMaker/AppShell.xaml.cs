using System.Reflection;

namespace ASecureNoteMaker
{
    public partial class AppShell : Shell
    {
        // Get the current version number
        //string version = VersionTracking.CurrentVersion;

        //// Get the current build number
        //string build = VersionTracking.CurrentBuild;

        public AppShell()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();

            string? appVersion = assembly?.GetName()?.Version?.ToString();
            
            var TitleAttribute = assembly?.GetCustomAttribute<AssemblyTitleAttribute>();

            this.Title = $"{(TitleAttribute == null ? Assembly.GetExecutingAssembly().GetName().Name : TitleAttribute.Title)} - {appVersion}";
        }



    }
}
