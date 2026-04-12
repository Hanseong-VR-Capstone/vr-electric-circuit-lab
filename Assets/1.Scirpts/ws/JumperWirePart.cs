using UnityEngine;

public class JumperWirePart : CircuitPart
{
    [Header("Jumper Wire")]
    [SerializeField] private JumperWirePart otherEnd;

    protected override void Awake()
    {
        base.Awake();
        partType = PartType.JumperWire;
    }

    // 고정될 때 enabledValue = flase, 고정 풀릴 때 enabledValue = true
    protected override void SetDistanceGrabEnabled(bool enabledValue)
    {
        string message = $"{partType} : Distance Grab 활성화 여부 변경 - {enabledValue}";
        if (otherEnd != null)
        {
            if (otherEnd.HasAnyPinAttached())
            {
                message += " -> false (다른 쪽 핀이 연결되어 있어서 false로 변경)";
                enabledValue = false;
            }
        }

        if (distanceGrabInteractable != null) 
            distanceGrabInteractable.enabled = enabledValue;

        if (distanceHandGrabInteractable != null)
            distanceHandGrabInteractable.enabled = enabledValue;
        
        Debug.Log(message);
    }
}