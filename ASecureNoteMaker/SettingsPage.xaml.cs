using ASecureNoteMaker.Extensions;
using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;
using System.Linq;
using System.Text.Json;

namespace ASecureNoteMaker;

public partial class SettingsPage : ContentPage
{
    SettingsModel _SettingsModel = new SettingsModel();
    private readonly string _SettingsFileFullLocation = string.Empty;
    public SettingsPage()
	{
		InitializeComponent();
        _SettingsFileFullLocation = Path.Combine(FileSystem.AppDataDirectory,"Settings.json");
	}

    private void LoadSettingFile()
    {
        if (File.Exists(_SettingsFileFullLocation))
        {
            string jsonString = File.ReadAllText(_SettingsFileFullLocation);

            _SettingsModel = JsonSerializer.Deserialize<SettingsModel>(jsonString);

            DefaultFileLocationEntry.Text = _SettingsModel.DefaultFileLocation;
            AutoSaveTimer.Text = _SettingsModel.AutoSaveTimeSeconds;

        }
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        string timerText = AutoSaveTimer.Text?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(timerText))
        {
            if (!double.TryParse(timerText, out double seconds) || seconds <= 0)
            {
                await DisplayAlert("Invalid Value", "Auto-save interval must be a positive number of seconds.", "OK");
                return;
            }
        }

        _SettingsModel.DefaultFileLocation = DefaultFileLocationEntry.Text?.Trim() ?? string.Empty;
        _SettingsModel.AutoSaveTimeSeconds = string.IsNullOrWhiteSpace(timerText) ? "30" : timerText;

        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(_SettingsModel, options);
        File.WriteAllText(_SettingsFileFullLocation, jsonString);

        await DisplayAlert("Saved", "Settings have been saved.", "OK");
    }

    private async void OnBrowseButtonClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();

        if (result is null)
            return;

        _SettingsModel.DefaultFileLocation = result.FullPath;
        DefaultFileLocationEntry.Text = _SettingsModel.DefaultFileLocation;
    }

    private void OnAutoSaveTimerTextChanged(object sender, TextChangedEventArgs e)
    {
        string filtered = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
        if (filtered != e.NewTextValue)
            AutoSaveTimer.Text = filtered;
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        LoadSettingFile();
    }
}