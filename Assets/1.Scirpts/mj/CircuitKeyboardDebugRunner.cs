using System;
using System.Collections.Generic;
using UnityEngine;
using VRCircuit.Data;
using VRCircuit.Debugging;
using VRCircuit.Evaluation;
using VRCircuit.Services;

namespace VRCircuit.Debugging
{
    public class CircuitKeyboardDebugRunner : MonoBehaviour
    {
        [Serializable]
        private class PinSocketBinding
        {
            [SerializeField] private string pinId;
            [SerializeField] private string socketId;

            public string PinId => pinId;
            public string SocketId => socketId;
        }

        [Serializable]
        private class DebugScenario
        {
            [SerializeField] private string scenarioName;
            [SerializeField] private KeyCode applyKey = KeyCode.None;
            [SerializeField] private bool applySwitchState;
            [SerializeField] private string switchId;
            [SerializeField] private bool switchState;
            [SerializeField] private List<PinSocketBinding> bindings = new List<PinSocketBinding>();

            public string ScenarioName => scenarioName;
            public KeyCode ApplyKey => applyKey;
            public bool ApplySwitchState => applySwitchState;
            public string SwitchId => switchId;
            public bool SwitchState => switchState;
            public IReadOnlyList<PinSocketBinding> Bindings => bindings;
        }

        [Header("Core Data")]
        [SerializeField] private CircuitContext context = new CircuitContext();

        [Header("Tracked Pins")]
        [SerializeField] private List<string> trackedPinIds = new List<string>();

        [Header("Scenarios")]
        [SerializeField] private List<DebugScenario> scenarios = new List<DebugScenario>();

        [Header("Debug Keys")]
        [SerializeField] private KeyCode printFullSummaryKey = KeyCode.Tab;
        [SerializeField] private KeyCode validateStateKey = KeyCode.V;

        [Header("Logging")]
        [SerializeField] private bool logAfterScenarioApply = true;

        private CircuitEvaluator evaluator;
        private CircuitConnectionService connectionService;
        private CircuitDebugValidator validator;

        private void Awake()
        {
            RebuildRuntimeObjects();
        }

        private void Update()
        {
            if (Input.GetKeyDown(printFullSummaryKey))
            {
                PrintFullSummary();
            }

            if (Input.GetKeyDown(validateStateKey))
            {
                ValidateCurrentState();
            }

            for (int i = 0; i < scenarios.Count; i++)
            {
                DebugScenario scenario = scenarios[i];
                if (scenario == null || scenario.ApplyKey == KeyCode.None)
                {
                    continue;
                }

                if (Input.GetKeyDown(scenario.ApplyKey))
                {
                    ApplyScenario(scenario);
                }
            }
        }

        [ContextMenu("Rebuild Runtime Objects")]
        private void RebuildRuntimeObjects()
        {
            evaluator = new CircuitEvaluator(context);
            connectionService = new CircuitConnectionService(context);
            validator = new CircuitDebugValidator(context, connectionService, evaluator);
        }

        [ContextMenu("Print Full Summary")]
        private void PrintFullSummary()
        {
            EnsureRuntimeObjects();

            if (validator == null)
            {
                Debug.LogWarning("CircuitKeyboardDebugRunner: Validator is not ready.");
                return;
            }

            Debug.Log(validator.GetFullSummary());
        }

        [ContextMenu("Validate Current State")]
        private void ValidateCurrentState()
        {
            EnsureRuntimeObjects();

            if (validator == null)
            {
                Debug.LogWarning("CircuitKeyboardDebugRunner: Validator is not ready.");
                return;
            }

            Debug.Log(validator.ValidateBasicState());
        }

        private void ApplyScenario(DebugScenario scenario)
        {
            EnsureRuntimeObjects();

            if (scenario == null)
            {
                Debug.LogWarning("CircuitKeyboardDebugRunner: Scenario is null.");
                return;
            }

            DisconnectTrackedPins();

            if (scenario.ApplySwitchState && !string.IsNullOrEmpty(scenario.SwitchId))
            {
                bool switchResult = connectionService.SwitchStateChanged(scenario.SwitchId, scenario.SwitchState);
                if (!switchResult)
                {
                    Debug.LogWarning($"CircuitKeyboardDebugRunner: Failed to set switch state. scenario={scenario.ScenarioName}, switchId={scenario.SwitchId}");
                }
            }

            IReadOnlyList<PinSocketBinding> bindings = scenario.Bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                PinSocketBinding binding = bindings[i];
                if (binding == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(binding.PinId) || string.IsNullOrEmpty(binding.SocketId))
                {
                    Debug.LogWarning($"CircuitKeyboardDebugRunner: Invalid binding in scenario {scenario.ScenarioName}.");
                    continue;
                }

                bool connected = connectionService.ConnectPinToSocket(binding.PinId, binding.SocketId);
                if (!connected)
                {
                    Debug.LogWarning($"CircuitKeyboardDebugRunner: Failed to connect pin to socket. scenario={scenario.ScenarioName}, pinId={binding.PinId}, socketId={binding.SocketId}");
                }
            }

            if (logAfterScenarioApply && validator != null)
            {
                Debug.Log($"[Scenario Applied] {scenario.ScenarioName}\n{validator.GetFullSummary()}");
            }
        }

        private void DisconnectTrackedPins()
        {
            if (connectionService == null)
            {
                return;
            }

            for (int i = 0; i < trackedPinIds.Count; i++)
            {
                string pinId = trackedPinIds[i];
                if (string.IsNullOrEmpty(pinId))
                {
                    continue;
                }

                connectionService.DisconnectPin(pinId);
            }
        }

        private void EnsureRuntimeObjects()
        {
            if (evaluator != null && connectionService != null && validator != null)
            {
                return;
            }

            RebuildRuntimeObjects();
        }
    }
}