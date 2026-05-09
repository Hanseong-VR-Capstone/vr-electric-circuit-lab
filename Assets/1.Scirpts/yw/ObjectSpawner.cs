using System.Collections.Generic;
using UnityEngine;
using VRCircuit.Effects;
using VRCircuit.Registration;
using VRCircuit.Runtime;

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
    [SerializeField] private CircuitRuntimeRoot runtimeRoot;

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

        GameObject spawnedObject = Instantiate(selected, entry.spawnPoint.position, entry.spawnPoint.rotation);
        InitializeCircuitPartIfNeeded(spawnedObject, selected.name);
    }

    public void DeleteAllVRObjects()
    {
        GameObject[] vrObjects = GameObject.FindGameObjectsWithTag("VR object");
        foreach (GameObject obj in vrObjects)
        {
            Destroy(obj);
        }

        ClearRuntimeCircuitData();
        ForceBreadboardCurrentVisualsOff();

        Debug.Log($"VR object 태그 오브젝트 {vrObjects.Length}개 삭제됨");
    }

    private void InitializeCircuitPartIfNeeded(GameObject spawnedObject, string basePrefabName)
    {
        if (spawnedObject == null)
        {
            return;
        }

        if (!CircuitSpawnedPartInitializer.ContainsCircuitRegistrar(spawnedObject))
        {
            return;
        }

        if (runtimeRoot == null)
        {
            runtimeRoot = FindFirstObjectByType<CircuitRuntimeRoot>();
        }

        CircuitSpawnedPartInitializer initializer = spawnedObject.GetComponent<CircuitSpawnedPartInitializer>();
        if (initializer == null)
        {
            initializer = spawnedObject.AddComponent<CircuitSpawnedPartInitializer>();
        }

        initializer.InitializeSpawnedPart(runtimeRoot, basePrefabName);
    }

    private void ClearRuntimeCircuitData()
    {
        if (runtimeRoot == null)
        {
            runtimeRoot = FindFirstObjectByType<CircuitRuntimeRoot>();
        }

        if (runtimeRoot == null)
        {
            return;
        }

        runtimeRoot.EnsureInitialized();
        runtimeRoot.Context?.ClearDynamicCircuitData();
    }

    private void ForceBreadboardCurrentVisualsOff()
    {
        BreadboardCurrentVisualController[] visualControllers =
            FindObjectsByType<BreadboardCurrentVisualController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < visualControllers.Length; i++)
        {
            BreadboardCurrentVisualController visualController = visualControllers[i];
            if (visualController != null)
            {
                visualController.ForceTurnOffAll();
            }
        }
    }
}
