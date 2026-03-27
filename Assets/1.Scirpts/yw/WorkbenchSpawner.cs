using UnityEngine;

public class WorkbenchSpawner : MonoBehaviour
{
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private GameObject workbenchPrefab;

    [Tooltip("눈 높이 대비 작업대 높이 비율 (0.65 = 눈높이의 65%)")]
    [SerializeField] private float heightRatio = 0.65f;

    [Tooltip("플레이어 앞 거리")]
    [SerializeField] private float forwardDistance = 0.8f;

    public void SpawnWorkbench()
    {
        Transform eye = cameraRig.centerEyeAnchor;
        float eyeHeight = eye.position.y;

        // 작업대 위치: 플레이어 앞, 눈높이 비율만큼 아래
        Vector3 forward = Vector3.ProjectOnPlane(eye.forward, Vector3.up).normalized;
        Vector3 spawnPos = new Vector3(
            eye.position.x + forward.x * forwardDistance,
            eyeHeight * heightRatio,
            eye.position.z + forward.z * forwardDistance
        );

        Instantiate(workbenchPrefab, spawnPos, Quaternion.LookRotation(forward));
    }
}