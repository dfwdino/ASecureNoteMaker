using ASecureNoteMaker.Extensions;
using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;


namespace ASecureNoteMaker
{
    public partial class MainPage : ContentPage
    {

        CurrentAppSettings _CurrentAppSettings = new();

        private IDispatcherTimer autoSaveTimer;

        // Base Functions
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoaded;

        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {

        }

        private void AutoSave_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                // Start auto-save
                autoSaveTimer = Dispatcher.CreateTimer();
                autoSaveTimer.Interval = TimeSpan.FromSeconds(30);
                autoSaveTimer.Tick += (s, e) => SaveText_ClickedAsync(this, EventArgs.Empty);
                autoSaveTimer.Start();
            }
            else
            {
                // Stop auto-save
                autoSaveTimer?.Stop();
            }

        }


        private async void OpenFile_Clicked(object sender, EventArgs e)
        {
            if (Note.Text.Length > 0)
            {
                bool NewFile = await DisplayAlert("Confirmation", "Are you sure you want to start a new file?", "Yes", "No");

                if (NewFile.Equals(false))

                { return; }

            }


            #region Blank Values. Maybe Save them?

            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace() && _CurrentAppSettings.EncryptedFilePath.IsNullOrWhiteSpace())
            {
                bool SaveData = await DisplayAlert("Confirmation", "Want to save current text?", "Yes", "No");

                var PassphraseLogicTask = PassphraseLogic();

                await PassphraseLogicTask;


                FileSaverResult fileSaverResult;

                try
                {
                    fileSaverResult = await FileSaver.Default.SaveAsync(_CurrentAppSettings.FileName, new MemoryStream(), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Saving File Issue", $"Can't save file b/c {ex.Message}", "OK");
                    return;
                }


                _CurrentAppSettings.EncryptedFilePath = fileSaverResult.FilePath;

                if (!_CurrentAppSettings.EncryptedFilePath.IsNullOrWhiteSpace())
                {
                    DisplayAlert("File Selected", "No file is selected.", "Ok");
                    return;
                }

            }
            else
            {
                FilEncryption.EncryptFile(Note.Text, _CurrentAppSettings.EncryptedFilePath, _CurrentAppSettings.Passphrase);

                _CurrentAppSettings.Passphrase = string.Empty;
            }

            #endregion

            #region Loading New File
            await PassphraseLogic();

            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace())
            { return; }

            var result = await FilePicker.PickAsync();

            try
            {
                string decrypttext = FilEncryption.DecryptFile(result.FullPath, _CurrentAppSettings.Passphrase);

                if (decrypttext.IsNullOrWhiteSpace())
                {
                    await DisplayAlert("Blank File", "The file is not encypted or blank.", "OK");
                    return;
                }

                Note.Text = FilEncryption.DecryptFile(result.FullPath, _CurrentAppSettings.Passphrase);


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }

            #endregion

            return;
        }

        private async Task PassphraseLogic(string filename = "")
        {
            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace())
            {
                _CurrentAppSettings.Passphrase = await DisplayPromptAsync("Input", $"Please enter some text for the passphrase for the file: {filename}");
                Task.WaitAll();
            }

            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace())
            {
                await DisplayAlert("Blank value", "Can't have blank text in the password", "Ok");
                Task.WaitAll();
            }

            return;
        }

        private void AutoSaverTimerStopClear()
        {
            autoSaveTimer?.Stop();
            AutoSave.IsChecked = false;
        }

        private async Task SaveText_ClickedAsync(object sender, EventArgs e)
        {
            await PassphraseLogic();
            Task.WaitAll();

            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace())
            {
                AutoSaverTimerStopClear();
                return;
            }

            if (_CurrentAppSettings.FileLocation.IsNullOrWhiteSpace())
            {
                var fileSaverResult = await FileSaver.Default.SaveAsync(_CurrentAppSettings.FileName, new MemoryStream(), CancellationToken.None);

                if (fileSaverResult.FilePath.IsNullOrWhiteSpace())
                {
                    await DisplayAlert("Blank value", "No locatoin found or used.", "Ok");
                    AutoSaverTimerStopClear();
                    return;
                }
                else
                {
                    _CurrentAppSettings.FullLocation = fileSaverResult.FilePath;
                }
            }

            FilEncryption.EncryptFile(Note.Text, _CurrentAppSettings.FileLocation, _CurrentAppSettings.Passphrase);

            MainPageStatus.Text = $"Note Saved in {_CurrentAppSettings.FileLocation}";

            return;
        }

        private async void NewFile_Clicked(object sender, EventArgs e)
        {
            bool NewFile = await DisplayAlert("Confirmation", "Are you sure you want to start a new file?", "Yes", "No");

            if (NewFile)
            {
                Note.Text = string.Empty;
            }
        }

        private async void Exit_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Close Program", "Do you want to close the program?", "Yes", "No");
            if (answer)
            {
                // Close the application
                Application.Current.Quit();
            }
        }

        private void Settings_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingsPage());
        }

        private async void SaveText_Clicked(object sender, EventArgs e)
        {
            await SaveText_ClickedAsync(null, null);
        }
    }








}
