using UnityEngine;
using VRCircuit.Board;
using VRCircuit.Data;
using VRCircuit.Runtime;

namespace VRCircuit.Registration
{
    public class CircuitLedRegistrar : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private LedPart ledPart;
        [SerializeField] private PartPin anodePin;
        [SerializeField] private PartPin cathodePin;
        [SerializeField] private string ledIdOverride;
        [SerializeField] private bool enableDebugLogs = true;

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }

            if (ledPart == null)
            {
                ledPart = GetComponent<LedPart>();
            }

            ResolvePinsIfNeeded();
        }

        private void Start()
        {
            RegisterLed();
        }

        public void InitializeForSpawnedPart(CircuitRuntimeRoot injectedRuntimeRoot, string idOverride)
        {
            if (injectedRuntimeRoot != null)
            {
                runtimeRoot = injectedRuntimeRoot;
                runtimeRoot.EnsureInitialized();
            }

            if (!string.IsNullOrEmpty(idOverride))
            {
                ledIdOverride = idOverride;
            }

            if (ledPart == null)
            {
                ledPart = GetComponent<LedPart>();
            }

            ResolvePinsIfNeeded();
            RegisterLed();
        }

        public void RegisterLed()
        {
            if (runtimeRoot != null)
            {
                runtimeRoot.EnsureInitialized();
            }

            CircuitContext context = runtimeRoot != null ? runtimeRoot.Context : null;

            if (context == null)
            {
                Debug.LogWarning("CircuitLedRegistrar: CircuitRuntimeRoot or Context is missing.");
                return;
            }

            if (ledPart == null)
            {
                Debug.LogWarning("CircuitLedRegistrar: LedPart is missing.");
                return;
            }

            if (anodePin == null || cathodePin == null)
            {
                Debug.LogWarning("CircuitLedRegistrar: LED requires two PartPin references.");
                return;
            }

            if (!TryResolveLedId(out string ledId))
            {
                Debug.LogWarning("CircuitLedRegistrar: Failed to resolve ledId.");
                return;
            }

            bool ledAlreadyExists = context.GetLedById(ledId) != null;

            if (!TryResolvePinId(anodePin, ledId, out string anodePinId) ||
                !TryResolvePinId(cathodePin, ledId, out string cathodePinId))
            {
                Debug.LogWarning($"CircuitLedRegistrar: Failed to resolve LED pin IDs. ledId={ledId}");
                return;
            }

            bool anodeExistsAtRegistration = context.GetPinById(anodePinId) != null;
            bool cathodeExistsAtRegistration = context.GetPinById(cathodePinId) != null;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[LedRegistrarDebug] register attempt | object={name} | ledId={ledId} | " +
                    $"anodePinId={anodePinId} | cathodePinId={cathodePinId} | " +
                    $"contextExists={(context != null)} | pinsCount={context.Pins.Count} | " +
                    $"anodeExistsAtRegistration={anodeExistsAtRegistration} | " +
                    $"cathodeExistsAtRegistration={cathodeExistsAtRegistration} | ledAlreadyExists={ledAlreadyExists}");
            }

            if (ledAlreadyExists)
            {
                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[LedRegistrarDebug] register skipped | object={name} | ledId={ledId} already exists | " +
                        $"ledsCount={context.Leds.Count}");
                }

                return;
            }

            CircuitLed led = new CircuitLed(
                ledId,
                anodePinId,
                cathodePinId);

            context.AddLed(led);

            bool finalRegistered = context.GetLedById(ledId) != null;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[LedRegistrarDebug] register result | object={name} | ledId={ledId} | " +
                    $"finalRegistered={finalRegistered} | ledsCount={context.Leds.Count}");
            }
        }

        private void ResolvePinsIfNeeded()
        {
            if (anodePin != null && cathodePin != null)
            {
                return;
            }

            PartPin[] pins = GetComponentsInChildren<PartPin>();
            if (pins == null || pins.Length < 2)
            {
                return;
            }

            for (int i = 0; i < pins.Length; i++)
            {
                PartPin pin = pins[i];
                if (pin == null)
                {
                    continue;
                }

                if (anodePin == null && pin.pinRole == PinRole.LED_Plus)
                {
                    anodePin = pin;
                    continue;
                }

                if (cathodePin == null && pin.pinRole == PinRole.LED_Minus)
                {
                    cathodePin = pin;
                }
            }

            if (anodePin == null)
            {
                anodePin = pins[0];
            }

            if (cathodePin == null)
            {
                cathodePin = pins[1];
            }
        }

        private bool TryResolveLedId(out string ledId)
        {
            ledId = null;

            if (!string.IsNullOrEmpty(ledIdOverride))
            {
                ledId = ledIdOverride;
                return true;
            }

            if (ledPart == null || string.IsNullOrEmpty(ledPart.name))
            {
                return false;
            }

            ledId = ledPart.name;
            return true;
        }

        private bool TryResolvePinId(PartPin partPin, string ownerId, out string pinId)
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

            if (!string.IsNullOrEmpty(ownerId))
            {
                pinId = $"{ownerId}_{partPin.pinRole}";
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
