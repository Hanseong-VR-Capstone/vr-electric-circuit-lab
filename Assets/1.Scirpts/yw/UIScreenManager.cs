using UnityEngine;

public class UIScreenManager : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject prologueScreen;
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject componentScreen;
    [SerializeField] private GameObject voltageScreen;

    public void ShowMain()
    {
        SetAll(false);
        mainScreen.SetActive(true);
    }

    public void ShowComponent()
    {
        SetAll(false);
        componentScreen.SetActive(true);
    }

    public void ShowVoltage()
    {
        SetAll(false);
        voltageScreen.SetActive(true);
    }

    public void ShowPrologue()
    {
        SetAll(false);
        prologueScreen.SetActive(true);
    }

    private void SetAll(bool state)
    {
        prologueScreen.SetActive(state);
        mainScreen.SetActive(state);
        componentScreen.SetActive(state);
        voltageScreen.SetActive(state);
    }
}