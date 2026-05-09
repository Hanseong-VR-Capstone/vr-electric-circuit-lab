using System.Collections.Generic;
using UnityEngine;
using VRCircuit.Effects;
using VRCircuit.Runtime;

namespace VRCircuit.Registration
{
    public class CircuitSpawnedPartInitializer : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private bool renameSpawnedObject = true;
        [SerializeField] private bool enableDebugLogs = false;

        private static readonly Dictionary<string, int> CountersByBaseId = new Dictionary<string, int>();

        private bool isInitialized;
        private string uniquePartId;

        public string UniquePartId => uniquePartId;

        public static bool ContainsCircuitRegistrar(GameObject root)
        {
            if (root == null)
            {
                return false;
            }

            return root.GetComponentInChildren<CircuitWireRegistrar>(true) != null ||
                   root.GetComponentInChildren<CircuitResistorRegistrar>(true) != null ||
                   root.GetComponentInChildren<CircuitLedRegistrar>(true) != null ||
                   root.GetComponentInChildren<CircuitSwitchRegistrar>(true) != null;
        }

        public void InitializeSpawnedPart(CircuitRuntimeRoot injectedRuntimeRoot, string basePartId)
        {
            if (isInitialized)
            {
                return;
            }

            if (!ContainsCircuitRegistrar(gameObject))
            {
                return;
            }

            runtimeRoot = injectedRuntimeRoot != null ? injectedRuntimeRoot : runtimeRoot;
            if (runtimeRoot == null)
            {
                runtimeRoot = FindFirstObjectByType<CircuitRuntimeRoot>();
            }

            if (runtimeRoot == null)
            {
                Debug.LogWarning($"CircuitSpawnedPartInitializer: CircuitRuntimeRoot not found. object={name}");
                return;
            }

            runtimeRoot.EnsureInitialized();

            uniquePartId = GenerateUniquePartId(basePartId);

            if (renameSpawnedObject)
            {
                gameObject.name = uniquePartId;
                RenameCircuitParts(uniquePartId);
            }

            InitializeRegistrars(uniquePartId);
            RegisterEffectBindings(uniquePartId);
            isInitialized = true;

            if (enableDebugLogs)
            {
                Debug.Log($"[SpawnedPartInitializer] initialized | object={name} | uniquePartId={uniquePartId}");
            }
        }

        private static string GenerateUniquePartId(string basePartId)
        {
            string normalizedBaseId = NormalizeBaseId(basePartId);

            if (!CountersByBaseId.TryGetValue(normalizedBaseId, out int counter))
            {
                counter = 0;
            }

            counter++;
            CountersByBaseId[normalizedBaseId] = counter;

            return $"{normalizedBaseId}_{counter:000}";
        }

        private static string NormalizeBaseId(string basePartId)
        {
            if (string.IsNullOrEmpty(basePartId))
            {
                return "CircuitPart";
            }

            return basePartId.Replace("(Clone)", string.Empty).Trim();
        }

        private void RenameCircuitParts(string partId)
        {
            if (string.IsNullOrEmpty(partId))
            {
                return;
            }

            CircuitPart[] circuitParts = GetComponentsInChildren<CircuitPart>(true);
            if (circuitParts == null)
            {
                return;
            }

            for (int i = 0; i < circuitParts.Length; i++)
            {
                CircuitPart circuitPart = circuitParts[i];
                if (circuitPart == null)
                {
                    continue;
                }

                circuitPart.name = partId;
            }
        }

        private void InitializeRegistrars(string idOverride)
        {
            CircuitWireRegistrar[] wireRegistrars = GetComponentsInChildren<CircuitWireRegistrar>(true);
            for (int i = 0; i < wireRegistrars.Length; i++)
            {
                if (wireRegistrars[i] != null)
                {
                    wireRegistrars[i].InitializeForSpawnedPart(runtimeRoot, idOverride);
                }
            }

            CircuitResistorRegistrar[] resistorRegistrars = GetComponentsInChildren<CircuitResistorRegistrar>(true);
            for (int i = 0; i < resistorRegistrars.Length; i++)
            {
                if (resistorRegistrars[i] != null)
                {
                    resistorRegistrars[i].InitializeForSpawnedPart(runtimeRoot, idOverride);
                }
            }

            CircuitLedRegistrar[] ledRegistrars = GetComponentsInChildren<CircuitLedRegistrar>(true);
            for (int i = 0; i < ledRegistrars.Length; i++)
            {
                if (ledRegistrars[i] != null)
                {
                    ledRegistrars[i].InitializeForSpawnedPart(runtimeRoot, idOverride);
                }
            }

            CircuitSwitchRegistrar[] switchRegistrars = GetComponentsInChildren<CircuitSwitchRegistrar>(true);
            for (int i = 0; i < switchRegistrars.Length; i++)
            {
                if (switchRegistrars[i] != null)
                {
                    switchRegistrars[i].InitializeForSpawnedPart(runtimeRoot, idOverride);
                }
            }
        }

        private void RegisterEffectBindings(string idOverride)
        {
            if (string.IsNullOrEmpty(idOverride))
            {
                return;
            }

            RegisterWireEffectBinding(idOverride);
            RegisterLedEffectBinding(idOverride);
        }

        private void RegisterWireEffectBinding(string wireId)
        {
            JumperWireLight jumperWireLight = GetComponentInChildren<JumperWireLight>(true);
            if (jumperWireLight == null)
            {
                return;
            }

            CircuitWireEffectController wireEffectController = FindFirstObjectByType<CircuitWireEffectController>();
            if (wireEffectController == null)
            {
                return;
            }

            wireEffectController.RegisterWireBinding(wireId, jumperWireLight);

            if (enableDebugLogs)
            {
                Debug.Log($"[SpawnedPartInitializer] wire effect bound | wireId={wireId}");
            }
        }

        private void RegisterLedEffectBinding(string ledId)
        {
            LedPart ledPart = GetComponentInChildren<LedPart>(true);
            if (ledPart == null)
            {
                return;
            }

            CircuitLedEffectController ledEffectController = FindFirstObjectByType<CircuitLedEffectController>();
            if (ledEffectController == null)
            {
                return;
            }

            ledEffectController.RegisterLedBinding(ledId, ledPart);

            if (enableDebugLogs)
            {
                Debug.Log($"[SpawnedPartInitializer] led effect bound | ledId={ledId}");
            }
        }
    }
}
