using System;
using System.Collections.Generic;
using UnityEngine;
using VRCircuit.Analysis;
using VRCircuit.Runtime;

namespace VRCircuit.Effects
{
    [Serializable]
    public class WireLightBinding
    {
        public string wireId;
        public JumperWireLight jumperWireLight;
    }

    public class CircuitWireEffectController : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private List<WireLightBinding> wireBindings = new List<WireLightBinding>();
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private bool enableDebugLogs = false;

        private readonly Dictionary<string, bool> lastIsOnByWireId = new Dictionary<string, bool>();
        private readonly Dictionary<string, PinRole> lastStartRoleByWireId = new Dictionary<string, PinRole>();

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }
        }

        private void Update()
        {
            if (!updateEveryFrame)
            {
                return;
            }

            RefreshEffects();
        }

        public void RefreshEffects()
        {
            if (runtimeRoot == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("CircuitWireEffectController: CircuitRuntimeRoot is missing.");
                }

                TurnOffAll();
                return;
            }

            runtimeRoot.EnsureInitialized();

            var context = runtimeRoot.Context;
            if (context == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("CircuitWireEffectController: CircuitContext is missing.");
                }

                TurnOffAll();
                return;
            }

            CircuitCurrentFlowAnalyzer flowAnalyzer = new CircuitCurrentFlowAnalyzer(context);
            CircuitCurrentFlowResult flowResult = flowAnalyzer.Analyze();

            if (flowResult == null || !flowResult.IsFlowing)
            {
                TurnOffAll();
                return;
            }

            for (int i = 0; i < wireBindings.Count; i++)
            {
                WireLightBinding binding = wireBindings[i];
                if (binding == null || binding.jumperWireLight == null)
                {
                    continue;
                }

                if (!TryFindDirection(flowResult, binding.wireId, out CurrentFlowWireDirection direction))
                {
                    ApplyWireOff(binding);
                    continue;
                }

                if (!TryResolveStartPinRole(direction.FromPinId, out PinRole startPinRole))
                {
                    ApplyWireOff(binding);

                    if (enableDebugLogs)
                    {
                        Debug.LogWarning(
                            $"CircuitWireEffectController: Failed to resolve start pin role. wireId={binding.wireId}, fromPinId={direction.FromPinId}");
                    }

                    continue;
                }

                ApplyWireOn(binding, startPinRole);
            }
        }

        private bool TryFindDirection(
            CircuitCurrentFlowResult flowResult,
            string wireId,
            out CurrentFlowWireDirection direction)
        {
            direction = null;

            if (flowResult == null ||
                flowResult.WireDirections == null ||
                string.IsNullOrEmpty(wireId))
            {
                return false;
            }

            for (int i = 0; i < flowResult.WireDirections.Count; i++)
            {
                CurrentFlowWireDirection candidate = flowResult.WireDirections[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.WireId == wireId)
                {
                    direction = candidate;
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveStartPinRole(string fromPinId, out PinRole startPinRole)
        {
            startPinRole = PinRole.None;

            if (string.IsNullOrEmpty(fromPinId))
            {
                return false;
            }

            if (fromPinId.EndsWith("Wire_A", StringComparison.Ordinal))
            {
                startPinRole = PinRole.Wire_A;
                return true;
            }

            if (fromPinId.EndsWith("Wire_B", StringComparison.Ordinal))
            {
                startPinRole = PinRole.Wire_B;
                return true;
            }

            return false;
        }

        private void ApplyWireOn(WireLightBinding binding, PinRole startPinRole)
        {
            if (binding == null || binding.jumperWireLight == null || string.IsNullOrEmpty(binding.wireId))
            {
                return;
            }

            bool wasOn = lastIsOnByWireId.TryGetValue(binding.wireId, out bool cachedIsOn) && cachedIsOn;
            bool hasCachedRole = lastStartRoleByWireId.TryGetValue(binding.wireId, out PinRole cachedRole);

            if (wasOn && hasCachedRole && cachedRole == startPinRole)
            {
                return;
            }

            binding.jumperWireLight.LightOn(startPinRole);
            lastIsOnByWireId[binding.wireId] = true;
            lastStartRoleByWireId[binding.wireId] = startPinRole;

            if (enableDebugLogs)
            {
                Debug.Log($"[WireEffect] ON wireId={binding.wireId} startPinRole={startPinRole}");
            }
        }

        private void ApplyWireOff(WireLightBinding binding)
        {
            if (binding == null || binding.jumperWireLight == null || string.IsNullOrEmpty(binding.wireId))
            {
                return;
            }

            bool wasOn = !lastIsOnByWireId.TryGetValue(binding.wireId, out bool cachedIsOn) || cachedIsOn;
            if (!wasOn)
            {
                return;
            }

            binding.jumperWireLight.LightOff();
            lastIsOnByWireId[binding.wireId] = false;
            lastStartRoleByWireId[binding.wireId] = PinRole.None;

            if (enableDebugLogs)
            {
                Debug.Log($"[WireEffect] OFF wireId={binding.wireId}");
            }
        }

        private void TurnOffAll()
        {
            for (int i = 0; i < wireBindings.Count; i++)
            {
                WireLightBinding binding = wireBindings[i];
                if (binding == null || binding.jumperWireLight == null)
                {
                    continue;
                }

                ApplyWireOff(binding);
            }
        }
    }
}
