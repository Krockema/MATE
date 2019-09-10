using System;
using Master40.Tools.Messages;
using Master40.Tools.SignalR;

namespace Master40.Simulation.CLI
{
    public class ConsoleHub: IMessageHub
    {
        public void SendToAllClients(string msg)
        {
            System.Diagnostics.Debug.WriteLine(message: msg);
            ///Console.WriteLine(msg);
        }

        public void SendToAllClients(string msg, MessageType msgType)
        {
            System.Diagnostics.Debug.WriteLine(message: msgType.ToString() + ": " + msg);
            //Console.WriteLine(msg);
        }

        public void SendToClient(string listener, string msg, MessageType msgType)
        {
            Console.WriteLine(value: listener + ": " + msg);
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            Console.WriteLine(value: msg);
            return msg;
        }

        public void EndScheduler()
        {
            Console.WriteLine(value: "Finished Scheduler");
        }

        public void EndSimulation(string msg)
        {
            Console.WriteLine(value: msg);
        }

        public void ProcessingUpdate(int simId, int finished, string simType, int max)
        {
            //Do nothing 
        }

        public void EndSimulation(string msg, string simId, string simNumber)
        {
            Console.WriteLine(value: "End Simulation");
        }
    }

}