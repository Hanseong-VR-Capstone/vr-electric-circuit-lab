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
        [SerializeField] private bool enableDebugLogs = true;

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
            CircuitContext context = runtimeRoot != null ? runtimeRoot.Context : null;

            if (context == null)
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

            bool resistorAlreadyExists = context.GetResistorById(resistorId) != null;

            if (!TryResolvePinId(pinA, out string pinAId) ||
                !TryResolvePinId(pinB, out string pinBId))
            {
                Debug.LogWarning($"CircuitResistorRegistrar: Failed to resolve resistor pin IDs. resistorId={resistorId}");
                return;
            }

            bool pinAExistsAtRegistration = context.GetPinById(pinAId) != null;
            bool pinBExistsAtRegistration = context.GetPinById(pinBId) != null;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[ResistorRegistrarDebug] register attempt | object={name} | resistorId={resistorId} | " +
                    $"resistanceOhms={resistorPart.resistance} | pinAId={pinAId} | pinBId={pinBId} | " +
                    $"contextExists={(context != null)} | pinsCount={context.Pins.Count} | " +
                    $"pinAExistsAtRegistration={pinAExistsAtRegistration} | pinBExistsAtRegistration={pinBExistsAtRegistration} | " +
                    $"resistorAlreadyExists={resistorAlreadyExists}");
            }

            if (resistorAlreadyExists)
            {
                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[ResistorRegistrarDebug] register skipped | object={name} | resistorId={resistorId} already exists | " +
                        $"resistorsCount={context.Resistors.Count}");
                }

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

            context.AddResistor(resistor);

            bool finalRegistered = context.GetResistorById(resistorId) != null;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[ResistorRegistrarDebug] register result | object={name} | resistorId={resistorId} | " +
                    $"finalRegistered={finalRegistered} | resistorsCount={context.Resistors.Count}");
            }
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
