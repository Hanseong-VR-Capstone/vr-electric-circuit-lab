using UnityEngine;

public class WorkbenchSpawner : MonoBehaviour
{
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private GameObject workbenchPrefab;
    [SerializeField] private Prologue2Controller prologue2Controller; // 추가

    [Tooltip("눈 높이 대비 작업대 높이 비율 (0.65 = 눈높이의 65%)")]
    [SerializeField] private float heightRatio = 0.65f;
    [Tooltip("플레이어 앞 거리")]
    [SerializeField] private float forwardDistance = 0.8f;

    public void SpawnWorkbench()
    {
        Transform eye = cameraRig.centerEyeAnchor;
        float eyeHeight = eye.position.y;
        Vector3 forward = Vector3.ProjectOnPlane(eye.forward, Vector3.up).normalized;
        Vector3 spawnPos = new Vector3(
            eye.position.x + forward.x * forwardDistance,
            eyeHeight * heightRatio,
            eye.position.z + forward.z * forwardDistance
        );
        Quaternion baseRotation = Quaternion.LookRotation(forward);
        Quaternion spawnRotation = baseRotation * Quaternion.Euler(0, 90, 0);

        GameObject workbench = Instantiate(workbenchPrefab, spawnPos, spawnRotation);

        // 스폰된 인스턴스에서 Prologue2Controller 가져오기
        Prologue2Controller p2 = workbench.GetComponentInChildren<Prologue2Controller>(true);
        if (p2 != null)
            p2.StartPrologue2();
    }
}