using Akka.Actor;
using Akka.Event;

namespace Master40.SimulationCore.Reporting
{
    // A dead letter handling actor specifically for messages of type "DeadLetter"
    class DeadLetterMonitor :  ReceiveActor
    {
        public DeadLetterMonitor()
        {
            Receive<DeadLetter>(handler: dl => HandleDeadletter(dl: dl));
        }

        private void HandleDeadletter(DeadLetter dl)
        {
            if (dl.Message.GetType().Name == "AdvanceTo")
            {
                return;
            }
            //TODO Throw unhandled messages ?
            System.Diagnostics.Debug.WriteLine(message: $"DeadLetter captured: {dl.Message}, sender: {dl.Sender}, recipient: {dl.Recipient}");
        }
    }
}

