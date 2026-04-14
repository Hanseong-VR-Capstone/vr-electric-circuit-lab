using UnityEngine;

// A LED node emitting light based on start/stop signals from the previous node in the chain.
public class LEDNode : MonoBehaviour
{
    // Previous linked LED node.
    public LEDNode prevNode = null;
    // Start/stop signals for the next linked LED node.
    public bool nextStart { get; private set; } = false;

    // Mark in GUI if first node in the chain.
    public bool isFirstNode = false;
    // Point light enable control.
    public bool isPointLightEn = false;
    // Slow light progress indicator.
    public bool slowProgress = false;

    [Header("Add MySelf Control")]
    [SerializeField] private bool useAutoPlay = true;           // 기존 에셋 처럼 자동 반복 재생
    [SerializeField] private bool externalPowerOn = false;      // 외부 전원 상태


    // Attached components.
    private Light pointLight = null;
    private Renderer rend = null;

    // Emitted light intensity parameters.
    private float intensity = 0;
    private float minIntensity = 0;
    private float maxIntensity = 1.0f;
    private float onTime = 0;
    private float maxOnTime = 1.0f;
    private float onTimerSpeed = 2.0f;
    private float offTime = 0;
    private float maxOffTime = 20.0f;
    private float offTimerSpeed = 20.0f;
    // Emitted light increase/decrease controls.
    private enum LightState { INCR, DECR, IDLE}
    private LightState lightState = LightState.IDLE;

    void Start()
    {
        // Init the attached components.
        pointLight = this.GetComponent<Light>();
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        ResolveNodeState();
        UpdateColor();
    }

    // ======== 기존 코드 외 내가 추가한 외부에서 사용할 메서드 ========

    // 외부에서 "전원 켜기"로  첫 노드에 호출
    private void PowerOn(bool immediateStart = true)
    {
        externalPowerOn = true;

        if (immediateStart && isFirstNode)
        {
            StartPulse();
        }
    }

    // 외부에서 "전원 끄기"
    private void PowerOff(bool immediateOff = true)
    {
        externalPowerOn = false;
        nextStart = false;
        
        if (immediateOff)
        {
            ForceOff();
        }
    }

    // ture/false로 외부에서 전원 상태 변경
    public void SetPower(bool isOn, bool immediate = true)
    {
        if (isOn)
        {
            PowerOn(immediate);
        }
        else
        {
            PowerOff(immediate);
        }
    }

    // 즉시 현재 노드 상태 초기화
    public void ForceOff()
    {
        nextStart = false;
        intensity = minIntensity;
        onTime = 0;
        offTime = 0;
        lightState = LightState.IDLE;

        if (pointLight != null)
            pointLight.enabled = false;

        UpdateColor();
    }

    // 자동 재생 켜고 끄기
    public void SetAutoPlay(bool autoPlay)
    {
        useAutoPlay = autoPlay;
    }

    // 외부에서 현재 전원 상태 확인용
    public bool IsPoweredOn()
    {
        return externalPowerOn;
    }

    // =======================================
    // Decides on state of fading in/out based on the input parameters.
    private void ResolveNodeState()
    {
        switch (lightState)
        {
            case LightState.INCR:
                IntensityIncrease();
                break;
            case LightState.DECR:
                IntensityDecrease();
                break;
            case LightState.IDLE:
                LightIdle();
                break;
            default:
                break;
        }
    }

    // Gradually increase intensity level up to the maximum-level defined in GUI.
    private void IntensityIncrease()
    {
        if (!slowProgress)
        {
            if (nextStart == false)
                nextStart = true;
        }

        intensity = maxIntensity;

        // If point light is enabled, light it up.
        // 기존 에셋 코드에 pointLight != null 체크 추가
        if (isPointLightEn && pointLight != null)
            pointLight.enabled = true;

        // If ON timer handn't been reached yet
        if (onTime < maxOnTime)
            onTime += onTimerSpeed * Time.deltaTime;
        // ON timer had been reached.
        else
        {
            onTime = 0;
            // Start decreasing intensity.
            lightState = LightState.DECR;
        }
    }

    // Gradually decrease intensity level down to 0.
    private void IntensityDecrease()
    {
        intensity = minIntensity;
        
        // If there's a point light component enabled, disable the point light.
        // 기존 에셋 코드에 pointLight != null 체크 추가
        if (isPointLightEn && pointLight != null)
            pointLight.enabled = false;

        if (slowProgress)
        {
            if (nextStart == false)
                nextStart = true;
        }
        else
        {
            // Stop sending the start signal to the next node.
            if (nextStart == true)
                nextStart = false;
        }

        // Move to the idle (no light) state.
        lightState = LightState.IDLE;
    }

    // Light idles until signaled to do otherwise.
    private void LightIdle()
    {
        if (slowProgress)
        {
            if (nextStart == true)
                nextStart = false;
        }

        // If there is a previous linked node
        if (prevNode != null)
        {
            if (prevNode.nextStart)
            {
                // Start increasing light intensity.
                lightState = LightState.INCR;
            }
        }
        // If first node in the chain
        else if (isFirstNode)
        {
            // 내가 추가한 코드 => 자동 재생 또는 외부 전원 둘 중 하나라도 켜져있으면 흐름 시작
            if (!useAutoPlay && !externalPowerOn)
                return;

            // If max Idle time wasn't reached yet
            if (offTime < maxOffTime)
                offTime += offTimerSpeed * Time.deltaTime;
            else
            {
                // 기존 에셋 코드
                StartPulse();
            }
        }
    }

    // 기존 에셋의 코드를 메서드로 분리하여 공통으로 사용
    private void StartPulse()
    {
        offTime = 0;
        // Start increasing light intensity.
        lightState = LightState.INCR;
        // Light-up the next node in chain.
        if (nextStart == false)
            nextStart = true;
    }

    // Updates the calculated LED color and intensity level.
    private void UpdateColor()
    {
        Material mat = rend.material;
        Color baseColor = mat.color;

        // Calculate the resulting color based on the intensity.
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(intensity);
        mat.SetColor("_EmissionColor", finalColor);
    }
}
