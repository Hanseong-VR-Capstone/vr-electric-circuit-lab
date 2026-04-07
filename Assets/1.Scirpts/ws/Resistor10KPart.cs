using UnityEngine;

public class Resistor10KPart : ResistorPart
{
    protected override void Awake()
    {
        base.Awake();
        partType = PartType.Resistor10K;
        resistance = 10000f;
    }
}