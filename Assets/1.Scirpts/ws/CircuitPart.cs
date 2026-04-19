using NUnit.Framework;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class CircuitPart : MonoBehaviour
{
    public PartType partType;
    [SerializeField] protected PartPin[] pins;

    [SerializeField] protected bool isLocked;
    [SerializeField] protected bool isGrabbed;
    [SerializeField] protected Vector3 lockedPosition;
    [SerializeField] protected Quaternion lockedRotation;

    [Header("Distance Grab Components")]
    [SerializeField] protected DistanceGrabInteractable distanceGrabInteractable;
    [SerializeField] protected DistanceHandGrabInteractable distanceHandGrabInteractable;

    [Header("Grab Latch")]
    [SerializeField] protected bool isPoseLatchedWhileGrabbed;
    [SerializeField] protected Vector3 latchedPosition;
    [SerializeField] protected Quaternion latchedRotation;

    protected virtual void Awake()
    {
        pins = GetComponentsInChildren<PartPin>();

        isLocked = false;
        isGrabbed = false;
        isPoseLatchedWhileGrabbed = false;
    }

    protected virtual void LateUpdate()
    {
        // 잡고있는 동안 모든 핀이 연결되면 해당 위치 고정
        if (isGrabbed && isPoseLatchedWhileGrabbed)
        {
            transform.position = latchedPosition;
            transform.rotation = latchedRotation;
            return;
        }

        // 손을 놓고 난 뒤에는 완전 고정 상태 유지
        if (isLocked && !isGrabbed)
        {
            transform.position = lockedPosition;
            transform.rotation = lockedRotation;
        }
    }

    public void OnGrabDefault()
    {
        isGrabbed = true;
        isLocked = false;
        isPoseLatchedWhileGrabbed = false;
        Debug.Log(partType + " 잡았다");
    }

    public void OnReleaseCheckLock()
    {
        isGrabbed = false;
        if (HasAnyPinAttached())
        {
            if (isPoseLatchedWhileGrabbed)
            {
                lockedPosition = latchedPosition;
                lockedRotation = latchedRotation;
            }
            else
            {
                lockedPosition = transform.position;
                lockedRotation = transform.rotation;
            }

            isLocked = true;
            isPoseLatchedWhileGrabbed = false;
            SetDistanceGrabEnabled(false);
            Debug.Log(partType + " 놓았는데, 핀이 1개 이상 꽂혀있어서 고정됨");
        }
        else
        {
            isLocked = false;
            isPoseLatchedWhileGrabbed = false;
            SetDistanceGrabEnabled(true);
            Debug.Log(partType + " 놓았는데, 핀이 하나도 안꽂혀있어서 고정 안됨");
        }
    }

    // 잡고있는 동안 모든 핀이 꽂히면 현재 위치로 고정
    public void TryLatchPoseWhileGrabbed()
    {
        if (!isGrabbed) return;
        if (isPoseLatchedWhileGrabbed) return;
        if (!AreAllPinsAttached()) return;

        latchedPosition = transform.position;
        latchedRotation = transform.rotation;
        isPoseLatchedWhileGrabbed = true;
        Debug.Log(partType + " 잡고있는 동안 모든 핀이 꽂혀서 위치 고정됨, 위치를 옮기고 싶으면 손을 놨다가 다시 잡아야 함");
    }

    protected bool AreAllPinsAttached()
    {
        if (pins == null || pins.Length == 0)
            return false;

        foreach (PartPin pin in pins)
        {
            if (pin.currentHole == null)
                return false;
        }
        return true;
    }

    protected bool HasAnyPinAttached()
    {
        foreach (PartPin pin in pins)
        {
            if (pin.currentHole != null)
                return true;
        }
        return false;
    }

    protected virtual void SetDistanceGrabEnabled(bool enabledValue)
    {
        string message = $"{partType} : Distance Grab 활성화 여부 변경 - {enabledValue}";
        if (distanceGrabInteractable != null)
            distanceGrabInteractable.enabled = enabledValue;

        if (distanceHandGrabInteractable != null)
            distanceHandGrabInteractable.enabled = enabledValue;
        
        Debug.Log(message);
    }
}