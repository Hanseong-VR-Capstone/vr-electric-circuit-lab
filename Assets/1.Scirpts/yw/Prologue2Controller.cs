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
    [SerializeField] private GameObject prologueRoot;  // Interface/Prologue
    [SerializeField] private GameObject mainRoot;      // Interface/Main

    [Header("TTS")]
    [SerializeField] private VideoPlayer ttsVideoPlayer;

    public void StartPrologue2()
    {
        StartCoroutine(Prologue2Sequence());
        StartCoroutine(TtsPlayCoroutine());
    }

    // 스킵 버튼 OnClick에 연결
    public void SkipPrologue()
    {
        StopAllCoroutines();
        if (ttsVideoPlayer != null) ttsVideoPlayer.Stop();
        GoToMain();
    }

    private IEnumerator Prologue2Sequence()
    {
        yield return new WaitForSeconds(delayBeforePrologue2);
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(loadingDuration);
        loadingScreen.SetActive(false);
        ariaScreen.SetActive(true);

        // 프롤로그 정상 종료 후 메인으로
        // 필요하면 여기서 추가 대기 후 GoToMain() 호출
    }

    private IEnumerator TtsPlayCoroutine()
    {
        yield return new WaitForSeconds(ttsDelay);
        ttsVideoPlayer.Play();
    }

    private void GoToMain()
    {
        prologueRoot.SetActive(false);
        mainRoot.SetActive(true);
    }
}