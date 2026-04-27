using UnityEngine;
using System.Collections.Generic;

public class ComponentPanelManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> panels = new List<GameObject>();

    private void Start()
    {
        HideAllPanels();
    }

    public void ShowPanel(int index)
    {
        HideAllPanels();

        if (index >= 0 && index < panels.Count && panels[index] != null)
        {
            panels[index].SetActive(true);
        }
    }

    public void HideAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
}