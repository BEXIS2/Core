namespace BExIS.Modules.Sam.UI.Models
{
    public class InfoModel
    {
        public InfoModel(string windowName, string message)
        {
            WindowName = windowName;
            Message = message;
        }

        public string Message { get; set; }
        public string WindowName { get; set; }
    }
}