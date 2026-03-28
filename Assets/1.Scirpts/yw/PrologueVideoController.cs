using Oculus.Interaction.Locomotion;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class PrologueVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Transform mainMapSpawnPoint;
    [SerializeField] private LocomotionEventsConnection locomotionEventsConnection;
    [SerializeField] private Material blackSkybox;
    [SerializeField] private Material spaceSkybox;
    [SerializeField] private GameObject livingRoom;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject cinematicScreen;

    private WorkbenchSpawner spawner;

    private void Awake()
    {
        spawner = GetComponent<WorkbenchSpawner>();
    }

    void Start()
    {
        livingRoom.SetActive(false);
        RenderSettings.skybox = blackSkybox;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        livingRoom.SetActive(true);
        spawnPoint.SetActive(false);
        cinematicScreen.SetActive(false);

        LocomotionEvent teleportEvent = new LocomotionEvent(
            0,
            new Pose(mainMapSpawnPoint.position, mainMapSpawnPoint.rotation),
            LocomotionEvent.TranslationType.Absolute,
            LocomotionEvent.RotationType.Absolute
        );
        locomotionEventsConnection.HandleLocomotionEvent(teleportEvent);

        RenderSettings.skybox = spaceSkybox;
        DynamicGI.UpdateEnvironment();
        StartCoroutine(SpawnAfterTeleport());
    }

    private IEnumerator SpawnAfterTeleport()
    {
        yield return null; // 1프레임 대기
        spawner.SpawnWorkbench();
    }
}