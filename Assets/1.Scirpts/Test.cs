using UnityEngine;

public class Test : MonoBehaviour
{
    public float force = 0.5f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.right * force, ForceMode.Force);
    }
}
