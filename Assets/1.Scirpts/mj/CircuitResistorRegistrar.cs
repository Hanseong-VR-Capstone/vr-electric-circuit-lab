using UnityEngine;
using VRCircuit.Board;
using VRCircuit.Data;
using VRCircuit.Runtime;

namespace VRCircuit.Registration
{
    public class CircuitResistorRegistrar : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private ResistorPart resistorPart;
        [SerializeField] private PartPin pinA;
        [SerializeField] private PartPin pinB;
        [SerializeField] private string resistorIdOverride;

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }

            if (resistorPart == null)
            {
                resistorPart = GetComponent<ResistorPart>();
            }

            ResolvePinsIfNeeded();
        }

        private void Start()
        {
            RegisterResistor();
        }

        public void RegisterResistor()
        {
            if (runtimeRoot == null || runtimeRoot.Context == null)
            {
                Debug.LogWarning("CircuitResistorRegistrar: CircuitRuntimeRoot or Context is missing.");
                return;
            }

            if (resistorPart == null)
            {
                Debug.LogWarning("CircuitResistorRegistrar: ResistorPart is missing.");
                return;
            }

            if (pinA == null || pinB == null)
            {
                Debug.LogWarning("CircuitResistorRegistrar: Resistor requires two PartPin references.");
                return;
            }

            if (!TryResolveResistorId(out string resistorId))
            {
                Debug.LogWarning("CircuitResistorRegistrar: Failed to resolve resistorId.");
                return;
            }

            if (runtimeRoot.Context.GetResistorById(resistorId) != null)
            {
                return;
            }

            if (!TryResolvePinId(pinA, out string pinAId) ||
                !TryResolvePinId(pinB, out string pinBId))
            {
                Debug.LogWarning($"CircuitResistorRegistrar: Failed to resolve resistor pin IDs. resistorId={resistorId}");
                return;
            }

            if (resistorPart.resistance <= 0f)
            {
                Debug.LogWarning($"CircuitResistorRegistrar: Invalid resistance value. resistorId={resistorId}");
                return;
            }

            CircuitResistor resistor = new CircuitResistor(
                resistorId,
                pinAId,
                pinBId,
                resistorPart.resistance);

            runtimeRoot.Context.AddResistor(resistor);
            Debug.Log($"[Circuit] Registered resistor | resistorId={resistorId} | pinA={pinAId} | pinB={pinBId} | resistance={resistorPart.resistance}");
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

        private bool TryResolveResistorId(out string resistorId)
        {
            resistorId = null;

            if (!string.IsNullOrEmpty(resistorIdOverride))
            {
                resistorId = resistorIdOverride;
                return true;
            }

            if (resistorPart == null || string.IsNullOrEmpty(resistorPart.name))
            {
                return false;
            }

            resistorId = resistorPart.name;
            return true;
        }

        private bool TryResolvePinId(PartPin partPin, out string pinId)
        {
            pinId = null;

            if (partPin == null)
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

            if (partPin.parentPart == null || string.IsNullOrEmpty(partPin.parentPart.name))
            {
                return false;
            }

            pinId = $"{partPin.parentPart.name}_{partPin.pinRole}";
            return true;
        }
    }
}
