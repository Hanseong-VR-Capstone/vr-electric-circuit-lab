using System;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitLed
    {
        [SerializeField] private string ledId;
        [SerializeField] private string anodePinId;
        [SerializeField] private string cathodePinId;

        public string LedId => ledId;
        public string AnodePinId => anodePinId;
        public string CathodePinId => cathodePinId;

        public CircuitLed()
        {
        }

        public CircuitLed(string ledId, string anodePinId, string cathodePinId)
        {
            this.ledId = ledId;
            this.anodePinId = anodePinId;
            this.cathodePinId = cathodePinId;
        }
    }
}
