using ASecureNoteMaker.Models;
using CommunityToolkit.Maui.Storage;
using System.Text.Json;

namespace ASecureNoteMaker;

public partial class SettingsPage : ContentPage
{
    SettingsModel _SettingsModel = new SettingsModel();
    private string _SettingsFileFullLocation = string.Empty;
    public SettingsPage()
	{
		InitializeComponent();
        _SettingsFileFullLocation = Path.Combine(FileSystem.Current.AppDataDirectory,"Settings.json");
	}

    private void LoadSettingFile()
    {
        if (File.Exists(_SettingsFileFullLocation))
        {
            string jsonString = File.ReadAllText(_SettingsFileFullLocation);

            _SettingsModel = JsonSerializer.Deserialize<SettingsModel>(jsonString);

            DefaultFileLocationEntry.Text = _SettingsModel.DefaultFileLocation;

        }
    }

    private void OnSaveButtonClicked(object sender, EventArgs e)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };

        string jsonString = JsonSerializer.Serialize(_SettingsModel, options);

        File.WriteAllText(_SettingsFileFullLocation, jsonString);
    }

    private async void OnBrowseButtonClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();

        _SettingsModel.DefaultFileLocation = result.FullPath;

        DefaultFileLocationEntry.Text = _SettingsModel.DefaultFileLocation;



    }
}