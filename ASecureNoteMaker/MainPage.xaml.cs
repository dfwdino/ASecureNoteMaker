using ASecureNoteMaker.Extensions;
using CommunityToolkit.Maui.Storage;
using System.Text;

namespace ASecureNoteMaker
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        string passphrase = "YourSecurePassphrase"; // Use a strong passphrase
        string encryptedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "encryptedfile.txt");



        public MainPage()
        {
            InitializeComponent();
            //encryptedFilePath = String.Empty;
            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Input", "Please enter some text:");

            if (!string.IsNullOrEmpty(result))
            {
                passphrase = result;
            }

            //Note.Text = FilEncryption.DecryptFile(encryptedFilePath, passphrase);
        }

        private async void SaveText_Clicked(object sender, EventArgs e)
        {

            if (passphrase == null)
            {
                passphrase = await DisplayPromptAsync("Input", "Can't save file with no password.  Please enter a password.");
            }

            if (passphrase == null)
            {
                await DisplayAlert("Password Blank", "Pasword is blank.  Need to have a password to save the file.", "Ok");
                return;
            }

            if (!encryptedFilePath.Equals(string.Empty))
            {
                using var stream = new MemoryStream(Encoding.Default.GetBytes(Note.Text));

                var encryptedFilePath = await FileSaver.Default.SaveAsync(null, stream, CancellationToken.None);

            }


            if (!encryptedFilePath.IsNullOrWhiteSpace() && !passphrase.IsNullOrWhiteSpace())
            {
                FilEncryption.EncryptFile(Note.Text, encryptedFilePath, passphrase);

                MainPageStatus.Text = $"Note Saved in {encryptedFilePath}";
            }
        }
    }










}
