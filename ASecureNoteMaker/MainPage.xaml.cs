using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;
using System.Text.Json;

namespace ASecureNoteMaker;

public partial class MainPage : ContentPage
{
    private readonly CurrentAppSettings _currentAppSettings = new();
    private readonly string _settingsFileFullLocation;
    private IDispatcherTimer? _autoSaveTimer;

    public MainPage()
    {
        InitializeComponent();

        _settingsFileFullLocation = Path.Combine(FileSystem.AppDataDirectory, "Settings.json");
        Loaded += OnPageLoaded;
    }

    private void AutoSave_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _autoSaveTimer = Dispatcher.CreateTimer();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(double.Parse(_currentAppSettings.Settings.AutoSaveTimeSeconds));
            _autoSaveTimer.Tick += (_, _) => SaveTextAsync();
            _autoSaveTimer.Start();
        }
        else
        {
            _autoSaveTimer?.Stop();
        }
    }

    private void StopAndClearAutoSaveTimer()
    {
        _autoSaveTimer?.Stop();
        AutoSaveCheckbox.IsChecked = false;
    }

    private void ResetAppState()
    {
        _currentAppSettings.Dispose();
        _currentAppSettings.Clear();
        MainPageStatus.Text = string.Empty;
        Note.Text = string.Empty;
        lblFileName.Text = "File not saved yet.";
    }

    private async void Exit_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Close Program", "Do you want to close the program?", "Yes", "No"))
        {
            Application.Current?.Quit();
        }
    }

    private async void NewFile_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(Note.Text))
        {
            return;
        }

        if (await DisplayAlert("Confirmation", "Are you sure you want to start a new file? Make sure your content is saved.", "Yes", "No"))
        {
            ResetAppState();
        }
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        if (!File.Exists(_settingsFileFullLocation))
        {
            return;
        }

        string jsonString = await File.ReadAllTextAsync(_settingsFileFullLocation);
        _currentAppSettings.Settings = JsonSerializer.Deserialize<SettingsModel>(jsonString);

        if (!File.Exists(_currentAppSettings.Settings.DefaultFileLocation))
        {
            MainPageStatus.Text = $"File {_currentAppSettings.Settings.DefaultFileLocation} can't be found.";
            return;
        }

        await SetPassphraseAsync();

        try
        {
            string decryptedText = FilEncryption.DecryptFile(_currentAppSettings.Settings.DefaultFileLocation, _currentAppSettings.Passphrase);
            if (string.IsNullOrWhiteSpace(decryptedText))
            {
                await DisplayAlert("Blank File", "The file is not encrypted, blank, or the passcode is incorrect.", "OK");
                return;
            }

            Note.Text = decryptedText;
            lblFileName.Text = $"Current File: {Path.GetFileName(_currentAppSettings.Settings.DefaultFileLocation)}";
            _currentAppSettings.FullLocation = _currentAppSettings.Settings.DefaultFileLocation;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void OpenFile_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(Note.Text))
        {
            bool newFile = await DisplayAlert("Confirmation", "Are you sure you want to open a new file? Make sure your updates have been saved.", "Yes", "No");
            if (!newFile)
            {
                return;
            }

            if (await DisplayAlert("Confirmation", "Do you want to save this file?", "Yes", "No"))
            {
                await SaveCurrentFileAsync();
            }
        }

        ResetAppState();
        var result = await FilePicker.PickAsync();
        if (result is null)
        {
            return;
        }

        await SetPassphraseAsync();
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
        {
            return;
        }

        try
        {
            string decryptedText = FilEncryption.DecryptFile(result.FullPath, _currentAppSettings.Passphrase);
            if (string.IsNullOrWhiteSpace(decryptedText))
            {
                await DisplayAlert("Blank File", "The file is not encrypted, blank, or the passcode is incorrect.", "OK");
                return;
            }

            Note.Text = decryptedText;
            lblFileName.Text = $"Current File: {Path.GetFileName(result.FullPath)}";
            _currentAppSettings.FullLocation = result.FullPath;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            _currentAppSettings.Clear();
        }
    }

    private async Task SetPassphraseAsync(string filename = "")
    {
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
        {
            _currentAppSettings.Passphrase = await DisplayPromptAsync("Input", $"Please enter the passphrase for the file: {filename}");
        }

        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
        {
            await DisplayAlert("Blank value", "The passphrase cannot be blank", "Ok");
        }
    }

    private async void SaveText_Clicked(object sender, EventArgs e)
    {
        AddNewEntryToHistoryMenu();
        await SaveTextAsync();
    }

    private void AddNewEntryToHistoryMenu()
    {
        var historySubMenu = MainMenu.OfType<MenuFlyoutSubItem>().FirstOrDefault(item => item.Text == "History");
        if (historySubMenu != null)
        {
            historySubMenu.Add(new MenuItem { Text = "ads" });
        }
    }

    private async Task SaveTextAsync()
    {
        await SetPassphraseAsync();
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
        {
            StopAndClearAutoSaveTimer();
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentAppSettings.FileLocation))
        {
            var fileSaverResult = await FileSaver.Default.SaveAsync(_currentAppSettings.FileName, Stream.Null);

            if (string.IsNullOrWhiteSpace(fileSaverResult.FilePath))
            {
                await DisplayAlert("Blank value", "No location found or used.", "Ok");
                StopAndClearAutoSaveTimer();
                return;
            }

            _currentAppSettings.FullLocation = fileSaverResult.FilePath;
        }

        FilEncryption.EncryptFile(Note.Text, _currentAppSettings.FullLocation, _currentAppSettings.Passphrase);
        MainPageStatus.Text = $"Note Saved in {_currentAppSettings.FullLocation} at {DateTime.Now}";
        lblFileName.Text = $"Current File: {Path.GetFileName(_currentAppSettings.FullLocation)}";
    }

    private async Task SaveCurrentFileAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase) && string.IsNullOrWhiteSpace(_currentAppSettings.EncryptedFilePath))
        {
            await SetPassphraseAsync();

            try
            {
                var fileSaverResult = await FileSaver.Default.SaveAsync(_currentAppSettings.FileName, Stream.Null);
                _currentAppSettings.EncryptedFilePath = fileSaverResult.FilePath;
                if (string.IsNullOrWhiteSpace(_currentAppSettings.EncryptedFilePath))
                {
                    await DisplayAlert("File Selected", "No file is selected.", "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Saving File Issue", $"Can't save file: {ex.Message}", "OK");
                return;
            }
        }
        else
        {
            FilEncryption.EncryptFile(Note.Text, _currentAppSettings.EncryptedFilePath, _currentAppSettings.Passphrase);
            _currentAppSettings.Clear();
        }
    }

    private void Settings_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SettingsPage());
    }
}
