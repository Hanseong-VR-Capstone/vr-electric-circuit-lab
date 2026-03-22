using System;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitBattery
    {
        [SerializeField] private string batteryId;
        [SerializeField] private string positivePinId;
        [SerializeField] private string negativePinId;
        [SerializeField] private float voltage = 1.5f;

        public string BatteryId => batteryId;
        public string PositivePinId => positivePinId;
        public string NegativePinId => negativePinId;
        public float Voltage => voltage;

        public CircuitBattery()
        {
        }

        public CircuitBattery(string batteryId, string positivePinId, string negativePinId, float voltage)
        {
            this.batteryId = batteryId;
            this.positivePinId = positivePinId;
            this.negativePinId = negativePinId;
            this.voltage = voltage;
        }
    }
}
