using UnityEngine;

public class HoleGroup : MonoBehaviour
{
    [Header("전류 이펙트")]
    public GameObject currentEffect;

    public void UpdateEffect(bool isActive = true)
    {
        if (currentEffect != null)
        {
            currentEffect.SetActive(isActive);
        }
    }
}