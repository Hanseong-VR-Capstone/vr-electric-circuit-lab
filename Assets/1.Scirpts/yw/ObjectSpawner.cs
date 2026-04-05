using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    public Transform spawnPoint;
}

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnEntry> spawnEntries = new List<SpawnEntry>();

    // 버튼 OnClick()에서 호출 ? index는 버튼마다 다르게 설정
    public void SpawnObject(int index)
    {
        if (index < 0 || index >= spawnEntries.Count)
        {
            Debug.LogWarning($"SpawnEntry index {index} out of range.");
            return;
        }

        SpawnEntry entry = spawnEntries[index];

        if (entry.prefab == null || entry.spawnPoint == null)
        {
            Debug.LogWarning($"SpawnEntry[{index}]: prefab 또는 spawnPoint가 비어있습니다.");
            return;
        }

        Instantiate(entry.prefab, entry.spawnPoint.position, entry.spawnPoint.rotation);
    }
}