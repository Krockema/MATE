using Master40.Tools.Messages;
using Microsoft.AspNetCore.SignalR;

namespace Master40.Tools.SignalR
{
    public class ProcessingHub : Hub
    {
        /*
        private readonly IProcessMrp _processMrp;
        */
        private readonly IMessageHub _hubCallback;

        public ProcessingHub(IMessageHub hubCallback) //, IProcessMrp processMrp)
        {
            //_processMrp = processMrp;
            _hubCallback = hubCallback;
        }
        
        public void SystemReady()
        {
            Clients.All.SendAsync(method: "Send", arg1: _hubCallback.ReturnMsgBox(msg: "SignalR Hub active.", type: MessageType.info));
        }

        public void SignalReady()
        {
            Clients.All.SendAsync(method: "Send", arg1: "SignalReady");
        }


        public void SystemReady2()
        {
            Clients.All.SendAsync(method: "Send", arg1: _hubCallback.ReturnMsgBox(msg: "SignalR Hub active Call Internal.", type: MessageType.info));
        }
    }
}
