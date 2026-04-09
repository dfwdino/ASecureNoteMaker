namespace ASecureNoteMaker;

public partial class PassphrasePromptPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _tcs = new();

    public Task<string?> Result => _tcs.Task;

    public PassphrasePromptPage(string filename = "", bool isSaving = false)
    {
        InitializeComponent();

        ConfirmButton.Text = isSaving ? "Lock" : "Unlock";

        if (!string.IsNullOrWhiteSpace(filename))
            PromptLabel.Text = $"Enter the passphrase for:\n{Path.GetFileName(filename)}";

        PassphraseEntry.Focused += (_, _) => PassphraseEntry.ReturnCommand = new Command(async () =>
        {
            await CompleteAsync(PassphraseEntry.Text);
        });
    }

    private void OnShowPassphraseChanged(object sender, CheckedChangedEventArgs e)
    {
        PassphraseEntry.IsPassword = !e.Value;
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        await CompleteAsync(PassphraseEntry.Text);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CompleteAsync(null);
    }

    private async Task CompleteAsync(string? passphrase)
    {
        if (_tcs.Task.IsCompleted) return;
        _tcs.SetResult(passphrase);
        await Navigation.PopModalAsync(animated: true);
    }
}
