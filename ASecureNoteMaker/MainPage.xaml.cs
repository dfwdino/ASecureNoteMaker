using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;
using System.Security.Cryptography;
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
            if (!double.TryParse(_currentAppSettings.Settings.AutoSaveTimeSeconds, out double intervalSeconds) || intervalSeconds <= 0)
                intervalSeconds = 30;

            _autoSaveTimer = Dispatcher.CreateTimer();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(intervalSeconds);
            _autoSaveTimer.Tick += async (_, _) =>
            {
                try
                {
                    await SaveTextAsync();
                }
                catch (Exception ex)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                        SetStatus($"Auto-save failed: {ex.Message}"));
                }
            };
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
        SetStatus("Ready to edit");
        Note.Text = string.Empty;
        lblFileName.Text = "Untitled Document";
        LastSavedTime.Text = string.Empty;
    }

    private void SetStatus(string message)
    {
        MainPageStatus.Text = message;
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
            StopAndClearAutoSaveTimer();
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
        _currentAppSettings.Settings = JsonSerializer.Deserialize<SettingsModel>(jsonString) ?? new SettingsModel();
        RebuildHistoryMenu();

        if (string.IsNullOrWhiteSpace(_currentAppSettings.Settings?.DefaultFileLocation) ||
            !File.Exists(_currentAppSettings.Settings.DefaultFileLocation))
        {
            if (!string.IsNullOrWhiteSpace(_currentAppSettings.Settings?.DefaultFileLocation))
                SetStatus($"Default file not found: {Path.GetFileName(_currentAppSettings.Settings.DefaultFileLocation)}");
            return;
        }

        await SetPassphraseAsync(_currentAppSettings.Settings.DefaultFileLocation);
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
            return;

        await OpenAndDecryptFileAsync(_currentAppSettings.Settings.DefaultFileLocation);
    }

    private async void OpenFile_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(Note.Text))
        {
            if (!await DisplayAlert("Confirmation", "Are you sure you want to open a new file? Unsaved changes will be lost unless you save first.", "Yes", "No"))
                return;

            if (await DisplayAlert("Save First?", "Do you want to save the current file before opening another?", "Yes", "No"))
                await SaveTextAsync();
        }

        ResetAppState();

        var result = await FilePicker.PickAsync();
        if (result is null)
            return;

        await SetPassphraseAsync(result.FullPath);
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
            return;

        await OpenAndDecryptFileAsync(result.FullPath);
    }

    private async Task OpenAndDecryptFileAsync(string filePath)
    {
        try
        {
            string decryptedText = FilEncryption.DecryptFile(filePath, _currentAppSettings.Passphrase);

            if (string.IsNullOrWhiteSpace(decryptedText))
            {
                await DisplayAlert("Empty File", "The file appears to be empty or could not be decrypted.", "OK");
                _currentAppSettings.Clear();
                return;
            }

            Note.Text = decryptedText;
            lblFileName.Text = Path.GetFileName(filePath);
            _currentAppSettings.FullLocation = filePath;
            SetStatus("File opened successfully");
            AddToRecentFiles(filePath);
        }
        catch (CryptographicException)
        {
            await DisplayAlert("Decryption Failed", "Could not decrypt this file. The passphrase may be incorrect, or the file may be corrupted.", "OK");
            _currentAppSettings.Clear();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Opening File", ex.Message, "OK");
            _currentAppSettings.Clear();
        }
    }

    private async Task SetPassphraseAsync(string filename = "", bool isSaving = false)
    {
        if (!string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
            return;

        var promptPage = new PassphrasePromptPage(filename, isSaving);
        await Navigation.PushModalAsync(promptPage, animated: true);
        string? passphrase = await promptPage.Result;

        if (string.IsNullOrWhiteSpace(passphrase))
        {
            await DisplayAlert("No Passphrase", "A passphrase is required to encrypt or decrypt the file.", "OK");
            return;
        }

        _currentAppSettings.Passphrase = passphrase;
    }

    private async void SaveText_Clicked(object sender, EventArgs e)
    {
        await SaveTextAsync();
    }

    private async Task SaveTextAsync()
    {
        await SetPassphraseAsync(isSaving: true);
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
                await DisplayAlert("No Location", "Please choose a file location to save to.", "OK");
                StopAndClearAutoSaveTimer();
                return;
            }

            _currentAppSettings.FullLocation = fileSaverResult.FilePath;
        }

        try
        {
            FilEncryption.EncryptFile(Note.Text, _currentAppSettings.FullLocation, _currentAppSettings.Passphrase);
            lblFileName.Text = Path.GetFileName(_currentAppSettings.FullLocation);
            SetStatus($"Saved: {Path.GetFileName(_currentAppSettings.FullLocation)}");
            LastSavedTime.Text = $"Last saved {DateTime.Now:HH:mm:ss}";
            AddToRecentFiles(_currentAppSettings.FullLocation);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Save Failed", $"Could not save the file: {ex.Message}", "OK");
        }
    }

    private async Task SaveCurrentFileAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
            await SetPassphraseAsync(isSaving: true);

        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
            return;

        if (string.IsNullOrWhiteSpace(_currentAppSettings.EncryptedFilePath))
        {
            try
            {
                var fileSaverResult = await FileSaver.Default.SaveAsync(_currentAppSettings.FileName, Stream.Null);
                _currentAppSettings.EncryptedFilePath = fileSaverResult.FilePath;

                if (string.IsNullOrWhiteSpace(_currentAppSettings.EncryptedFilePath))
                {
                    await DisplayAlert("No Location", "No file location was selected.", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Save Failed", $"Could not save file: {ex.Message}", "OK");
                return;
            }
        }

        FilEncryption.EncryptFile(Note.Text, _currentAppSettings.EncryptedFilePath, _currentAppSettings.Passphrase);
        _currentAppSettings.Clear();
    }

    private void Settings_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SettingsPage());
    }

    // ── History ──────────────────────────────────────────────────────────────

    private void AddToRecentFiles(string filePath)
    {
        var recent = _currentAppSettings.Settings.RecentFiles;

        recent.Remove(filePath);
        recent.Insert(0, filePath);

        if (recent.Count > 5)
            recent.RemoveRange(5, recent.Count - 5);

        RebuildHistoryMenu();
        _ = SaveSettingsAsync();
    }

    private void RebuildHistoryMenu()
    {
        Historymnu.Clear();

        var recent = _currentAppSettings.Settings.RecentFiles;
        if (recent.Count == 0)
        {
            var empty = new MenuFlyoutItem { Text = "(No recent files)", IsEnabled = false };
            Historymnu.Add(empty);
            return;
        }

        foreach (string path in recent)
        {
            string path1 = path;
            var item = new MenuFlyoutItem { Text = Path.GetFileName(path1) };
            item.Clicked += async (_, _) => await OpenRecentFileAsync(path1);
            Historymnu.Add(item);
        }
    }

    private async Task OpenRecentFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            await DisplayAlert("File Not Found", $"{Path.GetFileName(filePath)} could not be found. It may have been moved or deleted.", "OK");
            _currentAppSettings.Settings.RecentFiles.Remove(filePath);
            RebuildHistoryMenu();
            await SaveSettingsAsync();
            return;
        }

        if (!string.IsNullOrEmpty(Note.Text))
        {
            if (!await DisplayAlert("Open Recent", $"Open {Path.GetFileName(filePath)}? Unsaved changes will be lost.", "Yes", "No"))
                return;
        }

        ResetAppState();
        await SetPassphraseAsync(filePath);
        if (string.IsNullOrWhiteSpace(_currentAppSettings.Passphrase))
            return;

        await OpenAndDecryptFileAsync(filePath);
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_currentAppSettings.Settings, options);
            await File.WriteAllTextAsync(_settingsFileFullLocation, json);
        }
        catch
        {
            // Settings persistence failure is non-critical
        }
    }
}
