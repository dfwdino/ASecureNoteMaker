using Microsoft.Maui.Controls;
using System.IO;
using Microsoft.Maui.Storage;
using System;
using Microsoft.Maui.Controls;
using System.IO;
using Microsoft.Maui.Storage;




namespace ASecureNoteMaker
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        string passphrase = "YourSecurePassphrase"; // Use a strong passphrase
        string encryptedFilePath = Path.Combine(@"c:\temp\", "encryptedfile.txt");

        private IDispatcherTimer autoSaveTimer;



        public MainPage()
        {
            InitializeComponent();
            //encryptedFilePath = String.Empty;
            this.Loaded += OnPageLoaded;
        }

        private async void OpenFile_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    Note.Text = FilEncryption.DecryptFile(result.FullPath, passphrase);

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Input", "Please enter some text:");

            if (!string.IsNullOrEmpty(result))
            {
                passphrase = result;
            }

            if (!encryptedFilePath.Equals(string.Empty))
            {
                using var stream = new MemoryStream(Encoding.Default.GetBytes(Note.Text));

                var encryptedFilePath = await FileSaver.Default.SaveAsync(null, stream, CancellationToken.None);

            }

        private void SaveText_Clicked(object sender, EventArgs e)
        {
            FilEncryption.EncryptFile(Note.Text, encryptedFilePath, passphrase);
            MainPageStatus.Text = $"Note Saved in {encryptedFilePath}";
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

        private void AutoSave_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                // Start auto-save
                autoSaveTimer = Dispatcher.CreateTimer();
                autoSaveTimer.Interval = TimeSpan.FromSeconds(30);
                autoSaveTimer.Tick += (s, e) => SaveText_Clicked(this, EventArgs.Empty);
                autoSaveTimer.Start();
            }
            else
            {
                // Stop auto-save
                autoSaveTimer?.Stop();
            }

        }

    }








}
