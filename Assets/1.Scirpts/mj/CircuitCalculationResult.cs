using System;
using System.Collections.Generic;

namespace VRCircuit.Calculation
{
    [Serializable]
    public class CircuitCalculationResult
    {
        private bool hasValidCircuit;
        private float totalVoltage;
        private float totalCurrent;
        private float totalResistance;
        private int activeLoadCount;
        private Dictionary<string, float> perLoadVoltage = new Dictionary<string, float>();
        private Dictionary<string, float> perBranchCurrent = new Dictionary<string, float>();

        public bool HasValidCircuit => hasValidCircuit;
        public float TotalVoltage => totalVoltage;
        public float TotalCurrent => totalCurrent;
        public float TotalResistance => totalResistance;
        public int ActiveLoadCount => activeLoadCount;
        public IReadOnlyDictionary<string, float> PerLoadVoltage => perLoadVoltage;
        public IReadOnlyDictionary<string, float> PerBranchCurrent => perBranchCurrent;

        public CircuitCalculationResult()
        {
        }

        public CircuitCalculationResult(
            bool hasValidCircuit,
            float totalVoltage,
            float totalCurrent,
            float totalResistance,
            int activeLoadCount)
        {
            this.hasValidCircuit = hasValidCircuit;
            this.totalVoltage = totalVoltage;
            this.totalCurrent = totalCurrent;
            this.totalResistance = totalResistance;
            this.activeLoadCount = activeLoadCount;
        }

        public void SetPerLoadVoltage(string loadId, float voltage)
        {
            if (string.IsNullOrEmpty(loadId))
            {
                return;
            }

            perLoadVoltage[loadId] = voltage;
        }

        public void SetPerBranchCurrent(string branchId, float current)
        {
            if (string.IsNullOrEmpty(branchId))
            {
                return;
            }

            perBranchCurrent[branchId] = current;
        }
    }
}
