using System;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitSocket
    {
        [SerializeField] private string socketId;
        [SerializeField] private SocketType socketType;
        [SerializeField] private string nodeId;
        [SerializeField] private string connectedPinId;

        public string SocketId => socketId;
        public SocketType SocketType => socketType;
        public string NodeId => nodeId;
        public string ConnectedPinId => connectedPinId;
        public bool IsOccupied => !string.IsNullOrEmpty(connectedPinId);

        public CircuitSocket()
        {
        }

        public CircuitSocket(string socketId, SocketType socketType, string nodeId)
        {
            this.socketId = socketId;
            this.socketType = socketType;
            this.nodeId = nodeId;
            connectedPinId = null;
        }

        public void SetConnectedPinId(string pinId)
        {
            connectedPinId = pinId;
        }
    }
}
