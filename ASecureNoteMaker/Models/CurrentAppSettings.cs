using System.Security.Cryptography;
using System.Text;

namespace ASecureNoteMaker.Models
{
    internal class CurrentAppSettings : IDisposable
    {
        private const string DefaultFileNamePrefix = "TodaysFile";
        private string _fileName = string.Empty;

        public string Passphrase { get; set; } = string.Empty;
        public string EncryptedFilePath { get; set; } = string.Empty;
        public string FileLocation { get; set; } = string.Empty;
        public SettingsModel Settings { get; set; } = new();

        public string FileName
        {
            get => string.IsNullOrWhiteSpace(_fileName) ? GenerateDefaultFileName() : _fileName;
            set => _fileName = string.IsNullOrWhiteSpace(value) ? GenerateDefaultFileName() : value;
        }

        public string FullLocation
        {
            get => string.IsNullOrEmpty(FileLocation) ? string.Empty : Path.Combine(FileLocation, FileName);
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

        private static string GenerateDefaultFileName() => $"{DefaultFileNamePrefix}-{DateTime.Now.ToFileTime()}.txt";

        private static void ClearValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(value);
                CryptographicOperations.ZeroMemory(keyBytes);
            }
        }

        public void Clear()
        {
            ClearValue(Passphrase);
            ClearValue(_fileName);
            ClearValue(FileLocation);

            Passphrase = string.Empty;
            _fileName = string.Empty;
            FileLocation = string.Empty;
            EncryptedFilePath = string.Empty;
        }

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}
