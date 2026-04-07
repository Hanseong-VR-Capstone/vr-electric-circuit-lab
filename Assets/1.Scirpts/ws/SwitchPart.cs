using UnityEngine;

public class SwitchPart : CircuitPart
{
    public bool isOn;

    protected override void Awake()
    {
        base.Awake();
        partType = PartType.Switch;
        isOn = false;
    }

        public void TurnOn()
    {
        isOn = true;
        Debug.Log($"{name} : ON");
    }

    public void TurnOff()
    {
        isOn = false;
        Debug.Log($"{name} : OFF");
    }
}