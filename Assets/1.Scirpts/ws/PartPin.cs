using UnityEngine;

public enum PinRole
{
    None,

    LED_Plus,
    LED_Minus,

    Switch_Pin0,
    Switch_Pin1,
    Switch_Pin2,
    Switch_Pin3,

    // left = A, right = B
    Resistor_A,
    Resistor_B,

    Wire_A,
    Wire_B
}

public class PartPin : MonoBehaviour
{
    public CircuitPart parentPart;
    public PinRole pinRole;
    public HoleTrigger currentHole;

    private void Awake()
    {
        parentPart = GetComponentInParent<CircuitPart>();
    }

    public void SetHole(HoleTrigger hole)
    {
        currentHole = hole;
        
        if (parentPart != null)
        {
            parentPart.TryLatchPoseWhileGrabbed();
            Debug.Log(parentPart.partType + "의 " + pinRole + " 핀이 구멍에 꽂혀서 위치 고정 시도");
        }
    }

    public void ClearHole(HoleTrigger hole)
    {
        if (currentHole == hole)
            currentHole = null;
    }
}