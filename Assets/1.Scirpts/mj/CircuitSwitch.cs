using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitSwitchContactPair
    {
        [SerializeField] private string pinAId;
        [SerializeField] private string pinBId;

        public string PinAId => pinAId;
        public string PinBId => pinBId;

        public CircuitSwitchContactPair(string pinAId, string pinBId)
        {
            this.pinAId = pinAId;
            this.pinBId = pinBId;
        }
    }

    [Serializable]
    public class CircuitSwitch
    {
        [SerializeField] private string switchId;
        [SerializeField] private string pin0Id;
        [SerializeField] private string pin1Id;
        [SerializeField] private string pin2Id;
        [SerializeField] private string pin3Id;
        [SerializeField] private bool isOn;

        public string SwitchId => switchId;
        public string Pin0Id => pin0Id;
        public string Pin1Id => pin1Id;
        public string Pin2Id => pin2Id;
        public string Pin3Id => pin3Id;
        public bool IsOn => isOn;

        // Backward-compatible aliases for older debug code.
        public string PinAId => pin0Id;
        public string PinBId => pin2Id;

        public CircuitSwitch()
        {
        }

        public CircuitSwitch(string switchId, string pinAId, string pinBId, bool isOn)
        {
            this.switchId = switchId;
            pin0Id = pinAId;
            pin2Id = pinBId;
            this.isOn = isOn;
        }

        public CircuitSwitch(
            string switchId,
            string pin0Id,
            string pin1Id,
            string pin2Id,
            string pin3Id,
            bool isOn)
        {
            this.switchId = switchId;
            this.pin0Id = pin0Id;
            this.pin1Id = pin1Id;
            this.pin2Id = pin2Id;
            this.pin3Id = pin3Id;
            this.isOn = isOn;
        }

        public IReadOnlyList<CircuitSwitchContactPair> GetAlwaysConnectedPairs()
        {
            return new List<CircuitSwitchContactPair>
            {
                new CircuitSwitchContactPair(pin0Id, pin1Id),
                new CircuitSwitchContactPair(pin2Id, pin3Id)
            };
        }

        public IReadOnlyList<CircuitSwitchContactPair> GetOnConnectedPairs()
        {
            return new List<CircuitSwitchContactPair>
            {
                new CircuitSwitchContactPair(pin0Id, pin2Id),
                new CircuitSwitchContactPair(pin0Id, pin3Id),
                new CircuitSwitchContactPair(pin1Id, pin2Id),
                new CircuitSwitchContactPair(pin1Id, pin3Id)
            };
        }

        public IReadOnlyList<CircuitSwitchContactPair> GetActiveConnectedPairs()
        {
            List<CircuitSwitchContactPair> pairs = new List<CircuitSwitchContactPair>();
            pairs.AddRange(GetAlwaysConnectedPairs());

            if (isOn)
            {
                pairs.AddRange(GetOnConnectedPairs());
            }

            return pairs;
        }

        public void SetIsOn(bool value)
        {
            isOn = value;
        }
    }
}
