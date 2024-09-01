using ASecureNoteMaker.Extensions;
using CommunityToolkit.Maui.Storage;


namespace ASecureNoteMaker
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        string passphrase = string.Empty; //"YourSecurePassphrase"; // Use a strong passphrase
        string encryptedFilePath = string.Empty; //Path.Combine(@"c:\temp\", "encryptedfile.txt");

        private IDispatcherTimer autoSaveTimer;

        // Base Functions
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {


            //When add seetings... need to reade the settings file to get default file.
            //string result = await DisplayPromptAsync("Input", "Please enter some text:");

            //if (!string.IsNullOrEmpty(result))
            //{
            //    passphrase = result;
            //}

            //OpenFile_Clicked(null, null);
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

        // Custom Functions
        private async void OpenFile_Clicked(object sender, EventArgs e)
        {
            if (Note.Text.Length > 0)
            {
                bool NewFile = await DisplayAlert("Confirmation", "Are you sure you want to start a new file?", "Yes", "No");

                if (NewFile.Equals(false))

                { return; }


            }

            ///This is need b/c I cant do await for this function with out return Task in the OpenFile function. Which will brack the UI call. 
            var PassphraseLogicTask = PassphraseLogic();

            await PassphraseLogicTask;

            var result = await FilePicker.PickAsync();

            if (result.FullPath.IsNullOrWhiteSpace())
            {
                DisplayAlert("File Selected", "No file is selected.", "Ok");
                return;
            }

            passphrase = string.Empty;

            await PassphraseLogic(result.FileName);

            if (passphrase.IsNullOrWhiteSpace())
            { return; }


            try
            {
                string decrypttext = FilEncryption.DecryptFile(result.FullPath, passphrase);

                if (decrypttext.IsNullOrWhiteSpace())
                {
                    DisplayAlert("Blank File", "The file is not encypted or blank.", "OK");
                    return;
                }

                Note.Text = FilEncryption.DecryptFile(result.FullPath, passphrase);


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }

            return;
        }

        private async Task PassphraseLogic(string filename = "")
        {
            if (passphrase.IsNullOrWhiteSpace())
            {
                passphrase = await DisplayPromptAsync("Input", $"Please enter some text for file: {filename}");
                Task.WaitAll();
            }

            if (passphrase.IsNullOrWhiteSpace())
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

            if (passphrase.IsNullOrWhiteSpace())
            {
                AutoSaverTimerStopClear();
                return;
            }

            var fileSaverResult = await FileSaver.Default.SaveAsync("Newfile.txt", new MemoryStream(), CancellationToken.None);

            if (fileSaverResult.FilePath == null)
            {
                await DisplayAlert("Blank value", "No locatoin found or used.", "Ok");
                AutoSaverTimerStopClear();
                return;
            }

            FilEncryption.EncryptFile(Note.Text, fileSaverResult.FilePath, passphrase);

            MainPageStatus.Text = $"Note Saved in {fileSaverResult}";

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
