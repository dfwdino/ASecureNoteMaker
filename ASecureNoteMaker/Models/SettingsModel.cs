using static ASecureNoteMaker.Extensions.StringExtensions;

namespace ASecureNoteMaker.Models
{
    public class SettingsModel
    {
        public readonly string DefaultAutoSaveTimeSeconds;

        private string _AutoSaveTimeSeconds { get; set; }

        public SettingsModel()
        {
            DefaultAutoSaveTimeSeconds = "30";
            _AutoSaveTimeSeconds = DefaultAutoSaveTimeSeconds;
        }

      

        public string DefaultFileLocation { get; set; } = string.Empty;
        public string AutoSaveTimeSeconds {

            get => _AutoSaveTimeSeconds; 
            
            set {
                if (value.Equals("0") || value.IsNullOrWhiteSpace())
                    _AutoSaveTimeSeconds = DefaultAutoSaveTimeSeconds;
                else
                    _AutoSaveTimeSeconds = value;
            } 
        } 
    }
}
