using System.Collections.Generic;
using UnityEngine;
using VRCircuit.Analysis;
using VRCircuit.Calculation;
using VRCircuit.Evaluation;
using VRCircuit.Runtime;

namespace VRCircuit.Query
{
    public class CircuitUIData
    {
        private readonly CircuitState state;
        private readonly bool hasValidCircuit;
        private readonly int activeLoadCount;
        private readonly float totalVoltage;
        private readonly float totalResistance;
        private readonly float totalCurrent;
        private readonly IReadOnlyDictionary<string, float> perLoadVoltage;
        private readonly IReadOnlyDictionary<string, float> perBranchCurrent;
        private readonly bool isFlowing;
        private readonly IReadOnlyList<string> activeWireIds;
        private readonly IReadOnlyList<CurrentFlowWireDirection> wireDirections;

        public CircuitState State => state;
        public bool HasValidCircuit => hasValidCircuit;
        public int ActiveLoadCount => activeLoadCount;
        public float TotalVoltage => totalVoltage;
        public float TotalResistance => totalResistance;
        public float TotalCurrent => totalCurrent;
        public IReadOnlyDictionary<string, float> PerLoadVoltage => perLoadVoltage;
        public IReadOnlyDictionary<string, float> PerBranchCurrent => perBranchCurrent;
        public bool IsFlowing => isFlowing;
        public IReadOnlyList<string> ActiveWireIds => activeWireIds;
        public IReadOnlyList<CurrentFlowWireDirection> WireDirections => wireDirections;

        public CircuitUIData(
            CircuitState state,
            bool hasValidCircuit,
            int activeLoadCount,
            float totalVoltage,
            float totalResistance,
            float totalCurrent,
            IReadOnlyDictionary<string, float> perLoadVoltage,
            IReadOnlyDictionary<string, float> perBranchCurrent,
            bool isFlowing,
            IReadOnlyList<string> activeWireIds,
            IReadOnlyList<CurrentFlowWireDirection> wireDirections)
        {
            this.state = state;
            this.hasValidCircuit = hasValidCircuit;
            this.activeLoadCount = activeLoadCount;
            this.totalVoltage = totalVoltage;
            this.totalResistance = totalResistance;
            this.totalCurrent = totalCurrent;
            this.perLoadVoltage = perLoadVoltage ?? new Dictionary<string, float>();
            this.perBranchCurrent = perBranchCurrent ?? new Dictionary<string, float>();
            this.isFlowing = isFlowing;
            this.activeWireIds = activeWireIds ?? new List<string>();
            this.wireDirections = wireDirections ?? new List<CurrentFlowWireDirection>();
        }
    }

    public class CircuitQueryService : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private bool enableDebugLogs = false;

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }
        }

        public CircuitState GetCircuitState()
        {
            if (!TryGetContext(out var context))
            {
                return CircuitState.Open;
            }

            CircuitEvaluator evaluator = new CircuitEvaluator(context);
            return evaluator.Evaluate();
        }

        public CircuitCalculationResult GetCalculationResult()
        {
            if (!TryGetContext(out var context))
            {
                return new CircuitCalculationResult();
            }

            CircuitCalculationHelper calculationHelper = new CircuitCalculationHelper(context);
            return calculationHelper.Calculate() ?? new CircuitCalculationResult();
        }

        public CircuitCurrentFlowResult GetCurrentFlowResult()
        {
            if (!TryGetContext(out var context))
            {
                return new CircuitCurrentFlowResult(false, new List<string>(), new List<CurrentFlowWireDirection>());
            }

            CircuitCurrentFlowAnalyzer flowAnalyzer = new CircuitCurrentFlowAnalyzer(context);
            return flowAnalyzer.Analyze() ?? new CircuitCurrentFlowResult(false, new List<string>(), new List<CurrentFlowWireDirection>());
        }

        public CircuitUIData GetUIData()
        {
            if (!TryGetContext(out var context))
            {
                return CreateDefaultUIData();
            }

            CircuitEvaluator evaluator = new CircuitEvaluator(context);
            CircuitCalculationHelper calculationHelper = new CircuitCalculationHelper(context);
            CircuitCurrentFlowAnalyzer flowAnalyzer = new CircuitCurrentFlowAnalyzer(context);

            CircuitState state = evaluator.Evaluate();
            CircuitCalculationResult calculationResult = calculationHelper.Calculate() ?? new CircuitCalculationResult();
            CircuitCurrentFlowResult flowResult = flowAnalyzer.Analyze() ?? new CircuitCurrentFlowResult(false, new List<string>(), new List<CurrentFlowWireDirection>());

            return new CircuitUIData(
                state,
                calculationResult.HasValidCircuit,
                calculationResult.ActiveLoadCount,
                calculationResult.TotalVoltage,
                calculationResult.TotalResistance,
                calculationResult.TotalCurrent,
                calculationResult.PerLoadVoltage,
                calculationResult.PerBranchCurrent,
                flowResult.IsFlowing,
                flowResult.ActiveWireIds,
                flowResult.WireDirections);
        }

        private bool TryGetContext(out VRCircuit.Data.CircuitContext context)
        {
            context = null;

            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }

            if (runtimeRoot == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("CircuitQueryService: CircuitRuntimeRoot is missing.");
                }

                return false;
            }

            runtimeRoot.EnsureInitialized();

            context = runtimeRoot.Context;
            if (context == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("CircuitQueryService: CircuitContext is missing.");
                }

                return false;
            }

            return true;
        }

        private CircuitUIData CreateDefaultUIData()
        {
            return new CircuitUIData(
                CircuitState.Open,
                false,
                0,
                0f,
                0f,
                0f,
                new Dictionary<string, float>(),
                new Dictionary<string, float>(),
                false,
                new List<string>(),
                new List<CurrentFlowWireDirection>());
        }
    }
}
