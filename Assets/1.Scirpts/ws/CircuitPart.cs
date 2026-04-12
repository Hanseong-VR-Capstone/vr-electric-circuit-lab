using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class CircuitPart : MonoBehaviour
{
    public PartType partType;
    [SerializeField] protected Rigidbody rb;
    protected RigidbodyConstraints defaultConstraints;
    [SerializeField] protected PartPin[] pins;

    [SerializeField] protected bool isLocked;
    [SerializeField] protected bool isGrabbed;
    [SerializeField] protected Vector3 lockedPosition;
    [SerializeField] protected Quaternion lockedRotation;

    [Header("Distance Grab Components")]
    [SerializeField] protected DistanceGrabInteractable distanceGrabInteractable;
    [SerializeField] protected DistanceHandGrabInteractable distanceHandGrabInteractable;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        defaultConstraints = rb.constraints;
        pins = GetComponentsInChildren<PartPin>();

        isLocked = false;
        isGrabbed = false;
    }

    protected virtual void LateUpdate()
    {
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
        rb.constraints = defaultConstraints;
        Debug.Log(partType + " 잡았다");
    }

    public void OnReleaseCheckLock()
    {
        isGrabbed = false;
        if (HasAnyPinAttached())
        {
            SnapToAttachedPin();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            lockedPosition = transform.position;
            lockedRotation = transform.rotation;
            isLocked = true;
            SetDistanceGrabEnabled(false);
            Debug.Log(partType + " 놓았는데, 핀이 1개 이상 꽂혀있어서 고정됨");
        }
        else
        {
            rb.constraints = defaultConstraints;
            isLocked = false;
            SetDistanceGrabEnabled(true);
            Debug.Log(partType + " 놓았는데, 핀이 하나도 안꽂혀있어서 고정 안됨");
        }
    }

    protected void SnapToAttachedPin()
    {
        // 꽂혀있는 핀 중 첫 번째 핀을 기준으로 위치 보정
        PartPin attachedPin = GetFirstAttachedPin();
        if (attachedPin == null || attachedPin.currentHole == null)
            return;

        Vector3 offset = attachedPin.currentHole.transform.position - attachedPin.transform.position;
        offset.y = 0f; // Y축은 유지

        transform.position += offset;
    }

    protected PartPin GetFirstAttachedPin()
    {
        foreach (PartPin pin in pins)
        {
            if (pin.currentHole != null)
                return pin;
        }
        return null;
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