using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    public Transform spawnPoint;
    public List<GameObject> variants;
}

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnEntry> spawnEntries = new List<SpawnEntry>();

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

        Collider[] colliders = Physics.OverlapSphere(entry.spawnPoint.position, 0.2f);
        if (colliders.Length > 0)
        {
            Debug.Log($"SpawnEntry[{index}]: 스폰 포인트에 이미 오브젝트가 있습니다.");
            return;
        }

        GameObject selected = entry.prefab;
        if (entry.variants != null && entry.variants.Count > 0)
        {
            selected = entry.variants[Random.Range(0, entry.variants.Count)];
        }

        Instantiate(selected, entry.spawnPoint.position, entry.spawnPoint.rotation);
    }

    public void DeleteAllVRObjects()
    {
        GameObject[] vrObjects = GameObject.FindGameObjectsWithTag("VR object");
        foreach (GameObject obj in vrObjects)
        {
            Destroy(obj);
        }
        Debug.Log($"VR object 태그 오브젝트 {vrObjects.Length}개 삭제됨");
    }
}