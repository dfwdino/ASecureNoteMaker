using ASecureNoteMaker.Extensions;
using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;
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

    private void OnSaveButtonClicked(object sender, EventArgs e)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };

        _SettingsModel.DefaultFileLocation = DefaultFileLocationEntry.Text;
        _SettingsModel.AutoSaveTimeSeconds = AutoSaveTimer.Text.IsNullOrWhiteSpace() ? AutoSaveTimer.Placeholder : AutoSaveTimer.Text;

        string jsonString = JsonSerializer.Serialize(_SettingsModel, options);

        string directoryPath = Path.GetDirectoryName(_SettingsFileFullLocation);

        File.WriteAllText(_SettingsFileFullLocation, jsonString);
    }

    private async void OnBrowseButtonClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();

        _SettingsModel.DefaultFileLocation = result.FullPath;

        DefaultFileLocationEntry.Text = _SettingsModel.DefaultFileLocation;



    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        LoadSettingFile();
    }
}