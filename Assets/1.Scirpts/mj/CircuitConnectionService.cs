using UnityEngine;
using VRCircuit.Data;
using VRCircuit.Evaluation;

namespace VRCircuit.Services
{
    public class CircuitConnectionService
    {
        private readonly CircuitContext context;
        private readonly CircuitEvaluator evaluator;
        private CircuitState currentState = CircuitState.Open;

        public CircuitState CurrentState => currentState;

        public CircuitConnectionService(CircuitContext context)
        {
            this.context = context;
            evaluator = new CircuitEvaluator(context);
        }

        public bool ConnectPinToSocket(string pinId, string socketId)
        {
            Debug.Log($"[Service] CONNECT request | pinId={pinId} | socketId={socketId}");

            if (context == null)
            {
                Debug.LogWarning("CircuitConnectionService: CircuitContext is null.");
                return false;
            }

            CircuitPin pin = context.GetPinById(pinId);
            Debug.Log($"[Service] pin lookup: {(pin != null ? "OK" : "NULL")}");
            if (pin == null)
            {
                Debug.LogWarning($"CircuitConnectionService: Pin not found. pinId={pinId}");
                return false;
            }

            CircuitSocket socket = context.GetSocketById(socketId);
            Debug.Log($"[Service] socket lookup: {(socket != null ? "OK" : "NULL")}");
            if (socket == null)
            {
                Debug.LogWarning($"CircuitConnectionService: Socket not found. socketId={socketId}");
                return false;
            }

            if (socket.IsOccupied && socket.ConnectedPinId != pin.PinId)
            {
                Debug.LogWarning($"CircuitConnectionService: Socket is already occupied. socketId={socketId}");
                return false;
            }

            if (pin.CurrentSocketId == socket.SocketId)
            {
                return true;
            }

            if (pin.IsConnected)
            {
                bool disconnected = DisconnectPinInternal(pin, false);
                if (!disconnected)
                {
                    Debug.LogWarning($"CircuitConnectionService: Failed to disconnect existing socket connection. pinId={pinId}");
                    return false;
                }
            }

            pin.SetCurrentSocketId(socket.SocketId);
            socket.SetConnectedPinId(pin.PinId);

            Evaluate();
            Debug.Log($"[Service] CONNECT success | pinId={pinId} -> socketId={socketId}");
            Debug.Log($"[Service] CurrentState={CurrentState}");
            return true;
        }

        public bool DisconnectPin(string pinId)
        {
            Debug.Log($"[Service] DISCONNECT request | pinId={pinId}");

            if (context == null)
            {
                Debug.LogWarning("CircuitConnectionService: CircuitContext is null.");
                return false;
            }

            CircuitPin pin = context.GetPinById(pinId);
            Debug.Log($"[Service] pin lookup: {(pin != null ? "OK" : "NULL")}");
            if (pin == null)
            {
                Debug.LogWarning($"CircuitConnectionService: Pin not found. pinId={pinId}");
                return false;
            }

            if (!pin.IsConnected)
            {
                return false;
            }

            return DisconnectPinInternal(pin, true);
        }

        public bool SwitchStateChanged(string switchId, bool isOn)
        {
            if (context == null)
            {
                Debug.LogWarning("CircuitConnectionService: CircuitContext is null.");
                return false;
            }

            CircuitSwitch circuitSwitch = context.GetSwitchById(switchId);
            if (circuitSwitch == null)
            {
                Debug.LogWarning($"CircuitConnectionService: Switch not found. switchId={switchId}");
                return false;
            }

            if (circuitSwitch.IsOn == isOn)
            {
                return true;
            }

            circuitSwitch.SetIsOn(isOn);
            Evaluate();
            return true;
        }

        private bool DisconnectPinInternal(CircuitPin pin, bool shouldEvaluate)
        {
            if (pin == null || !pin.IsConnected)
            {
                return false;
            }

            CircuitSocket socket = context.GetSocketById(pin.CurrentSocketId);
            if (socket != null && socket.ConnectedPinId == pin.PinId)
            {
                socket.SetConnectedPinId(null);
            }

            pin.SetCurrentSocketId(null);

            if (shouldEvaluate)
            {
                Debug.Log($"[Service] DISCONNECT success | pinId={pin.PinId}");
                Evaluate();
                Debug.Log($"[Service] CurrentState={CurrentState}");
            }

            return true;
        }

        private void Evaluate()
        {
            currentState = evaluator.Evaluate();
        }
    }
}
