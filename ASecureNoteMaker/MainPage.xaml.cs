using ASecureNoteMaker.Extensions;
using CommunityToolkit.Maui.Storage;
using System.Text;


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
            passphrase = string.Empty;

            await PassphraseLogic();

            if (string.IsNullOrEmpty(passphrase))
            { return; }

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

        private async Task PassphraseLogic()
        {
            if (passphrase.IsNullOrWhiteSpace())
            {
                passphrase = await DisplayPromptAsync("Input", "Please enter some text:");
            }

            if (passphrase.IsNullOrWhiteSpace())
            {
                await DisplayAlert("Blank value", "Can't have blank text in the password", "Ok");
            }

            return;
        }

        private void AutoSaverTimerStopClear()
        {
            autoSaveTimer?.Stop();
            AutoSave.IsChecked = false;
        }

        private async void SaveText_ClickedAsync(object sender, EventArgs e)
        {
            await PassphraseLogic();
            
            if(passphrase.IsNullOrWhiteSpace())
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


            MainPageStatus.Text = $"Note Saved in {encryptedFilePath}";

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

    }








}
