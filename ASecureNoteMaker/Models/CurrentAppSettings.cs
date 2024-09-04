using ASecureNoteMaker.Extensions;

namespace ASecureNoteMaker.Models
{
    internal class CurrentAppSettings
    {
        public string Passphrase { get; set; } = string.Empty;
        public string EncryptedFilePath { get; set; } = string.Empty;

        private string _fileName = string.Empty;
        public string FileName
        {
            get
            {
                if (_fileName.IsNullOrWhiteSpace())
                    _fileName = $"TodaysFile-{DateTime.Now.ToFileTime()}";

                return _fileName;
            }

            set
            {
                if (value.IsNullOrWhiteSpace())
                    _fileName = $"TodaysFile-{DateTime.Now.ToFileTime()}";
                else
                    _fileName = value;
            }
        }

        public string FileLocation { get; set; } = string.Empty;

        public string FullLocation
        {
            get => Path.Combine(FileLocation, FileName);

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    FileLocation = string.Empty;
                    FileName = string.Empty;
                }
                else
                {
                    FileLocation = Path.GetDirectoryName(value) ?? string.Empty;
                    FileName = Path.GetFileName(value);
                }
            }
        }
    }
}
