﻿using ASecureNoteMaker.Extensions;
using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;
using System.Text.Json;


namespace ASecureNoteMaker
{
    public partial class MainPage : ContentPage
    {

        CurrentAppSettings _CurrentAppSettings = new();
        private string _SettingsFileFullLocation = string.Empty;

        private IDispatcherTimer autoSaveTimer;

        // Base Functions
        public MainPage()
        {
            InitializeComponent();

            _SettingsFileFullLocation = Path.Combine(FileSystem.AppDataDirectory, "Settings.json");

            this.Loaded += OnPageLoaded;
        }

        private void AutoSave_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                // Start auto-save
                autoSaveTimer = Dispatcher.CreateTimer();
                autoSaveTimer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(_CurrentAppSettings.Settings.AutoSaveTimeSeconds));
                autoSaveTimer.Tick += (s, e) => SaveText_ClickedAsync(this, EventArgs.Empty);
                autoSaveTimer.Start();
            }
            else
            {
                // Stop auto-save
                autoSaveTimer?.Stop();
            }

        }

        private void AutoSaverTimerStopClear()
        {
            autoSaveTimer?.Stop();
            AutoSaveCheckbox.IsChecked = false;
        }

        private void ClearOutStoredValues()
        {
            _CurrentAppSettings.Dispose();
            _CurrentAppSettings = new CurrentAppSettings();
            MainPageStatus.Text = string.Empty;
        }

        private async void Exit_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Close Program", "Do you want to close the program?", "Yes", "No");

            if (answer)
            {
                //Close the application
                Application.Current.Quit();
            }
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

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            if (File.Exists(_SettingsFileFullLocation).Equals(false))
            {
                return;
            }


            string jsonString = string.Empty;

            jsonString = File.ReadAllText(_SettingsFileFullLocation);

            _CurrentAppSettings.Settings = JsonSerializer.Deserialize<SettingsModel>(jsonString);


            if (File.Exists(_CurrentAppSettings.Settings.DefaultFileLocation).Equals(false))

            {
                MainPageStatus.Text = $"File {_CurrentAppSettings.Settings.DefaultFileLocation} can't be found.";
                return;
            }


            await PassphraseLogic();

            try
            {
                string decrypttext = FilEncryption.DecryptFile(_CurrentAppSettings.Settings.DefaultFileLocation, _CurrentAppSettings.Passphrase);

                if (decrypttext.IsNullOrWhiteSpace())
                {
                    await DisplayAlert("Blank File", "The file is not encypted, blank or wrong passcode.", "OK");
                    return;
                }

                Note.Text = decrypttext;

                lblFileName.Text = $"Current File is " + Path.GetFileName(_CurrentAppSettings.Settings.DefaultFileLocation);

                _CurrentAppSettings.FullLocation = _CurrentAppSettings.Settings.DefaultFileLocation;


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }




        }
        private async void OpenFile_Clicked(object sender, EventArgs e)
        {

            //SaveText_Clicked(null, null); //Test the history only

            if (Note.Text.Length > 0)
            {
                bool NewFile = await DisplayAlert("Confirmation", "Are you sure you want to start a new file?  Make sure your update has been saved.", "Yes", "No");

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
                            fileSaverResult = await FileSaver.Default.SaveAsync(_CurrentAppSettings.FileName, new MemoryStream());
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
                        _CurrentAppSettings.Clear();
                    }
                }
            }

            #region Loading New File

            //Clear out any new values if they are saved.
            ClearOutStoredValues();

            var result = await FilePicker.PickAsync();

            if (result is null)
            {
                return;
            }

            await PassphraseLogic();

            if (_CurrentAppSettings.Passphrase.IsNullOrWhiteSpace())
            {
                return;
            }

            try
            {
                string decrypttext = FilEncryption.DecryptFile(result.FullPath, _CurrentAppSettings.Passphrase);

                if (decrypttext.IsNullOrWhiteSpace())
                {
                    await DisplayAlert("Blank File", "The file is not encrypted, blank or wrong passcode.", "OK");
                    return;
                }

                Note.Text = FilEncryption.DecryptFile(result.FullPath, _CurrentAppSettings.Passphrase);

                lblFileName.Text = $"Current File is " + Path.GetFileName(result.FullPath);

                _CurrentAppSettings.FullLocation = result.FullPath;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                _CurrentAppSettings.Clear();
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
        private async void SaveText_Clicked(object sender, EventArgs e)
        {
            AddNewEntryToHistoryMenu();

            await SaveText_ClickedAsync(null, null);
        }

        private void AddNewEntryToHistoryMenu()
        {

            // Assuming 'menuItems' is a collection of IMenuElement
            var subMenuItems = MainMenu.OfType<MenuFlyoutSubItem>();

            // Now you can iterate over subMenuItems to find or add new items
            foreach (var subMenuItem in subMenuItems)
            {
                if (subMenuItem.Text == "History")
                {
                    var newEntry = new MenuFlyoutItem
                    {
                        Text = "New Entry",
                        Command = new Command(() => { /* Your command logic here */ })
                    };

                    subMenuItem.Add(newEntry);
                }
            }



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
                    await DisplayAlert("Blank value", "No location found or used.", "Ok");
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
        private void Settings_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingsPage());
        }
    }








}