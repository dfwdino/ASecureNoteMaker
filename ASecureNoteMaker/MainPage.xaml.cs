using ASecureNoteMaker.Extensions;
using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;
using System.Text.Json;



namespace ASecureNoteMaker
{
    public partial class MainPage : ContentPage
    {

        CurrentAppSettings _CurrentAppSettings = new();
        SettingsModel _SettingsModel = new SettingsModel();
        private string _SettingsFileFullLocation = string.Empty;

        private IDispatcherTimer autoSaveTimer;

        // Base Functions
        public MainPage()
        {
            InitializeComponent();

            _SettingsFileFullLocation = Path.Combine(FileSystem.AppDataDirectory, "Settings.json");

            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            if (File.Exists(_SettingsFileFullLocation).Equals(false))
            {
                return;
            }

           
            string jsonString = string.Empty;

            jsonString = File.ReadAllText(_SettingsFileFullLocation);

            _SettingsModel = JsonSerializer.Deserialize<SettingsModel>(jsonString);


            if(_SettingsModel.DefaultFileLocation.IsNullOrWhiteSpace().Equals(true) ||
                    File.Exists(_SettingsModel.DefaultFileLocation).Equals(false)) 

            {
                MainPageStatus.Text = $"File {_SettingsModel.DefaultFileLocation} can't be found.";
                return; 
            }


            await PassphraseLogic();

            var result = await FilePicker.PickAsync();

            try
            {
                string decrypttext = FilEncryption.DecryptFile(_SettingsModel.DefaultFileLocation, _CurrentAppSettings.Passphrase);

                if (decrypttext.IsNullOrWhiteSpace())
                {
                    await DisplayAlert("Blank File", "The file is not encypted, blank or wrong passcode.", "OK");
                    return;
                }

                Note.Text = FilEncryption.DecryptFile(result.FullPath, _CurrentAppSettings.Passphrase);

                lblFileName.Text = $"Current File is " + Path.GetFileName(result.FullPath);


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }




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
                bool NewFile = await DisplayAlert("Confirmation", "Are you sure you want to start a new file?  Make sure you update has been saved.", "Yes", "No");

                if (NewFile.Equals(false))
                {
                    return;
                }
                
                
            bool SaveFile = await DisplayAlert("Confirmation", "You want to save this file?", "Yes", "No");

            if (SaveFile.Equals(true))
            {
                if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace() && _CurrentAppSettings.EncryptedFilePath.IsNullOrWhiteSpace())
                {
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



                    if (_CurrentAppSettings.EncryptedFilePath.IsNullOrWhiteSpace())
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
            }

            


            }


            #region Loading New File

            //Clear out any new values if they are saved.
            ClearOutStoredValues();
                       

            var result = await FilePicker.PickAsync();

            await PassphraseLogic();

            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace())
            { return; }

            try
            {
                string decrypttext = FilEncryption.DecryptFile(result.FullPath, _CurrentAppSettings.Passphrase);

                if (decrypttext.IsNullOrWhiteSpace())
                {
                    await DisplayAlert("Blank File", "The file is not encypted, blank or wrong passcode.", "OK");
                    return;
                }

                Note.Text = FilEncryption.DecryptFile(result.FullPath, _CurrentAppSettings.Passphrase);

                lblFileName.Text = $"Current File is " + Path.GetFileName(result.FullPath);


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
            AutoSaveCheckbox.IsChecked = false;
        }

        private async Task SaveText_ClickedAsync(object sender, EventArgs e)
        {
            await PassphraseLogic();


            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace())
            {
                AutoSaverTimerStopClear();
                return;
            }

            if (_CurrentAppSettings.FileLocation.IsNullOrWhiteSpace())
            {
                var fileSaverResult = await FileSaver.Default.SaveAsync(_CurrentAppSettings.FileName, new MemoryStream());
                Task.WaitAll();

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

            FilEncryption.EncryptFile(Note.Text, _CurrentAppSettings.FullLocation, _CurrentAppSettings.Passphrase);

            MainPageStatus.Text = $"Note Saved in {_CurrentAppSettings.FullLocation} at {DateTime.Now}";

            lblFileName.Text = $"Current File is " + Path.GetFileName(_CurrentAppSettings.FullLocation);

            return;
        }

        private async void NewFile_Clicked(object sender, EventArgs e)
        {
            bool NewFile = false;

            if (Note.Text.Length.Equals(0))
            {
                return;
            }

            NewFile = await DisplayAlert("Confirmation", "Are you sure you want to start a new file? Make sure you stuff is saved before creating a new file.", "Yes", "No");

            if (NewFile)
            {
                ClearOutStoredValues();

                MainPageStatus.Text = string.Empty;

                Note.Text = string.Empty;

                lblFileName.Text = $"File not saved yet.";
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


            // var flyout = Historymnu as MenuFlyoutSubItem;

            //var itemX = new MenuFlyoutItem { Text = "Item X", CommandParameter = "Test",IsEnabled = true, };

            // flyout.Add(itemX);
            //MainMenu.Add(itemX);


            await SaveText_ClickedAsync(null, null);
        }

        private void ClearOutStoredValues()
        {
            _CurrentAppSettings = new CurrentAppSettings();
            MainPageStatus.Text = string.Empty;
        }

    }








}
