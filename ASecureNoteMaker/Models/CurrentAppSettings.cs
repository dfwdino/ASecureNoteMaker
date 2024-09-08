﻿using ASecureNoteMaker.Extensions;

namespace ASecureNoteMaker.Models
{
    internal class CurrentAppSettings
    {

        #region Private Variables 

        private string _defaultFileNameValue = $"TodaysFile-{DateTime.Now.ToFileTime()}.txt";

        private string _fileName = string.Empty;

        #endregion End Private Variables


        #region Public Variables

        public string Passphrase { get; set; } = string.Empty;
        public string EncryptedFilePath { get; set; } = string.Empty;
        public string FileLocation { get; set; } = string.Empty;

        #endregion End Public Variables



        public string FileName
        {
            get
            {
                if (_fileName.IsNullOrWhiteSpace())
                    _fileName = _defaultFileNameValue;

                return _fileName;
            }

            set
            {
                if (value.IsNullOrWhiteSpace())
                    _fileName = _defaultFileNameValue;
                else
                    _fileName = value;
            }
        }



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
