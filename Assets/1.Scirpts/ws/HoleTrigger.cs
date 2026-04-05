using UnityEngine;

public class HoleTrigger : MonoBehaviour
{
    public int holeIndex;
    public PartPin currentPin;

    private void OnTriggerEnter(Collider other)
    {
        PartPin pin = other.GetComponent<PartPin>();
        if (pin == null) return;

        if (currentPin != null && currentPin != pin)
        {
            Debug.Log($"Hole {holeIndex} already occupied");
            return;
        }

        CircuitPart part = pin.parentPart;
        if (part == null) {
            Debug.Log($"Pin {pin.pinRole}은 부모 파트가 없습니다.");
            return;
        }

        currentPin = pin;
        pin.SetHole(this);

        string message = $"Hole {holeIndex} <- {part.partType} / {pin.pinRole}";

        switch (part.partType)
        {
            case PartType.LED:
                message += $" / {(pin.pinRole == PinRole.LED_Plus ? "LED +" : "LED -")}";
                break;

            case PartType.Switch:
                SwitchPart switchPart = part as SwitchPart;
                if (switchPart != null)
                {
                    message += $" / SwitchState: {(switchPart.isOn ? "ON" : "OFF")} / Switch";
                }
                break;

            case PartType.Resistor220:
            case PartType.Resistor10K:
                ResistorPart resistorPart = part as ResistorPart;
                if (resistorPart != null)
                {
                    message += $" / {resistorPart.resistance} ohm / Resistor";
                }
                break;

            case PartType.JumperWire:
                message += $" / JumperWire";
                break;
            
            default:
                message += $" / Unknown Part";
                break;
        }
        
        Debug.Log(message);
    }

    private void OnTriggerExit(Collider other)
    {
        PartPin pin = other.GetComponent<PartPin>();
        if (pin == null) return;

        if (currentPin == pin)
        {
            Debug.Log($"Hole {holeIndex} 에서 {pin.parentPart.partType} / {pin.pinRole} 제거");
            currentPin = null;
            pin.ClearHole(this);
        }
    }
}