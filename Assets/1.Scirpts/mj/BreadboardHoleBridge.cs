using UnityEngine;
using VRCircuit.Data;
using VRCircuit.Registration;
using VRCircuit.Runtime;
using VRCircuit.Services;

namespace VRCircuit.Board
{
    [RequireComponent(typeof(HoleTrigger))]
    public class BreadboardHoleBridge : MonoBehaviour
    {
        [SerializeField] private HoleTrigger holeTrigger;
        [SerializeField] private BreadboardSocketBootstrap bootstrap;
        [SerializeField] private bool enableDebugLogs = true;

        private void Awake()
        {
            if (holeTrigger == null)
            {
                holeTrigger = GetComponent<HoleTrigger>();
            }

            if (bootstrap == null)
            {
                bootstrap = GetComponentInParent<BreadboardSocketBootstrap>();
            }
        }

        public void ConnectPartPin(PartPin partPin, string colliderName = null)
        {
            if (partPin == null)
            {
                return;
            }

            CircuitRuntimeRoot runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();

            if (!TryGetDependencies(out CircuitContext context, out CircuitConnectionService connectionService))
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning(
                        $"[BridgeDebug] CONNECT blocked | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | " +
                        $"collider={ValueOrNone(colliderName)} | runtimeRootExists={(runtimeRoot != null)} | " +
                        $"bootstrapExists={(bootstrap != null)} | holeTriggerExists={(holeTrigger != null)}");
                }

