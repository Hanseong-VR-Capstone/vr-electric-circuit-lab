using UnityEngine;

public class TestJumperWireLight : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private JumperWireLight jumperWireLight;

    private void Update()
    {
        // O 키 → 왼쪽에서 전류 시작 (Wire_A)
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("LEFT → RIGHT 전류 시작");
            jumperWireLight.LightOn(PinRole.Wire_A);
        }

        // P 키 → 오른쪽에서 전류 시작 (Wire_B)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("RIGHT → LEFT 전류 시작");
            jumperWireLight.LightOn(PinRole.Wire_B);
        }

        // L 키 → 전류 끄기
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("전류 OFF");
            jumperWireLight.LightOff();
        }
    }
}