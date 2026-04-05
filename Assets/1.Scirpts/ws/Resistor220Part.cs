using UnityEngine;

public class Resistor220Part : ResistorPart
{
    private void Awake()
    {
        partType = PartType.Resistor220;
        resistance = 220f;
    }
}