                return;
            }

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BridgeDebug] CONNECT event | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | " +
                    $"collider={ValueOrNone(colliderName)} | partPinFound=True | source=HoleTrigger");
            }

            if (!TryResolvePinId(partPin, out string pinId))
            {
                Debug.LogWarning("BreadboardHoleBridge: Failed to resolve circuit pinId.");
                return;
            }

            if (!EnsureCircuitPinExists(context, partPin, pinId))
            {
                return;
            }

            if (!TryResolveSocketId(out string socketId))
            {
                Debug.LogWarning(
                    $"[BridgeDebug] CONNECT invalid socket | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | pinId={pinId}");
                return;
            }

            int pinCountBeforeConnect = context.Pins.Count;
            int socketCount = context.Sockets.Count;
            CircuitSocket socket = context.GetSocketById(socketId);

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BridgeDebug] CONNECT precheck | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | " +
                    $"pinId={pinId} | socketId={socketId} | runtimeRootExists={(runtimeRoot != null)} | " +
                    $"serviceExists={(connectionService != null)} | contextExists={(context != null)} | " +
                    $"pinsBefore={pinCountBeforeConnect} | socketsCount={socketCount} | socketExists={(socket != null)}");
            }

            if (socket == null)
            {
                Debug.LogWarning($"BreadboardHoleBridge: Socket not registered. socketId={socketId}");
                return;
            }

            if (socket.IsOccupied && socket.ConnectedPinId != pinId)
            {
                Debug.LogWarning($"BreadboardHoleBridge: Hole is already occupied. socketId={socketId}");
                return;
            }

            bool connectResult = connectionService.ConnectPinToSocket(pinId, socketId);

            CircuitPin pinAfterConnect = context.GetPinById(pinId);
            CircuitSocket socketAfterConnect = context.GetSocketById(socketId);

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BridgeDebug] CONNECT result | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | " +
                    $"pinId={pinId} | socketId={socketId} | result={connectResult} | " +
                    $"pinsAfter={context.Pins.Count} | pinCurrentSocketId={ValueOrNone(pinAfterConnect?.CurrentSocketId)} | " +
                    $"socketConnectedPinId={ValueOrNone(socketAfterConnect?.ConnectedPinId)} | " +
                    $"currentCircuitState={connectionService.CurrentState}");
            }
        }

        public void DisconnectPartPin(PartPin partPin, string colliderName = null)
        {
            if (partPin == null)
            {
                return;
            }

            if (!TryGetDependencies(out CircuitContext context, out CircuitConnectionService connectionService))
            {
                return;
            }

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BridgeDebug] DISCONNECT event | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | " +
                    $"collider={ValueOrNone(colliderName)} | partPinFound=True | source=HoleTrigger");
            }

            if (!TryResolvePinId(partPin, out string pinId))
            {
                Debug.LogWarning("BreadboardHoleBridge: Failed to resolve circuit pinId.");
                return;
            }

            if (string.IsNullOrEmpty(pinId))
            {
                Debug.LogWarning(
                    $"[BridgeDebug] DISCONNECT invalid pinId | bridge={name} | holeIndex={GetHoleIndexOrInvalid()}");
                return;
            }

            if (!TryResolveSocketId(out string socketId))
            {
                return;
            }

            CircuitSocket socket = context.GetSocketById(socketId);
            if (socket == null)
            {
                Debug.LogWarning($"[BridgeDebug] DISCONNECT socket missing | socketId={socketId}");
                return;
            }

            CircuitPin pin = context.GetPinById(pinId);
            if (pin == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning(
                        $"[BridgeDebug] DISCONNECT pin missing in context | pinId={pinId} | socketId={socketId}");
                }

                return;
            }

            if (socket.ConnectedPinId != pinId)
            {
                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[BridgeDebug] DISCONNECT skipped | socket connected to different pin | " +
                        $"socketId={socketId} | expectedPinId={pinId} | actualConnectedPinId={ValueOrNone(socket.ConnectedPinId)}");
                }

                return;
            }

            if (pin.CurrentSocketId != socketId)
            {
                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[BridgeDebug] DISCONNECT skipped | pin connected to different socket | " +
                        $"pinId={pinId} | expectedSocketId={socketId} | actualSocketId={ValueOrNone(pin.CurrentSocketId)}");
                }

                return;
            }

            bool disconnectResult = connectionService.DisconnectPin(pinId);

            CircuitPin pinAfterDisconnect = context.GetPinById(pinId);
            CircuitSocket socketAfterDisconnect = context.GetSocketById(socketId);

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BridgeDebug] DISCONNECT result | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | " +
                    $"collider={ValueOrNone(colliderName)} | pinId={pinId} | socketId={socketId} | result={disconnectResult} | " +
                    $"pinCurrentSocketId={ValueOrNone(pinAfterDisconnect?.CurrentSocketId)} | " +
                    $"socketConnectedPinId={ValueOrNone(socketAfterDisconnect?.ConnectedPinId)}");
            }
        }

        public void EnsurePartPinConnected(PartPin partPin, string colliderName = null)
        {
            if (partPin == null)
            {
                return;
            }

            if (!TryGetDependencies(out CircuitContext context, out _))
            {
                return;
            }

            if (!TryResolvePinId(partPin, out string pinId) || string.IsNullOrEmpty(pinId))
            {
                return;
            }

            if (!TryResolveSocketId(out string socketId) || string.IsNullOrEmpty(socketId))
            {
                return;
            }

            CircuitPin pin = context.GetPinById(pinId);
            CircuitSocket socket = context.GetSocketById(socketId);

            bool pinConnectedToThisSocket = pin != null && pin.CurrentSocketId == socketId;
            bool socketConnectedToThisPin = socket != null && socket.ConnectedPinId == pinId;

            if (pinConnectedToThisSocket && socketConnectedToThisPin)
            {
                return;
            }

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BridgeDebug] CONNECT repair | bridge={name} | holeIndex={GetHoleIndexOrInvalid()} | " +
                    $"collider={ValueOrNone(colliderName)} | pinId={pinId} | socketId={socketId} | " +
                    $"pinCurrentSocketId={ValueOrNone(pin?.CurrentSocketId)} | " +
                    $"socketConnectedPinId={ValueOrNone(socket?.ConnectedPinId)}");
            }

            ConnectPartPin(partPin, colliderName);
        }

        private bool TryGetDependencies(out CircuitContext context, out CircuitConnectionService connectionService)
        {
            context = null;
            connectionService = null;

            if (bootstrap == null || !bootstrap.IsInitialized || holeTrigger == null)
            {
                return false;
            }

            context = bootstrap.Context;
            connectionService = bootstrap.ConnectionService;

            return context != null && connectionService != null;
        }

        private bool TryResolveSocketId(out string socketId)
        {
            socketId = null;

            if (holeTrigger == null)
            {
                Debug.LogWarning("BreadboardHoleBridge: HoleTrigger reference is missing.");
                return false;
            }

            if (!BreadboardNodeMapper.IsValidHoleIndex(holeTrigger.holeIndex))
            {
                Debug.LogWarning($"BreadboardHoleBridge: Invalid holeIndex. holeIndex={holeTrigger.holeIndex}");
                return false;
            }

            socketId = BreadboardNodeMapper.GetSocketId(holeTrigger.holeIndex);
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

            if (partPin.parentPart != null && partPin.parentPart.partType == PartType.JumperWire)
            {
                if (TryResolveJumperWirePinId(partPin, out pinId))
                {
                    return true;
                }

                Debug.LogWarning("BreadboardHoleBridge: Jumper wire pinId fallback was used. Consider adding CircuitPinIdAdapter.");
            }

            if (partPin.parentPart == null || string.IsNullOrEmpty(partPin.parentPart.name))
            {
                return false;
            }

            pinId = $"{partPin.parentPart.name}_{partPin.pinRole}";
            return true;
        }

        private bool TryResolveJumperWirePinId(PartPin partPin, out string pinId)
        {
            pinId = null;

            if (partPin == null)
            {
                return false;
            }

            CircuitWireRegistrar wireRegistrar = partPin.GetComponentInParent<CircuitWireRegistrar>();
            if (wireRegistrar != null && wireRegistrar.TryGetResolvedWireId(out string wireId))
            {
                pinId = $"{wireId}_{partPin.pinRole}";
                return true;
            }

            string wireRootName = GetJumperWireRootName(partPin);
            if (string.IsNullOrEmpty(wireRootName))
            {
                return false;
            }

            pinId = $"{wireRootName}_{partPin.pinRole}";
            return true;
        }

        private string GetJumperWireRootName(PartPin partPin)
        {
            if (partPin == null)
            {
                return null;
            }

            CircuitPart[] parentParts = partPin.GetComponentsInParent<CircuitPart>();
            if (parentParts == null || parentParts.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < parentParts.Length; i++)
            {
                CircuitPart parentPart = parentParts[i];
                if (parentPart != null &&
                    parentPart.partType == PartType.JumperWire &&
                    !string.IsNullOrEmpty(parentPart.name))
                {
                    return parentPart.name;
                }
            }

            return null;
        }

        private bool EnsureCircuitPinExists(CircuitContext context, PartPin partPin, string pinId)
        {
            if (context == null || partPin == null || string.IsNullOrEmpty(pinId))
            {
                return false;
            }

            if (context.GetPinById(pinId) != null)
            {
                return true;
            }

            if (!TryGetOwnerType(partPin, out PinOwnerType ownerType))
            {
                Debug.LogWarning($"BreadboardHoleBridge: Failed to resolve PinOwnerType. pinId={pinId}");
                return false;
            }

            string ownerId = GetOwnerId(partPin);
            if (string.IsNullOrEmpty(ownerId))
            {
                Debug.LogWarning($"BreadboardHoleBridge: Failed to resolve ownerId. pinId={pinId}");
                return false;
            }

            CircuitPin circuitPin = new CircuitPin(pinId, ownerType, ownerId);
            context.AddPin(circuitPin);

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BridgeDebug] Created CircuitPin | pinId={pinId} | ownerType={ownerType} | ownerId={ownerId} | pinsCount={context.Pins.Count}");
            }

            return true;
        }

        private bool TryGetOwnerType(PartPin partPin, out PinOwnerType ownerType)
        {
            ownerType = PinOwnerType.Wire;

            if (partPin == null || partPin.parentPart == null)
            {
                return false;
            }

            switch (partPin.parentPart.partType)
            {
                case PartType.LED:
                    ownerType = PinOwnerType.Led;
                    return true;

                case PartType.Switch:
                    ownerType = PinOwnerType.Switch;
                    return true;

                case PartType.JumperWire:
                    ownerType = PinOwnerType.Wire;
                    return true;

                case PartType.Resistor220:
                case PartType.Resistor10K:
                    ownerType = PinOwnerType.Wire;
                    return true;

                default:
                    return false;
            }
        }

        private string GetOwnerId(PartPin partPin)
        {
            if (partPin == null || partPin.parentPart == null)
            {
                return null;
            }

            if (partPin.parentPart.partType == PartType.JumperWire)
            {
                CircuitWireRegistrar wireRegistrar = partPin.GetComponentInParent<CircuitWireRegistrar>();
                if (wireRegistrar != null && wireRegistrar.TryGetResolvedWireId(out string wireId))
                {
                    return wireId;
                }

                string wireRootName = GetJumperWireRootName(partPin);
                if (!string.IsNullOrEmpty(wireRootName))
                {
                    return wireRootName;
                }
            }

            return partPin.parentPart.name;
        }

        private PartPin ResolvePartPin(Collider other, out bool foundOnParent)
        {
            foundOnParent = false;

            if (other == null)
            {
                return null;
            }

            PartPin partPin = other.GetComponent<PartPin>();
            if (partPin != null)
            {
                return partPin;
            }

            partPin = other.GetComponentInParent<PartPin>();
            if (partPin != null)
            {
                foundOnParent = true;
            }

            return partPin;
        }

        private int GetHoleIndexOrInvalid()
        {
            return holeTrigger != null ? holeTrigger.holeIndex : -1;
        }

        private string GetColliderName(Collider other)
        {
            return other != null ? other.name : "NULL";
        }

        private string ValueOrNone(string value)
        {
            return string.IsNullOrEmpty(value) ? "none" : value;
        }
    }

    public class CircuitPinIdAdapter : MonoBehaviour
    {
        [SerializeField] private string circuitPinId;

        public string CircuitPinId => circuitPinId;
    }
}
