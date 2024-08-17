using Microsoft.Maui.Controls;

namespace ASecureNoteMaker
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        string passphrase = "YourSecurePassphrase"; // Use a strong passphrase
        string encryptedFilePath = Path.Combine(@"c:\temp\","encryptedfile.txt");
        

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Input", "Please enter some text:");
            if (!string.IsNullOrEmpty(result))
            {
                passphrase =  result;
            }

            Note.Text = FileEncryptor.DecryptFile(encryptedFilePath, passphrase);
        }

        private void SaveText_Clicked(object sender, EventArgs e)
        {
            FileEncryptor.EncryptFile(Note.Text,encryptedFilePath, passphrase);
        }
    }





           
      
 


}
