namespace BExIS.Modules.Sam.UI.Models
{
    public class InfoModel
    {
        public string WindowName { get; set; }

        public string Message { get; set; }

        public InfoModel(string windowName, string message)
        {
            WindowName = windowName;
            Message = message;
        }
    }
}