using UnityEngine;

public class Resistor10KPart : ResistorPart
{
    private void Awake()
    {
        partType = PartType.Resistor10K;
        resistance = 10000f;
    }
}