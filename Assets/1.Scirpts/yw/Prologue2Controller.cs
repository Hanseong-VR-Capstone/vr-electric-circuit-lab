using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class Prologue2Controller : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayBeforePrologue2 = 2f;
    [SerializeField] private float loadingDuration = 3f;
    [SerializeField] private float ttsDelay = 0f;
    [SerializeField] private float screen1Delay = 0f;
    [SerializeField] private float screen2Delay = 0f;

    [Header("UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject ariaScreen;
    [SerializeField] private GameObject screen1;
    [SerializeField] private GameObject screen2;

    [Header("TTS")]
    [SerializeField] private VideoPlayer ttsVideoPlayer;

    public void StartPrologue2()
    {
        StartCoroutine(Prologue2Sequence());
        StartCoroutine(TtsPlayCoroutine());
        StartCoroutine(ScreenActivateCoroutine(screen1, screen1Delay));
        StartCoroutine(ScreenActivateCoroutine(screen2, screen2Delay));
    }

    private IEnumerator Prologue2Sequence()
    {
        yield return new WaitForSeconds(delayBeforePrologue2);

        loadingScreen.SetActive(true);

        yield return new WaitForSeconds(loadingDuration);

        loadingScreen.SetActive(false);
        ariaScreen.SetActive(true);
    }

    private IEnumerator TtsPlayCoroutine()
    {
        yield return new WaitForSeconds(ttsDelay);
        ttsVideoPlayer.Play();
    }

    private IEnumerator ScreenActivateCoroutine(GameObject screen, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (screen != null)
            screen.SetActive(true);
    }
}