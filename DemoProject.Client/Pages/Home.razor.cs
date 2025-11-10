using DemoProject.Client.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace DemoProject.Client.Pages
{
    public partial class Home
    {
        string htmlValue = string.Empty;
        [Parameter]
        public string? url { get; set; }
        public string? password { get; set; }
        public string passwordToOpen { get; set; }
        private string NotePassword { get; set; }
        private bool IsEditorDisabled { get; set; }

        [Inject] NavigationManager Nav { get; set; } = null!;
        [Inject] NoteService NoteService { get; set; } = null!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;

        private bool IsOpen { 
            get;
            set; } = false;
        private bool IsOpenPassword { 
            get;
            set; } = false;
        private bool IsOpenPutPassword { 
            get;
            set; }



        private async Task OnModalSave(string newUrl)
        {
            var newurl = await NoteService.ChangeUrlAsync(url, newUrl);
            if (newurl == true)
            {
                Nav.NavigateTo($"/{newUrl}", true);
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "Error changing URL!");
            }


        }

        private async Task OnModalSavePassword(string password)
        {
            var result = await NoteService.SetPassword(url, password);
            NotePassword = password;

        }

        private async Task OnProtecMeModalSave()
        {
            var result = await NoteService.SetPassword(url, null, true);
            if (!result)
            {
                await JSRuntime.InvokeVoidAsync("alert", "Error setting password!");
            }
        }

        private async Task OnModalSavePutPassword(string password)
        {
            var note = await NoteService.GetNoteByUrl(url, password);

            if (note != null)
            {
                if (note.IsSecure && note.Content == null)
                {
                    IsEditorDisabled = true;
                    await JSRuntime.InvokeVoidAsync("alert", "Incorrect password!");
                    IsOpenPutPassword = true;
                }
                else
                {
                    IsEditorDisabled = false;
                    IsOpenPutPassword = false;
                    NotePassword = password;
                    htmlValue = note.Content;
                }
            }
        }


        private void OnExecute(HtmlEditorExecuteEventArgs args)
        {
            if (args.CommandName == "ChangeUrl")
            {
                IsOpen = true;
                IsOpenPassword = false;
                IsOpenPutPassword = false;
            }
            else if (args.CommandName == "SetPassword")
            {
                IsOpen = false;
                IsOpenPassword = true;
                IsOpenPutPassword = false;
            }
        }


        CancellationTokenSource cancellationToken;



        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(url))
            {
                url = Guid.NewGuid().ToString("N");
                Nav.NavigateTo($"/{url}", true);

            }
            else
            {
                var note = await NoteService.GetNoteByUrl(url);

                if (note != null)
                {
                    if (note.IsSecure && note.Content == null)
                    {
                        IsEditorDisabled = true;
                        IsOpenPutPassword = true;
                    }
                    else
                    {
                        htmlValue = note.Content;
                    }
                }
            }

            base.OnInitializedAsync();

        }

        async Task OnInput(string html)
        {
            cancellationToken?.Cancel();

            cancellationToken?.Dispose();

            cancellationToken = new CancellationTokenSource();

            try
            {
                var token = cancellationToken.Token;
                await Task.Delay(5000, token);

                if (!token.IsCancellationRequested)
                {
                    var result = await NoteService.CreateOrEditNoteAsync(url, html, NotePassword);
                    if (!result)
                    {
                        await JSRuntime.InvokeVoidAsync("alert", "Error saving note!");
                    }
                }
            }
            catch (Exception e)
            {
                // Ignore
            }
        }
    }
}
