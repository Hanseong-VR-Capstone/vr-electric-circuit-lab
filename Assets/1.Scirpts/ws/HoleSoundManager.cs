using UnityEngine;

public class HoleSoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip connectSound;
    public AudioClip disconnectSound;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource 컴포넌트가 없습니다");
            }
        }
    }

    public void PlayConnectSound()
    {
        if (connectSound != null)
        {
            audioSource.PlayOneShot(connectSound);
        }
    }

    public void PlayDisconnectSound()
    {
        if (disconnectSound != null)
        {
            audioSource.PlayOneShot(disconnectSound);
        }
    }
}
