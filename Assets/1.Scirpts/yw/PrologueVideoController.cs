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
    [SerializeField] private GameObject skipButton; // 추가

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
        HandlePrologueEnd();
    }

    public void OnSkipButtonPressed() // Skip 버튼 OnClick에 연결
    {
        videoPlayer.loopPointReached -= OnVideoEnd; // 중복 호출 방지
        HandlePrologueEnd();
    }

    private void HandlePrologueEnd()
    {
        livingRoom.SetActive(true);
        spawnPoint.SetActive(false);
        cinematicScreen.SetActive(false);
        skipButton.SetActive(false); // 추가

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
        yield return null;
        spawner.SpawnWorkbench();
    }
}