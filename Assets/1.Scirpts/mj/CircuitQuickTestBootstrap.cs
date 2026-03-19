using UnityEngine;
using UnityEngine.InputSystem;
using VRCircuit.Data;
using VRCircuit.Debugging;
using VRCircuit.Evaluation;
using VRCircuit.Services;

namespace VRCircuit.Debugging
{
    public class CircuitQuickTestBootstrap : MonoBehaviour
    {
        private CircuitContext context;
        private CircuitConnectionService connectionService;
        private CircuitEvaluator evaluator;
        private CircuitDebugValidator validator;

        private void Awake()
        {
            BuildTestContext();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ApplyOpenScenario();
            }

            if (Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ApplyLedOnScenario();
            }

            if (Keyboard.current != null && Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ApplyShortScenario();
            }

            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                Debug.Log(validator.GetFullSummary());
            }

            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            {
                Debug.Log(validator.ValidateBasicState());
            }
        }

        private void BuildTestContext()
        {
            context = new CircuitContext();
            context.ClearAll();

            AddPins();
            AddSockets();
            AddComponents();

            evaluator = new CircuitEvaluator(context);
            connectionService = new CircuitConnectionService(context);
            validator = new CircuitDebugValidator(context, connectionService, evaluator);

            Debug.Log("CircuitQuickTestBootstrap: Test context created.");
        }

        private void AddPins()
        {
            context.AddPin(new CircuitPin("Battery_Pos", PinOwnerType.Battery, "Battery1"));
            context.AddPin(new CircuitPin("Battery_Neg", PinOwnerType.Battery, "Battery1"));

            context.AddPin(new CircuitPin("LED1_Anode", PinOwnerType.Led, "LED1"));
            context.AddPin(new CircuitPin("LED1_Cathode", PinOwnerType.Led, "LED1"));

            context.AddPin(new CircuitPin("Wire1_A", PinOwnerType.Wire, "Wire1"));
            context.AddPin(new CircuitPin("Wire1_B", PinOwnerType.Wire, "Wire1"));

            context.AddPin(new CircuitPin("Wire2_A", PinOwnerType.Wire, "Wire2"));
            context.AddPin(new CircuitPin("Wire2_B", PinOwnerType.Wire, "Wire2"));
        }

        private void AddSockets()
        {
            context.AddSocket(new CircuitSocket("RailPlus_A", SocketType.PowerRailPlus, "RailPlus"));
            context.AddSocket(new CircuitSocket("RailPlus_B", SocketType.PowerRailPlus, "RailPlus"));

            context.AddSocket(new CircuitSocket("RailMinus_A", SocketType.PowerRailMinus, "RailMinus"));
            context.AddSocket(new CircuitSocket("RailMinus_B", SocketType.PowerRailMinus, "RailMinus"));

            context.AddSocket(new CircuitSocket("Row1_A", SocketType.BreadboardRow, "Row1_L"));
            context.AddSocket(new CircuitSocket("Row1_B", SocketType.BreadboardRow, "Row1_L"));

            context.AddSocket(new CircuitSocket("Row2_A", SocketType.BreadboardRow, "Row2_L"));
            context.AddSocket(new CircuitSocket("Row2_B", SocketType.BreadboardRow, "Row2_L"));
        }

        private void AddComponents()
        {
            context.AddWire(new CircuitWire("Wire1", "Wire1_A", "Wire1_B"));
            context.AddWire(new CircuitWire("Wire2", "Wire2_A", "Wire2_B"));

            context.AddBattery(new CircuitBattery("Battery1", "Battery_Pos", "Battery_Neg", 1.5f));
            context.AddLed(new CircuitLed("LED1", "LED1_Anode", "LED1_Cathode"));
        }

        private void DisconnectAllPins()
        {
            DisconnectIfExists("Battery_Pos");
            DisconnectIfExists("Battery_Neg");
            DisconnectIfExists("LED1_Anode");
            DisconnectIfExists("LED1_Cathode");
            DisconnectIfExists("Wire1_A");
            DisconnectIfExists("Wire1_B");
            DisconnectIfExists("Wire2_A");
            DisconnectIfExists("Wire2_B");
        }

        private void DisconnectIfExists(string pinId)
        {
            connectionService.DisconnectPin(pinId);
        }

        private void ApplyOpenScenario()
        {
            DisconnectAllPins();

            Debug.Log("[Scenario] Open");
            Debug.Log(validator.GetFullSummary());
        }

        private void ApplyLedOnScenario()
        {
            DisconnectAllPins();

            connectionService.ConnectPinToSocket("Battery_Pos", "RailPlus_A");
            connectionService.ConnectPinToSocket("Wire1_A", "RailPlus_B");
            connectionService.ConnectPinToSocket("Wire1_B", "Row1_A");
            connectionService.ConnectPinToSocket("LED1_Anode", "Row1_B");

            connectionService.ConnectPinToSocket("LED1_Cathode", "Row2_A");
            connectionService.ConnectPinToSocket("Wire2_A", "Row2_B");
            connectionService.ConnectPinToSocket("Wire2_B", "RailMinus_A");
            connectionService.ConnectPinToSocket("Battery_Neg", "RailMinus_B");

            Debug.Log("[Scenario] LedOn");
            Debug.Log(validator.GetFullSummary());
        }

        private void ApplyShortScenario()
        {
            DisconnectAllPins();

            connectionService.ConnectPinToSocket("Battery_Pos", "RailPlus_A");
            connectionService.ConnectPinToSocket("Wire1_A", "RailPlus_B");
            connectionService.ConnectPinToSocket("Wire1_B", "RailMinus_A");
            connectionService.ConnectPinToSocket("Battery_Neg", "RailMinus_B");

            Debug.Log("[Scenario] Short");
            Debug.Log(validator.GetFullSummary());
        }
    }
}