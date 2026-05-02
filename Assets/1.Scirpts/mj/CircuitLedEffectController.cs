using System;
using System.Collections.Generic;
using UnityEngine;
using VRCircuit.Evaluation;
using VRCircuit.Runtime;

namespace VRCircuit.Effects
{
    [Serializable]
    public class LedEffectBinding
    {
        public string ledId;
        public LedPart ledPart;
    }

    public class CircuitLedEffectController : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private List<LedEffectBinding> ledBindings = new List<LedEffectBinding>();
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private bool enableDebugLogs = false;

        private readonly Dictionary<string, bool> lastIsOnByLedId = new Dictionary<string, bool>();

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

            RefreshLedEffects();
        }

        public void RefreshLedEffects()
        {
            if (runtimeRoot == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("CircuitLedEffectController: CircuitRuntimeRoot is missing.");
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
                    Debug.LogWarning("CircuitLedEffectController: CircuitContext is missing.");
                }

                TurnOffAll();
                return;
            }

            CircuitEvaluator evaluator = new CircuitEvaluator(context);
            CircuitState state = evaluator.Evaluate();

            bool shouldTurnOn = state == CircuitState.LedOn;

            for (int i = 0; i < ledBindings.Count; i++)
            {
                LedEffectBinding binding = ledBindings[i];
                if (binding == null || binding.ledPart == null || string.IsNullOrEmpty(binding.ledId))
                {
                    continue;
                }

                ApplyLedState(binding, shouldTurnOn);
            }
        }

        private void ApplyLedState(LedEffectBinding binding, bool isOn)
        {
            if (binding == null || binding.ledPart == null || string.IsNullOrEmpty(binding.ledId))
            {
                return;
            }

            bool hasCachedState = lastIsOnByLedId.TryGetValue(binding.ledId, out bool cachedIsOn);
            if (hasCachedState && cachedIsOn == isOn)
            {
                return;
            }

            binding.ledPart.manageLight(isOn);
            lastIsOnByLedId[binding.ledId] = isOn;

            if (enableDebugLogs)
            {
                Debug.Log($"[LedEffect] ledId={binding.ledId} isOn={isOn}");
            }
        }

        private void TurnOffAll()
        {
            for (int i = 0; i < ledBindings.Count; i++)
            {
                LedEffectBinding binding = ledBindings[i];
                if (binding == null || binding.ledPart == null || string.IsNullOrEmpty(binding.ledId))
                {
                    continue;
                }

                ApplyLedState(binding, false);
            }
        }
    }
}
