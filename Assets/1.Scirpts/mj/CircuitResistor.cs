using System;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitResistor
    {
        [SerializeField] private string resistorId;
        [SerializeField] private string pinAId;
        [SerializeField] private string pinBId;
        [SerializeField] private float resistanceOhms;

        public string ResistorId => resistorId;
        public string PinAId => pinAId;
        public string PinBId => pinBId;
        public float ResistanceOhms => resistanceOhms;

        public CircuitResistor()
        {
        }

        public CircuitResistor(string resistorId, string pinAId, string pinBId, float resistanceOhms)
        {
            this.resistorId = resistorId;
            this.pinAId = pinAId;
            this.pinBId = pinBId;
            this.resistanceOhms = resistanceOhms;
        }
    }
}
