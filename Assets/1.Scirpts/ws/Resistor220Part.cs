using UnityEngine;

public class Resistor220Part : ResistorPart
{
    protected override void Awake()
    {
        base.Awake();
        partType = PartType.Resistor220;
        resistance = 220f;
    }
}