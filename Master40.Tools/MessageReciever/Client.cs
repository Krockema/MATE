using System;
using Master40.MessageSystem.SignalR;


namespace Master40.MessageSystem.MessageReciever
{
    public class Client
    {
        /*
        private readonly IConnectionManager _connectionManager;
        public Client(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            listernerAsync();
        }

        private async void listernerAsync()
        {

            //var hubConnectionCpmteContext = _connectionManager.GetConnectionContext(typeof(MessageHub));
            //var hubgoubs = hubConnectionCpmteContext.Groups;

            var hubConnection = new HubConnection("http://localhost:54347"); // /signalr");
            IHubProxy hubProxy = hubConnection.CreateHubProxy("MessageHub");
            hubProxy.On<string>("clientListener", msg);
            // start hub connection
            await hubConnection.Start();

            
            dynamic dynamic = "clientListener";
            // Send to all clientListener
            var whatsdis = _connectionManager.GetHubContext<MessageHub>()
                .Clients.All;

            // call Proxy by queue.
            whatsdis.Invoke("clientListener", "yo");
           
              //  .dynamic("SomeInformationWithParam", "info !!!!!!!!!!!!!");

          

        }

        private void msg(string s)
        {
            Console.WriteLine(s);
        }


    */

    }
}

