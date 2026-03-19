using Oculus.Interaction.Locomotion;
using UnityEngine;
using UnityEngine.Video;

public class PrologueVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Transform mainMapSpawnPoint;
    [SerializeField] private LocomotionEventsConnection locomotionEventsConnection;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        LocomotionEvent teleportEvent = new LocomotionEvent(
            0,
            new Pose(mainMapSpawnPoint.position, mainMapSpawnPoint.rotation),
            LocomotionEvent.TranslationType.Absolute,
            LocomotionEvent.RotationType.Absolute
        );
        locomotionEventsConnection.HandleLocomotionEvent(teleportEvent);
    }
}