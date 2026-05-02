using UnityEngine;

public class LedPart : CircuitPart
{
    public GameObject lightEffect;
    protected override void Awake()
    {
        base.Awake();
        partType = PartType.LED;
        manageLight(false);
    }

    // 회로 상태에 따라 LED 켜고 끄는 함수
    public void manageLight(bool isOn)
    {
        if (isOn)
        {
            lightEffect.SetActive(true);
        }
        else
        {
            lightEffect.SetActive(false);
        }
    }
}