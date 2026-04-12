using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class CircuitPart : MonoBehaviour
{
    [SerializeField] protected Transform VRTableTransform;
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
        VRTableTransform = GameObject.FindGameObjectWithTag("VR table").transform;

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
            SnapToAttachedPins();
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

    protected void SnapToAttachedPins()
    {
        if (VRTableTransform == null) 
        {
            Debug.LogWarning("VRTableTransform이 할당되지 않았습니다. 핀 스냅이 작동하지 않습니다.");
            return;
        }

        int attachedCount = 0;
        Vector3 totalOffsetLocal = Vector3.zero;

        foreach (PartPin pin in pins)
        {
            if (pin.currentHole == null) continue;

            Vector3 pinLocal = VRTableTransform.InverseTransformPoint(pin.transform.position);
            Vector3 holeLocal = VRTableTransform.InverseTransformPoint(pin.currentHole.transform.position);

            Vector3 offsetLocal = holeLocal - pinLocal;
            totalOffsetLocal += offsetLocal;
            attachedCount++;
        }

        if (attachedCount == 0) return;

        Vector3 averageOffsetLocal = totalOffsetLocal / attachedCount;

        // 보드 두께 방향 이동은 막고, 보드 평면 안에서만 이동
        averageOffsetLocal.y = 0f;

        Vector3 worldOffset =
            VRTableTransform.TransformVector(averageOffsetLocal);

        transform.position += worldOffset;
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