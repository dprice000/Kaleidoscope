using System.Threading.Tasks;

namespace Kaleidoscope.Services
{
    public interface IMessageBoxService
    {
        Task<DialogResult> ShowMessageAsync(string title, string message, MessageBoxButtons buttons = MessageBoxButtons.Ok);
    }
}