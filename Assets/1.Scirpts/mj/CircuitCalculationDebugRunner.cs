using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRCircuit.Calculation;
using VRCircuit.Data;
using VRCircuit.Evaluation;
using VRCircuit.Runtime;

namespace VRCircuit.Debugging
{
    public class CircuitCalculationDebugRunner : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private bool runOnStart;
        [SerializeField] private KeyCode runKey = KeyCode.C;

        private CircuitEvaluator evaluator;
        private CircuitCalculationHelper calculationHelper;

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }

            if (runtimeRoot == null)
            {
                Debug.LogWarning("CircuitCalculationDebugRunner: CircuitRuntimeRoot is missing.");
                return;
            }

            evaluator = new CircuitEvaluator(runtimeRoot.Context);
            calculationHelper = new CircuitCalculationHelper(runtimeRoot.Context);
        }

        private void Start()
        {
            if (runOnStart)
            {
                PrintCalculationSummary();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(runKey))
            {
                PrintCalculationSummary();
            }
        }

        public void PrintCalculationSummary()
        {
            if (runtimeRoot == null || runtimeRoot.Context == null)
            {
                Debug.LogWarning("CircuitCalculationDebugRunner: Runtime context is missing.");
                return;
            }

            if (evaluator == null || calculationHelper == null)
            {
                evaluator = new CircuitEvaluator(runtimeRoot.Context);
                calculationHelper = new CircuitCalculationHelper(runtimeRoot.Context);
            }

            CircuitState state = evaluator.Evaluate();
            CircuitCalculationResult result = calculationHelper.Calculate();

            Debug.Log(BuildSummary(state, runtimeRoot.Context, result));
        }

        private string BuildSummary(
            CircuitState state,
            CircuitContext context,
            CircuitCalculationResult result)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("[Calculation Debug]");
            builder.AppendLine($"State: {state}");
            builder.AppendLine($"RegisteredResistorCount: {GetRegisteredResistorCount(context)}");

            if (result == null)
            {
                builder.AppendLine("Result: NULL");
                return builder.ToString();
            }

            builder.AppendLine($"HasValidCircuit: {result.HasValidCircuit}");
            builder.AppendLine($"ActiveLoadCount: {result.ActiveLoadCount}");
            builder.AppendLine($"TotalVoltage: {result.TotalVoltage}");
            builder.AppendLine($"TotalResistance: {result.TotalResistance}");
            builder.AppendLine($"TotalCurrent: {result.TotalCurrent}");

            AppendFloatDictionary(builder, "PerLoadVoltage", result.PerLoadVoltage);
            AppendFloatDictionary(builder, "PerBranchCurrent", result.PerBranchCurrent);

            return builder.ToString();
        }

        private int GetRegisteredResistorCount(CircuitContext context)
        {
            if (context == null || context.Resistors == null)
            {
                return 0;
            }

            return context.Resistors.Count;
        }

        private void AppendFloatDictionary(
            StringBuilder builder,
            string title,
            IReadOnlyDictionary<string, float> values)
        {
            builder.AppendLine($"{title}:");

            if (values == null || values.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            foreach (KeyValuePair<string, float> pair in values)
            {
                builder.AppendLine($"- {pair.Key}: {pair.Value}");
            }
        }
    }
}
