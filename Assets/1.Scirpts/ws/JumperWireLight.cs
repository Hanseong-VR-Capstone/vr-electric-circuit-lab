using UnityEngine;
using System.Collections.Generic;
using Obi;

public class JumperWireLight : MonoBehaviour
{
    [Header("Current Flow Visual")]
    [SerializeField] private ObiPathSmoother pathSmoother;
    [SerializeField] private GameObject lightNodesPrefab;   // 자식 29개가 들어있는 부모 프리팹
    [SerializeField] private Transform lightNodeParent;

    private GameObject lightNodesRootInstance;
    private readonly List<GameObject> lightNodes = new();
    private readonly List<LEDNode> ledNodes = new();

    private bool isLightOn = false;
    private bool startFromRight = false;

    private void Awake()
    {
        if (pathSmoother == null)
            pathSmoother = GetComponentInChildren<ObiPathSmoother>();

        if (lightNodeParent == null)
            lightNodeParent = transform;

        if (lightNodesPrefab == null)
        {
            Debug.LogWarning("lightNodesPrefab이 설정되지 않았습니다.");
            return;
        }
    }

    private void LateUpdate()
    {
        if (isLightOn)
            UpdateLightNodes();
    }

    private void OnDisable()
    {
        LightOff();
    }

    private void OnDestroy()
    {
        LightOff();
    }

    public void LightOn(PinRole startPinRole)
    {
        if (pathSmoother == null || lightNodesPrefab == null)
        {
            Debug.LogWarning("pathSmoother 또는 lightNodesPrefab이 설정되지 않았습니다.");
            return;
        }

        bool newStartFromRight = IsRightStart(startPinRole);

        if (!isLightOn || newStartFromRight != startFromRight || lightNodesRootInstance == null)
        {
            startFromRight = newStartFromRight;
            isLightOn = true;
            CreateLightNodesRoot();
        }

        UpdateLightNodes();

        if (ledNodes.Count > 0)
        {
            ledNodes[0].SetAutoPlay(false);
            ledNodes[0].SetPower(true, true);
        }
        else
        {
            Debug.LogWarning("LEDNode가 하나도 없습니다.");
        }
    }

    public void LightOff()
    {
        isLightOn = false;

        if (ledNodes.Count > 0)
            ledNodes[0].SetPower(false, true);

        ClearLightNodes();
    }

    private bool IsRightStart(PinRole startPinRole)
    {
        return startPinRole == PinRole.Wire_B;
    }

    private void CreateLightNodesRoot()
    {
        ClearLightNodes();

        lightNodesRootInstance = Instantiate(lightNodesPrefab, lightNodeParent);
        lightNodesRootInstance.name = "LED_Nodes_Root";

        // 직계 자식들만 노드로 사용
        for (int i = 0; i < lightNodesRootInstance.transform.childCount; i++)
        {
            Transform child = lightNodesRootInstance.transform.GetChild(i);
            lightNodes.Add(child.gameObject);

            LEDNode ledNode = child.GetComponent<LEDNode>();
            if (ledNode != null)
                ledNodes.Add(ledNode);
            else
                Debug.LogWarning($"LEDNode 컴포넌트가 없는 자식이 있습니다: {child.name}");
        }

        if (lightNodes.Count == 0)
            Debug.LogWarning("LED_Nodes 프리팹 안에 자식 노드가 없습니다.");
    }

    private void ClearLightNodes()
    {
        lightNodes.Clear();
        ledNodes.Clear();

        if (lightNodesRootInstance != null)
        {
            Destroy(lightNodesRootInstance);
            lightNodesRootInstance = null;
        }
    }

    private void UpdateLightNodes()
    {
        if (pathSmoother == null || lightNodes.Count == 0)
            return;

        Transform smootherTransform = pathSmoother.transform;
        int count = lightNodes.Count;

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0f : (float)i / (count - 1);

            if (startFromRight)
                t = 1f - t;

            var section = pathSmoother.GetSectionAt(t);

            Vector3 worldPos = smootherTransform.TransformPoint(section.position);
            lightNodes[i].transform.position = worldPos;

            Quaternion worldRot = smootherTransform.rotation * Quaternion.LookRotation(section.tangent, section.binormal);

            if (startFromRight)
                worldRot *= Quaternion.Euler(0f, 180f, 0f);

            lightNodes[i].transform.rotation = worldRot;
        }
    }
}