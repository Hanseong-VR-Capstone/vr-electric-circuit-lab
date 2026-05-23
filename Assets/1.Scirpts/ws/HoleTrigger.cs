using UnityEngine;
using VRCircuit.Board;

public class HoleTrigger : MonoBehaviour
{
    public int holeIndex;
    public PartPin currentPin;
    public HoleSoundManager audioManager;

    [SerializeField] private HoleGroup holeGroup;
    [SerializeField] private BreadboardHoleBridge breadboardHoleBridge;

    private void Awake()
    {
        holeGroup = GetComponentInParent<HoleGroup>();

        if (breadboardHoleBridge == null)
        {
            breadboardHoleBridge = GetComponent<BreadboardHoleBridge>();
        }

        if (audioManager == null)
        {
            audioManager = GetComponentInParent<HoleSoundManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("HoleTrigger: HoleSoundManager was not found.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PartPin pin = ResolvePartPin(other);
        if (pin == null)
        {
            return;
        }

        if (currentPin != null && currentPin != pin)
        {
            Debug.Log($"Hole {holeIndex} already occupied");
            return;
        }

        CircuitPart part = pin.parentPart;
        if (part == null)
        {
            Debug.Log($"HoleTrigger: Pin {pin.pinRole} has no parent CircuitPart.");
            return;
        }

        currentPin = pin;
        pin.SetHole(this);
        breadboardHoleBridge?.ConnectPartPin(pin, GetColliderName(other));

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
                message += " / JumperWire";
                break;

            default:
                message += " / Unknown Part";
                break;
        }

        audioManager?.PlayConnectSound();
        Debug.Log(message);
    }

    private void OnTriggerExit(Collider other)
    {
        PartPin pin = ResolvePartPin(other);
        if (pin == null)
        {
            return;
        }

        if (currentPin == pin)
        {
            Debug.Log($"Hole {holeIndex} removed {pin.parentPart.partType} / {pin.pinRole}");
            audioManager?.PlayDisconnectSound();
            breadboardHoleBridge?.DisconnectPartPin(pin, GetColliderName(other));
            currentPin = null;
            pin.ClearHole(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PartPin pin = ResolvePartPin(other);
        if (pin == null || currentPin != pin)
        {
            return;
        }

        breadboardHoleBridge?.EnsurePartPinConnected(pin, GetColliderName(other));
    }

    public void HoleLightOn()
    {
        holeGroup?.UpdateEffect();
    }

    public void HoleLightOff()
    {
        holeGroup?.UpdateEffect(false);
    }

    private PartPin ResolvePartPin(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        PartPin pin = other.GetComponent<PartPin>();
        if (pin != null)
        {
            return pin;
        }

        return other.GetComponentInParent<PartPin>();
    }

    private string GetColliderName(Collider other)
    {
        return other != null ? other.name : "NULL";
    }
}
