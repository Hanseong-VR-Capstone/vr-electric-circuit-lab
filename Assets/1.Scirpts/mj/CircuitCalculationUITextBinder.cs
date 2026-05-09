using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRCircuit.Query;

namespace VRCircuit.UIBridge
{
    public class CircuitCalculationUITextBinder : MonoBehaviour
    {
        private enum DisplayMode
        {
            Summary,
            VoltageOnly
        }

        [SerializeField] private CircuitQueryService queryService;
        [SerializeField] private TMP_Text tmpText;
        [SerializeField] private Text uiText;
        [SerializeField] private DisplayMode displayMode = DisplayMode.Summary;
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private bool enableDebugLogs = false;

        private readonly StringBuilder builder = new StringBuilder();

        private void Awake()
        {
            ResolveReferencesIfNeeded();
        }

        private void Start()
        {
            RefreshText();
        }

        private void Update()
        {
            if (!updateEveryFrame)
            {
                return;
            }

            RefreshText();
        }

        public void RefreshText()
        {
            ResolveReferencesIfNeeded();

            if (queryService == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("CircuitCalculationUITextBinder: CircuitQueryService is missing.");
                }

                SetText("VOLTAGE : --");
                return;
            }

            CircuitUIData data = queryService.GetUIData();
            if (data == null)
            {
                SetText("VOLTAGE : --");
                return;
            }

            SetText(BuildText(data));
        }

        private void ResolveReferencesIfNeeded()
        {
            if (queryService == null)
            {
                queryService = FindFirstObjectByType<CircuitQueryService>();
            }

            if (tmpText == null)
            {
                tmpText = GetComponent<TMP_Text>();
            }

            if (uiText == null)
            {
                uiText = GetComponent<Text>();
            }
        }

        private string BuildText(CircuitUIData data)
        {
            if (displayMode == DisplayMode.VoltageOnly)
            {
                return $"VOLTAGE : {FormatVoltage(data.TotalVoltage)}";
            }

            builder.Clear();
            builder.AppendLine($"STATE : {data.State}");
            builder.AppendLine($"VOLTAGE : {FormatVoltage(data.TotalVoltage)}");
            builder.AppendLine($"RESISTANCE : {FormatResistance(data.TotalResistance)}");
            builder.AppendLine($"CURRENT : {FormatCurrent(data.TotalCurrent)}");

            if (data.PerLoadVoltage != null && data.PerLoadVoltage.Count > 0)
            {
                builder.AppendLine("LOAD VOLTAGE:");
                foreach (KeyValuePair<string, float> pair in data.PerLoadVoltage)
                {
                    builder.AppendLine($"- {pair.Key}: {FormatVoltage(pair.Value)}");
                }
            }

            if (data.PerBranchCurrent != null && data.PerBranchCurrent.Count > 0)
            {
                builder.AppendLine("BRANCH CURRENT:");
                foreach (KeyValuePair<string, float> pair in data.PerBranchCurrent)
                {
                    builder.AppendLine($"- {pair.Key}: {FormatCurrent(pair.Value)}");
                }
            }

            return builder.ToString().TrimEnd();
        }

        private void SetText(string value)
        {
            if (tmpText != null)
            {
                tmpText.text = value;
                return;
            }

            if (uiText != null)
            {
                uiText.text = value;
            }
        }

        private string FormatVoltage(float voltage)
        {
            return $"{voltage:0.###}V";
        }

        private string FormatResistance(float resistance)
        {
            return $"{resistance:0.###}Ω";
        }

        private string FormatCurrent(float current)
        {
            return $"{current:0.###}A";
        }
    }
}
