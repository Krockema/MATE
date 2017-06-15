using Master40.Models;

namespace Master40.SignalR
{
    public static class Callback
    {
        public static string ReturnMsgBox(string msg, MessageType type)
        {
            return "<div class=\"alert alert-" + type + "\">" + msg + "</div>";
        }
        
    }
}