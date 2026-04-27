using System.Collections.Generic;
using UnityEngine;

public class TestHoleLight : MonoBehaviour
{
    [SerializeField] private List<HoleTrigger> holeTriggers;
    [SerializeField] private int index = 0;

    private void Update()
    {
        // + 입력 → 불 켜고 index 증가
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (index < holeTriggers.Count)
            {
                holeTriggers[index].HoleLightOn();
                index++;
            }
        }

        // - 입력 → index 감소 후 불 끄기
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (index > 0)
            {
                index--;
                holeTriggers[index].HoleLightOff();
            }
        }
    }
}