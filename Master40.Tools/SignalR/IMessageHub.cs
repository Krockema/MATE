﻿using Master40.Tools.Messages;

namespace Master40.Tools.SignalR
{
    public interface IMessageHub
    {
        void SendToAllClients(string msg, MessageType msgType = MessageType.info);
        void SendToClient(string listener, string msg, MessageType msgType = MessageType.info);
        string ReturnMsgBox(string msg, MessageType type);
        void StartSimulation(string simId, string simNumber);
        void EndSimulation(string msg, string simId, string simNumber);
        void ProcessingUpdate(int simId, int finished, string simType, int max);
        void GuardianState(object msg);
    }
}