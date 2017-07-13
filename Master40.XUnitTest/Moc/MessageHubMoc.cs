using Master40.DB.Data.Helper;
using Master40.MessageSystem.Messages;
using Master40.MessageSystem.SignalR;

namespace Master40.XUnitTest.Moc
{
    public class MessageHub : IMessageHub
    {
        public void SendToAllClients(string msg)
        {
        }

        public void SendToAllClients(string msg, MessageType msgType)
        {

        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "";
        }

        public void EndScheduler()
        {

        }
    }

}