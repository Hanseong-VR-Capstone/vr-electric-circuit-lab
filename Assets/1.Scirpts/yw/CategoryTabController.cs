using System.Collections.Generic;
using UnityEngine;

public class CategoryTabController : MonoBehaviour
{
    [System.Serializable]
    public class CategoryTab
    {
        public GameObject tabObject;
    }

    [Header("Tabs")]
    public List<CategoryTab> tabs = new List<CategoryTab>();

    private GameObject currentTab;

    void Start()
    {
        // 모두 비활성화 후 첫 번째(Wire) 활성화
        foreach (var tab in tabs)
            tab.tabObject.SetActive(false);

        if (tabs.Count > 0)
        {
            currentTab = tabs[0].tabObject;
            currentTab.SetActive(true);
        }
    }

    public void OnCategoryChanged(int index)
    {
        if (index < 0 || index >= tabs.Count) return;

        if (currentTab != null)
            currentTab.SetActive(false);

        currentTab = tabs[index].tabObject;
        currentTab.SetActive(true);
    }
}