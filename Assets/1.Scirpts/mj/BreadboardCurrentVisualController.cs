using System;
using System.Collections.Generic;
using UnityEngine;
using VRCircuit.Analysis;
using VRCircuit.Board;
using VRCircuit.Runtime;

namespace VRCircuit.Effects
{
    [Serializable]
    public class BreadboardNodeVisualBinding
    {
        public string nodeId;
        public HoleGroup holeGroup;
    }

    public class BreadboardCurrentVisualController : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private List<BreadboardNodeVisualBinding> nodeBindings = new List<BreadboardNodeVisualBinding>();
        [SerializeField] private bool autoBindHoleGroups = true;
        [SerializeField] private Transform autoBindRoot;
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private bool enableDebugLogs = false;

        private readonly Dictionary<string, List<HoleGroup>> holeGroupsByNodeId = new Dictionary<string, List<HoleGroup>>();
        private readonly Dictionary<string, bool> lastIsOnByNodeId = new Dictionary<string, bool>();

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }
        }

        private void Start()
        {
            if (autoBindHoleGroups)
            {
                AutoBindHoleGroups();
            }
        }

        private void Update()
        {
            if (!updateEveryFrame)
            {
                return;
            }

            RefreshVisuals();
        }

        public void RefreshVisuals()
        {
            EnsureAutoBindingsIfNeeded();

            if (runtimeRoot == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("BreadboardCurrentVisualController: CircuitRuntimeRoot is missing.");
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
                    Debug.LogWarning("BreadboardCurrentVisualController: CircuitContext is missing.");
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

            HashSet<string> activeNodeIds = CollectActiveNodeIds(flowResult);
            Dictionary<string, List<HoleGroup>> allBindingsByNodeId = BuildCombinedBindingsByNodeId();

            foreach (KeyValuePair<string, List<HoleGroup>> pair in allBindingsByNodeId)
            {
                string nodeId = pair.Key;
                bool shouldBeOn = activeNodeIds.Contains(nodeId);

                ApplyNodeState(nodeId, pair.Value, shouldBeOn);
            }
        }

        public void AutoBindHoleGroups()
        {
            holeGroupsByNodeId.Clear();
            lastIsOnByNodeId.Clear();

            HoleTrigger[] holeTriggers = FindHoleTriggersForAutoBinding();
            int bindingCount = 0;

            for (int i = 0; i < holeTriggers.Length; i++)
            {
                HoleTrigger holeTrigger = holeTriggers[i];
                if (holeTrigger == null)
                {
                    continue;
                }

                if (!BreadboardNodeMapper.TryGetNodeId(holeTrigger.holeIndex, out string nodeId) ||
                    string.IsNullOrEmpty(nodeId))
                {
                    continue;
                }

                HoleGroup holeGroup = holeTrigger.GetComponentInParent<HoleGroup>();
                if (holeGroup == null)
                {
                    continue;
                }

                if (!holeGroupsByNodeId.TryGetValue(nodeId, out List<HoleGroup> holeGroups))
                {
                    holeGroups = new List<HoleGroup>();
                    holeGroupsByNodeId[nodeId] = holeGroups;
                }

                if (holeGroups.Contains(holeGroup))
                {
                    continue;
                }

                holeGroups.Add(holeGroup);
                bindingCount++;
            }

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BreadboardCurrentVisual] Auto bind complete. nodeCount={holeGroupsByNodeId.Count}, bindingCount={bindingCount}");
            }
        }

        private void EnsureAutoBindingsIfNeeded()
        {
            if (!autoBindHoleGroups || holeGroupsByNodeId.Count > 0)
            {
                return;
            }

            AutoBindHoleGroups();
        }

        private HoleTrigger[] FindHoleTriggersForAutoBinding()
        {
            if (autoBindRoot != null)
            {
                return autoBindRoot.GetComponentsInChildren<HoleTrigger>(true);
            }

            return FindObjectsByType<HoleTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private HashSet<string> CollectActiveNodeIds(CircuitCurrentFlowResult flowResult)
        {
            HashSet<string> activeNodeIds = new HashSet<string>();

            if (flowResult == null || flowResult.WireDirections == null)
            {
                return activeNodeIds;
            }

            for (int i = 0; i < flowResult.WireDirections.Count; i++)
            {
                CurrentFlowWireDirection direction = flowResult.WireDirections[i];
                if (direction == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(direction.FromNodeId))
                {
                    activeNodeIds.Add(direction.FromNodeId);
                }

                if (!string.IsNullOrEmpty(direction.ToNodeId))
                {
                    activeNodeIds.Add(direction.ToNodeId);
                }
            }

            return activeNodeIds;
        }

        private Dictionary<string, List<HoleGroup>> BuildCombinedBindingsByNodeId()
        {
            Dictionary<string, List<HoleGroup>> combinedBindings = new Dictionary<string, List<HoleGroup>>();

            foreach (KeyValuePair<string, List<HoleGroup>> pair in holeGroupsByNodeId)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Value == null)
                {
                    continue;
                }

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    AddHoleGroupBinding(combinedBindings, pair.Key, pair.Value[i]);
                }
            }

            for (int i = 0; i < nodeBindings.Count; i++)
            {
                BreadboardNodeVisualBinding binding = nodeBindings[i];
                if (binding == null)
                {
                    continue;
                }

                AddHoleGroupBinding(combinedBindings, binding.nodeId, binding.holeGroup);
            }

            return combinedBindings;
        }

        private void AddHoleGroupBinding(
            Dictionary<string, List<HoleGroup>> bindingsByNodeId,
            string nodeId,
            HoleGroup holeGroup)
        {
            if (bindingsByNodeId == null ||
                string.IsNullOrEmpty(nodeId) ||
                holeGroup == null)
            {
                return;
            }

            if (!bindingsByNodeId.TryGetValue(nodeId, out List<HoleGroup> holeGroups))
            {
                holeGroups = new List<HoleGroup>();
                bindingsByNodeId[nodeId] = holeGroups;
            }

            if (!holeGroups.Contains(holeGroup))
            {
                holeGroups.Add(holeGroup);
            }
        }

        private void ApplyNodeState(string nodeId, List<HoleGroup> holeGroups, bool isActive)
        {
            if (string.IsNullOrEmpty(nodeId) || holeGroups == null || holeGroups.Count == 0)
            {
                return;
            }

            bool hasCachedState = lastIsOnByNodeId.TryGetValue(nodeId, out bool cachedIsOn);
            if (hasCachedState && cachedIsOn == isActive)
            {
                return;
            }

            for (int i = 0; i < holeGroups.Count; i++)
            {
                HoleGroup holeGroup = holeGroups[i];
                if (holeGroup == null)
                {
                    continue;
                }

                holeGroup.UpdateEffect(isActive);
            }

            lastIsOnByNodeId[nodeId] = isActive;

            if (enableDebugLogs)
            {
                Debug.Log($"[BreadboardCurrentVisual] {(isActive ? "ON" : "OFF")} nodeId={nodeId}, holeGroupCount={holeGroups.Count}");
            }
        }

        public void ForceTurnOffAll()
        {
            EnsureAutoBindingsIfNeeded();

            Dictionary<string, List<HoleGroup>> allBindingsByNodeId = BuildCombinedBindingsByNodeId();

            foreach (KeyValuePair<string, List<HoleGroup>> pair in allBindingsByNodeId)
            {
                List<HoleGroup> holeGroups = pair.Value;
                if (holeGroups == null)
                {
                    continue;
                }

                for (int i = 0; i < holeGroups.Count; i++)
                {
                    HoleGroup holeGroup = holeGroups[i];
                    if (holeGroup != null)
                    {
                        holeGroup.UpdateEffect(false);
                    }
                }
            }

            lastIsOnByNodeId.Clear();
        }

        private void TurnOffAll()
        {
            Dictionary<string, List<HoleGroup>> allBindingsByNodeId = BuildCombinedBindingsByNodeId();

            foreach (KeyValuePair<string, List<HoleGroup>> pair in allBindingsByNodeId)
            {
                ApplyNodeState(pair.Key, pair.Value, false);
            }
        }
    }
}

