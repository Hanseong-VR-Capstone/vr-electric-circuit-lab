using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class Prologue2Controller : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayBeforePrologue2 = 2f;
    [SerializeField] private float loadingDuration = 3f;
    [SerializeField] private float ttsDelay = 0f;

    [Header("UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject ariaScreen;

    [Header("TTS")]
    [SerializeField] private VideoPlayer ttsVideoPlayer;

    public void StartPrologue2()
    {
        StartCoroutine(Prologue2Sequence());
        StartCoroutine(TtsPlayCoroutine()); // 완전 독립 실행
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
}