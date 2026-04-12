using UnityEngine;
using VRCircuit.Data;
using VRCircuit.Services;

namespace VRCircuit.Board
{
    [RequireComponent(typeof(HoleTrigger))]
    public class BreadboardHoleBridge : MonoBehaviour
    {
        [SerializeField] private HoleTrigger holeTrigger;
        [SerializeField] private BreadboardSocketBootstrap bootstrap;

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

        private void OnTriggerEnter(Collider other)
        {
            if (!TryGetDependencies(out CircuitContext context, out CircuitConnectionService connectionService))
            {
                return;
            }

            PartPin partPin = other.GetComponent<PartPin>();
            if (partPin == null)
            {
                return;
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
                return;
            }

            CircuitSocket socket = context.GetSocketById(socketId);
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

            Debug.Log($"[Bridge] CONNECT try | pinId={pinId} | socketId={socketId}");
            connectionService.ConnectPinToSocket(pinId, socketId);
            Debug.Log("[Bridge] CONNECT called");
        }

        private void OnTriggerExit(Collider other)
        {
            if (!TryGetDependencies(out CircuitContext context, out CircuitConnectionService connectionService))
            {
                return;
            }

            PartPin partPin = other.GetComponent<PartPin>();
            if (partPin == null)
            {
                return;
            }

            if (!TryResolvePinId(partPin, out string pinId))
            {
                Debug.LogWarning("BreadboardHoleBridge: Failed to resolve circuit pinId.");
                return;
            }

            if (!TryResolveSocketId(out string socketId))
            {
                return;
            }

            CircuitSocket socket = context.GetSocketById(socketId);
            if (socket == null)
            {
                return;
            }

            CircuitPin pin = context.GetPinById(pinId);
            if (pin == null)
            {
                return;
            }

            if (socket.ConnectedPinId != pinId)
            {
                return;
            }

            if (pin.CurrentSocketId != socketId)
            {
                return;
            }

            Debug.Log($"[Bridge] DISCONNECT try | pinId={pinId} | socketId={socketId}");
            connectionService.DisconnectPin(pinId);
            Debug.Log("[Bridge] DISCONNECT called");
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

            if (partPin.parentPart == null || string.IsNullOrEmpty(partPin.parentPart.name))
            {
                return false;
            }

            pinId = $"{partPin.parentPart.name}_{partPin.pinRole}";
            return true;
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
            Debug.Log($"[Service] Created CircuitPin | pinId={pinId}");
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
                    // Current PinOwnerType enum has no resistor category yet.
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

            return partPin.parentPart.name;
        }
    }

    public class CircuitPinIdAdapter : MonoBehaviour
    {
        [SerializeField] private string circuitPinId;

        public string CircuitPinId => circuitPinId;
    }
}
