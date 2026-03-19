using System;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitSwitch
    {
        [SerializeField] private string switchId;
        [SerializeField] private string pinAId;
        [SerializeField] private string pinBId;
        [SerializeField] private bool isOn;

        public string SwitchId => switchId;
        public string PinAId => pinAId;
        public string PinBId => pinBId;
        public bool IsOn => isOn;

        public CircuitSwitch()
        {
        }

        public CircuitSwitch(string switchId, string pinAId, string pinBId, bool isOn)
        {
            this.switchId = switchId;
            this.pinAId = pinAId;
            this.pinBId = pinBId;
            this.isOn = isOn;
        }

        public void SetIsOn(bool value)
        {
            isOn = value;
        }
    }
}
