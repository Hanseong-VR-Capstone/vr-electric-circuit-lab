using UnityEngine;
using VRCircuit.Board;
using VRCircuit.Data;
using VRCircuit.Runtime;

namespace VRCircuit.Registration
{
    public class CircuitWireRegistrar : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private PartPin pinA;
        [SerializeField] private PartPin pinB;
        [SerializeField] private string wireIdOverride;
        [SerializeField] private bool enableDebugLogs = true;

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }

            ResolvePinsIfNeeded();
        }

        private void Start()
        {
            RegisterWire();
        }

        public void RegisterWire()
        {
            CircuitContext context = runtimeRoot != null ? runtimeRoot.Context : null;

            if (context == null)
            {
                Debug.LogWarning("CircuitWireRegistrar: CircuitRuntimeRoot or Context is missing.");
                return;
            }

            if (pinA == null || pinB == null)
            {
                Debug.LogWarning("CircuitWireRegistrar: Wire requires two PartPin references.");
                return;
            }

            if (!TryGetResolvedWireId(out string wireId))
            {
                Debug.LogWarning("CircuitWireRegistrar: Failed to resolve wireId.");
                return;
            }

            bool wireAlreadyExists = context.GetWireById(wireId) != null;

            if (!TryResolveWirePinId(pinA, wireId, "Wire_A", out string pinAId) ||
                !TryResolveWirePinId(pinB, wireId, "Wire_B", out string pinBId))
            {
                Debug.LogWarning($"CircuitWireRegistrar: Failed to resolve wire pin IDs. wireId={wireId}");
                return;
            }

            bool pinAExistsAtRegistration = context.GetPinById(pinAId) != null;
            bool pinBExistsAtRegistration = context.GetPinById(pinBId) != null;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[WireRegistrarDebug] register attempt | object={name} | wireId={wireId} | " +
                    $"pinAId={pinAId} | pinBId={pinBId} | contextExists={(context != null)} | " +
                    $"pinsCount={context.Pins.Count} | pinAExistsAtRegistration={pinAExistsAtRegistration} | " +
                    $"pinBExistsAtRegistration={pinBExistsAtRegistration} | wireAlreadyExists={wireAlreadyExists}");
            }

            if (wireAlreadyExists)
            {
                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[WireRegistrarDebug] register skipped | object={name} | wireId={wireId} already exists | " +
                        $"wiresCount={context.Wires.Count}");
                }

                return;
            }

            CircuitWire wire = new CircuitWire(wireId, pinAId, pinBId);
            context.AddWire(wire);

            bool finalRegistered = context.GetWireById(wireId) != null;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[WireRegistrarDebug] register result | object={name} | wireId={wireId} | " +
                    $"finalRegistered={finalRegistered} | wiresCount={context.Wires.Count}");
            }
        }

        public bool TryGetResolvedWireId(out string wireId)
        {
            wireId = null;

            if (!string.IsNullOrEmpty(wireIdOverride))
            {
                wireId = wireIdOverride;
                return true;
            }

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            wireId = name;
            return true;
        }

        private void ResolvePinsIfNeeded()
        {
            if (pinA != null && pinB != null)
            {
                return;
            }

            PartPin[] pins = GetComponentsInChildren<PartPin>();
            if (pins == null || pins.Length < 2)
            {
                return;
            }

            if (pinA == null)
            {
                pinA = pins[0];
            }

            if (pinB == null)
            {
                pinB = pins[1];
            }
        }

        private bool TryResolveWirePinId(PartPin partPin, string wireId, string fallbackRole, out string pinId)
        {
            pinId = null;

            if (partPin == null || string.IsNullOrEmpty(wireId) || string.IsNullOrEmpty(fallbackRole))
            {
                return false;
            }

            CircuitPinIdAdapter adapter = partPin.GetComponent<CircuitPinIdAdapter>();
            if (adapter == null)
            {
                adapter = partPin.GetComponentInParent<CircuitPinIdAdapter>();
            }

            if (adapter != null && !string.IsNullOrEmpty(adapter.CircuitPinId))
            {
                pinId = adapter.CircuitPinId;
                return true;
            }

            pinId = $"{wireId}_{fallbackRole}";
            return true;
        }
    }
}
