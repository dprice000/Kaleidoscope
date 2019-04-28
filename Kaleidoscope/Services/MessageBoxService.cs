using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Kaleidoscope.Services
{
    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo
    }

    public enum DialogResult
    {
        None,
        Ok,
        Cancel,
        Yes,
        No
    }

    public class MessageBoxService : IMessageBoxService
    {
        public async Task<DialogResult> ShowMessageAsync(string title, string message, MessageBoxButtons buttons = MessageBoxButtons.Ok)
        {
            var dialog = BuildDialog(title, message, buttons);
            var dialogResult = await dialog.ShowAsync();
            return ResolveDialogResult(dialogResult, buttons);
        }

        #region Private Methods

        private ContentDialog BuildDialog(string title, string message, MessageBoxButtons buttons)
        {
            switch(buttons)
            {
                case MessageBoxButtons.Ok:
                    return BuildOkDialog(title, message);
                    break;

                case MessageBoxButtons.OkCancel:
                    return BuildOkCancelDialog(title, message);
                    break;

                case MessageBoxButtons.YesNo:
                    return BuildYesNoDialog(title, message);
                    break;

                default:
                    return BuildDefaultDialog();
            }
        }

        private ContentDialog BuildDefaultDialog()
        {
            return new ContentDialog()
            {
                Title = "Pay no attention to that man behind the curtain!",
                Content = "You should not be seeing this message. \n The Great Oz has spoken!",
                PrimaryButtonText = "Ok"
            };
        }

        private ContentDialog BuildOkDialog(string title, string message)
        {
            return new ContentDialog()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Ok"
            };
        }

        private ContentDialog BuildOkCancelDialog(string title, string message)
        {
            return new ContentDialog()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel"
            };
        }

        private ContentDialog BuildYesNoDialog(string title, string message)
        {
            return new ContentDialog()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };
        }

        private DialogResult ResolveDialogResult(ContentDialogResult dialogResult, MessageBoxButtons buttons)
        {
            DialogResult result = DialogResult.None;

            switch (buttons)
            {
                case MessageBoxButtons.Ok:
                    if (dialogResult == ContentDialogResult.Primary)
                        result = DialogResult.Ok;
                    break;

                case MessageBoxButtons.OkCancel:

                    if (dialogResult == ContentDialogResult.Primary)
                        result = DialogResult.Ok;
                    else if (dialogResult == ContentDialogResult.Secondary)
                        result = DialogResult.Cancel;
                    break;

                case MessageBoxButtons.YesNo:

                    if (dialogResult == ContentDialogResult.Primary)
                        result = DialogResult.Yes;
                    else if (dialogResult == ContentDialogResult.Secondary)
                        result = DialogResult.No;
                    break;
            }

            return result;
        }

        #endregion Private Methods
    }
}