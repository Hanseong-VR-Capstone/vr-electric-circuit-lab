using UnityEngine;
using VRCircuit.Board;
using VRCircuit.Data;
using VRCircuit.Runtime;
using VRCircuit.Services;

namespace VRCircuit.Registration
{
    public class CircuitSwitchRegistrar : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private SwitchPart switchPart;
        [SerializeField] private PartPin pin0;
        [SerializeField] private PartPin pin1;
        [SerializeField] private PartPin pin2;
        [SerializeField] private PartPin pin3;
        [SerializeField] private string switchIdOverride;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool syncSwitchStateEveryFrame = true;

        private string registeredSwitchId;
        private bool hasRegisteredSwitch;
        private bool lastIsOn;

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }

            if (switchPart == null)
            {
                switchPart = GetComponent<SwitchPart>();
            }

            ResolvePinsIfNeeded();
        }

        private void Start()
        {
            RegisterSwitch();
        }

        private void Update()
        {
            if (!syncSwitchStateEveryFrame)
            {
                return;
            }

            SyncSwitchStateIfChanged();
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
                switchIdOverride = idOverride;
            }

            if (switchPart == null)
            {
                switchPart = GetComponent<SwitchPart>();
            }

            ResolvePinsIfNeeded();
            RegisterSwitch();
        }

        public void RegisterSwitch()
        {
            if (runtimeRoot != null)
            {
                runtimeRoot.EnsureInitialized();
            }

            CircuitContext context = runtimeRoot != null ? runtimeRoot.Context : null;
            CircuitConnectionService connectionService = runtimeRoot != null ? runtimeRoot.ConnectionService : null;

            if (context == null || connectionService == null)
            {
                Debug.LogWarning("CircuitSwitchRegistrar: CircuitRuntimeRoot, Context, or ConnectionService is missing.");
                return;
            }

            if (switchPart == null)
            {
                Debug.LogWarning("CircuitSwitchRegistrar: SwitchPart is missing.");
                return;
            }

            if (pin0 == null || pin1 == null || pin2 == null || pin3 == null)
            {
                Debug.LogWarning("CircuitSwitchRegistrar: 4-pin switch requires pin0, pin1, pin2, and pin3 references.");
                return;
            }

            if (!TryResolveSwitchId(out string switchId))
            {
                Debug.LogWarning("CircuitSwitchRegistrar: Failed to resolve switchId.");
                return;
            }

            bool switchAlreadyExists = context.GetSwitchById(switchId) != null;

            if (!TryResolvePinId(pin0, switchId, out string pin0Id) ||
                !TryResolvePinId(pin1, switchId, out string pin1Id) ||
                !TryResolvePinId(pin2, switchId, out string pin2Id) ||
                !TryResolvePinId(pin3, switchId, out string pin3Id))
            {
                Debug.LogWarning($"CircuitSwitchRegistrar: Failed to resolve switch pin IDs. switchId={switchId}");
                return;
            }

            bool pin0ExistsAtRegistration = context.GetPinById(pin0Id) != null;
            bool pin1ExistsAtRegistration = context.GetPinById(pin1Id) != null;
            bool pin2ExistsAtRegistration = context.GetPinById(pin2Id) != null;
            bool pin3ExistsAtRegistration = context.GetPinById(pin3Id) != null;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[SwitchRegistrarDebug] register attempt | object={name} | switchId={switchId} | " +
                    $"pin0Id={pin0Id} | pin1Id={pin1Id} | pin2Id={pin2Id} | pin3Id={pin3Id} | " +
                    $"initialIsOn={switchPart.isOn} | contextExists={(context != null)} | pinsCount={context.Pins.Count} | " +
                    $"pin0ExistsAtRegistration={pin0ExistsAtRegistration} | pin1ExistsAtRegistration={pin1ExistsAtRegistration} | " +
                    $"pin2ExistsAtRegistration={pin2ExistsAtRegistration} | pin3ExistsAtRegistration={pin3ExistsAtRegistration} | " +
                    $"switchAlreadyExists={switchAlreadyExists}");
            }

            if (switchAlreadyExists)
            {
                CircuitSwitch existingSwitch = context.GetSwitchById(switchId);

                registeredSwitchId = switchId;
                hasRegisteredSwitch = true;
                lastIsOn = existingSwitch != null && existingSwitch.IsOn;

                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[SwitchRegistrarDebug] register skipped | object={name} | switchId={switchId} already exists | " +
                        $"switchesCount={context.Switches.Count}");
                }

                return;
            }

            CircuitSwitch circuitSwitch = new CircuitSwitch(
                switchId,
                pin0Id,
                pin1Id,
                pin2Id,
                pin3Id,
                switchPart.isOn);

            context.AddSwitch(circuitSwitch);

            bool finalRegistered = context.GetSwitchById(switchId) != null;

            registeredSwitchId = switchId;
            hasRegisteredSwitch = finalRegistered;
            lastIsOn = switchPart.isOn;

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[SwitchRegistrarDebug] register result | object={name} | switchId={switchId} | " +
                    $"finalRegistered={finalRegistered} | switchesCount={context.Switches.Count}");
            }
        }

        private void SyncSwitchStateIfChanged()
        {
            if (!hasRegisteredSwitch || string.IsNullOrEmpty(registeredSwitchId) || switchPart == null)
            {
                return;
            }

            bool currentIsOn = switchPart.isOn;
            if (currentIsOn == lastIsOn)
            {
                return;
            }

            CircuitConnectionService connectionService = runtimeRoot != null ? runtimeRoot.ConnectionService : null;
            if (connectionService == null)
            {
                Debug.LogWarning("CircuitSwitchRegistrar: ConnectionService is missing during switch state sync.");
                return;
            }

            bool result = connectionService.SwitchStateChanged(registeredSwitchId, currentIsOn);
            if (result)
            {
                lastIsOn = currentIsOn;
            }

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[SwitchRegistrarDebug] state sync | object={name} | switchId={registeredSwitchId} | " +
                    $"isOn={currentIsOn} | result={result}");
            }
        }

        private void ResolvePinsIfNeeded()
        {
            PartPin[] pins = GetComponentsInChildren<PartPin>();
            if (pins == null || pins.Length == 0)
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

                switch (pin.pinRole)
                {
                    case PinRole.Switch_Pin0:
                        if (pin0 == null)
                        {
                            pin0 = pin;
                        }
                        break;

                    case PinRole.Switch_Pin1:
                        if (pin1 == null)
                        {
                            pin1 = pin;
                        }
                        break;

                    case PinRole.Switch_Pin2:
                        if (pin2 == null)
                        {
                            pin2 = pin;
                        }
                        break;

                    case PinRole.Switch_Pin3:
                        if (pin3 == null)
                        {
                            pin3 = pin;
                        }
                        break;
                }
            }

            if (pin0 == null && pins.Length > 0)
            {
                pin0 = pins[0];
            }

            if (pin1 == null && pins.Length > 1)
            {
                pin1 = pins[1];
            }

            if (pin2 == null && pins.Length > 2)
            {
                pin2 = pins[2];
            }

            if (pin3 == null && pins.Length > 3)
            {
                pin3 = pins[3];
            }
        }

        private bool TryResolveSwitchId(out string switchId)
        {
            switchId = null;

            if (!string.IsNullOrEmpty(switchIdOverride))
            {
                switchId = switchIdOverride;
                return true;
            }

            if (switchPart == null || string.IsNullOrEmpty(switchPart.name))
            {
                return false;
            }

            switchId = switchPart.name;
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
