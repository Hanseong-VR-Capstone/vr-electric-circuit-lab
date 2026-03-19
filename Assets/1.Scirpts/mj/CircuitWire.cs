using System;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitWire
    {
        [SerializeField] private string wireId;
        [SerializeField] private string pinAId;
        [SerializeField] private string pinBId;

        public string WireId => wireId;
        public string PinAId => pinAId;
        public string PinBId => pinBId;

        public CircuitWire()
        {
        }

        public CircuitWire(string wireId, string pinAId, string pinBId)
        {
            this.wireId = wireId;
            this.pinAId = pinAId;
            this.pinBId = pinBId;
        }
    }
}
