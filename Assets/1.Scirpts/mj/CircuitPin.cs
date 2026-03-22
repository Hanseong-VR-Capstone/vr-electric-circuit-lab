using System;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitPin
    {
        [SerializeField] private string pinId;
        [SerializeField] private PinOwnerType ownerType;
        [SerializeField] private string ownerId;
        [SerializeField] private string currentSocketId;

        public string PinId => pinId;
        public PinOwnerType OwnerType => ownerType;
        public string OwnerId => ownerId;
        public string CurrentSocketId => currentSocketId;
        public bool IsConnected => !string.IsNullOrEmpty(currentSocketId);

        public CircuitPin()
        {
        }

        public CircuitPin(string pinId, PinOwnerType ownerType, string ownerId)
        {
            this.pinId = pinId;
            this.ownerType = ownerType;
            this.ownerId = ownerId;
            currentSocketId = null;
        }

        public void SetCurrentSocketId(string socketId)
        {
            currentSocketId = socketId;
        }
    }
}
