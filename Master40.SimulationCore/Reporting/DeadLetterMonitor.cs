using Akka.Actor;
using Akka.Event;

namespace Master40.SimulationCore.Reporting
{
    // A dead letter handling actor specifically for messages of type "DeadLetter"
    class DeadLetterMonitor :  ReceiveActor
    {
        public DeadLetterMonitor()
        {
            Receive<DeadLetter>(dl => HandleDeadletter(dl));
        }

        private void HandleDeadletter(DeadLetter dl)
        {
            System.Diagnostics.Debug.WriteLine($"DeadLetter captured: {dl.Message}, sender: {dl.Sender}, recipient: {dl.Recipient}");
        }
    }
}

