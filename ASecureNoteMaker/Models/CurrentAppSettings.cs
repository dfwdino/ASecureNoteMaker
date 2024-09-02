using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASecureNoteMaker.Models
{
    internal class CurrentAppSettings
    {
        public string Passphrase = string.Empty;
        public string EncryptedFilePath = string.Empty;
        public string FileName = $"TodaysFile-{DateTime.Now.ToFileTime()}";
        public string FileLocation = string.Empty;

public string FullLocation
    {
        get { return Path.Combine(FolderLocation, FileName); }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                FolderLocation = Path.GetDirectoryName(value);
                FileName = Path.GetFileName(value);
            }
        }
    }

    }
}
