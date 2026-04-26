using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRCircuit.Analysis;
using VRCircuit.Calculation;
using VRCircuit.Data;
using VRCircuit.Evaluation;
using VRCircuit.Runtime;

namespace VRCircuit.Debugging
{
    public class CircuitCalculationDebugRunner : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;
        [SerializeField] private bool runOnStart;
        [SerializeField] private KeyCode runKey = KeyCode.C;

        private CircuitEvaluator evaluator;
        private CircuitCalculationHelper calculationHelper;

        private void Awake()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
            }

            if (runtimeRoot == null)
            {
                Debug.LogWarning("CircuitCalculationDebugRunner: CircuitRuntimeRoot is missing.");
                return;
            }

            evaluator = new CircuitEvaluator(runtimeRoot.Context);
            calculationHelper = new CircuitCalculationHelper(runtimeRoot.Context);
        }

        private void Start()
        {
            if (runOnStart)
            {
                PrintCalculationSummary();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(runKey))
            {
                PrintCalculationSummary();
            }
        }

        public void PrintCalculationSummary()
        {
            if (runtimeRoot == null)
            {
                Debug.LogWarning("CircuitCalculationDebugRunner: Runtime root is missing.");
                return;
            }

            runtimeRoot.EnsureInitialized();

            CircuitContext context = runtimeRoot.Context;
            if (context == null)
            {
                Debug.LogWarning("CircuitCalculationDebugRunner: Runtime context is missing.");
                return;
            }

            evaluator = new CircuitEvaluator(context);
            calculationHelper = new CircuitCalculationHelper(context);

            CircuitCurrentFlowAnalyzer flowAnalyzer = new CircuitCurrentFlowAnalyzer(context);
            CircuitCurrentFlowResult flowResult = flowAnalyzer.Analyze();

            CircuitState state = evaluator.Evaluate();
            CircuitCalculationResult result = calculationHelper.Calculate();

            Debug.Log(BuildSummary(state, context, result, flowResult));
        }

        private string BuildSummary(
            CircuitState state,
            CircuitContext context,
            CircuitCalculationResult result,
            CircuitCurrentFlowResult flowResult)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("[Calculation Debug]");
            builder.AppendLine($"State: {state}");
            builder.AppendLine($"RegisteredResistorCount: {GetRegisteredResistorCount(context)}");

            AppendContextCountSummary(builder, context);

            if (result == null)
            {
                builder.AppendLine("Result: NULL");
                return builder.ToString();
            }

            builder.AppendLine($"HasValidCircuit: {result.HasValidCircuit}");
            builder.AppendLine($"ActiveLoadCount: {result.ActiveLoadCount}");
            builder.AppendLine($"TotalVoltage: {result.TotalVoltage}");
            builder.AppendLine($"TotalResistance: {result.TotalResistance}");
            builder.AppendLine($"TotalCurrent: {result.TotalCurrent}");

            AppendFloatDictionary(builder, "PerLoadVoltage", result.PerLoadVoltage);
            AppendFloatDictionary(builder, "PerBranchCurrent", result.PerBranchCurrent);

            AppendCurrentFlowSummary(builder, flowResult);
            AppendRegisteredWireSummary(builder, context);
            AppendRegisteredResistorSummary(builder, context);
            AppendResistorDetails(builder, context);
            AppendWireDetails(builder, context);
            AppendRailNodeDetails(builder, context);
            AppendConnectedPinSummary(builder, context);
            AppendDiagnosticWarnings(builder, context);

            return builder.ToString();
        }

        private void AppendContextCountSummary(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Context Count Summary]");

            if (context == null)
            {
                builder.AppendLine("- context is null");
                return;
            }

            builder.AppendLine($"Pins.Count: {context.Pins.Count}");
            builder.AppendLine($"Sockets.Count: {context.Sockets.Count}");
            builder.AppendLine($"Wires.Count: {context.Wires.Count}");
            builder.AppendLine($"Switches.Count: {context.Switches.Count}");
            builder.AppendLine($"Leds.Count: {context.Leds.Count}");
            builder.AppendLine($"Resistors.Count: {context.Resistors.Count}");
        }

        private void AppendCurrentFlowSummary(StringBuilder builder, CircuitCurrentFlowResult flowResult)
        {
            builder.AppendLine();
            builder.AppendLine("[Current Flow Summary]");

            if (flowResult == null)
            {
                builder.AppendLine("- flowResult is null");
                return;
            }

            builder.AppendLine($"IsFlowing: {flowResult.IsFlowing}");

            builder.AppendLine("ActiveWireIds:");
            if (flowResult.ActiveWireIds == null || flowResult.ActiveWireIds.Count == 0)
            {
                builder.AppendLine("- none");
            }
            else
            {
                for (int i = 0; i < flowResult.ActiveWireIds.Count; i++)
                {
                    builder.AppendLine($"- {flowResult.ActiveWireIds[i]}");
                }
            }

            builder.AppendLine("WireDirections:");
            if (flowResult.WireDirections == null || flowResult.WireDirections.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            for (int i = 0; i < flowResult.WireDirections.Count; i++)
            {
                CurrentFlowWireDirection direction = flowResult.WireDirections[i];
                if (direction == null)
                {
                    builder.AppendLine("- direction: NULL");
                    continue;
                }

                builder.AppendLine($"- wireId={direction.WireId}");
                builder.AppendLine($"  fromNodeId={ValueOrNone(direction.FromNodeId)}");
                builder.AppendLine($"  toNodeId={ValueOrNone(direction.ToNodeId)}");
                builder.AppendLine($"  fromPinId={ValueOrNone(direction.FromPinId)}");
                builder.AppendLine($"  toPinId={ValueOrNone(direction.ToPinId)}");
                builder.AppendLine($"  fromSocketId={ValueOrNone(direction.FromSocketId)}");
                builder.AppendLine($"  toSocketId={ValueOrNone(direction.ToSocketId)}");
            }
        }

        private void AppendRegisteredWireSummary(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Registered Wire Summary]");

            if (context == null || context.Wires == null || context.Wires.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            for (int i = 0; i < context.Wires.Count; i++)
            {
                CircuitWire wire = context.Wires[i];
                if (wire == null)
                {
                    continue;
                }

                builder.AppendLine($"- wireId={wire.WireId} | pinAId={wire.PinAId} | pinBId={wire.PinBId}");
            }
        }

        private void AppendRegisteredResistorSummary(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Registered Resistor Summary]");

            if (context == null || context.Resistors == null || context.Resistors.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            for (int i = 0; i < context.Resistors.Count; i++)
            {
                CircuitResistor resistor = context.Resistors[i];
                if (resistor == null)
                {
                    continue;
                }

                builder.AppendLine(
                    $"- resistorId={resistor.ResistorId} | pinAId={resistor.PinAId} | pinBId={resistor.PinBId} | resistanceOhms={resistor.ResistanceOhms}");
            }
        }

        private void AppendResistorDetails(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Resistor Detail Debug]");

            if (context == null || context.Resistors == null || context.Resistors.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            for (int i = 0; i < context.Resistors.Count; i++)
            {
                CircuitResistor resistor = context.Resistors[i];
                if (resistor == null)
                {
                    builder.AppendLine("- resistor: NULL");
                    continue;
                }

                GetPinDebugInfo(context, resistor.PinAId, out bool pinAExists, out string pinASocketId, out string pinANodeId);
                GetPinDebugInfo(context, resistor.PinBId, out bool pinBExists, out string pinBSocketId, out string pinBNodeId);

                bool isConnectedBothPins =
                    pinAExists &&
                    pinBExists &&
                    !string.IsNullOrEmpty(pinASocketId) &&
                    !string.IsNullOrEmpty(pinBSocketId) &&
                    !string.IsNullOrEmpty(pinANodeId) &&
                    !string.IsNullOrEmpty(pinBNodeId);

                bool isSameNode = isConnectedBothPins && pinANodeId == pinBNodeId;
                bool isActiveCandidate = isConnectedBothPins && !isSameNode && resistor.ResistanceOhms > 0f;

                builder.AppendLine($"- resistorId={resistor.ResistorId}");
                builder.AppendLine($"  resistanceOhms={resistor.ResistanceOhms}");
                builder.AppendLine($"  pinAId={resistor.PinAId}");
                builder.AppendLine($"  pinA exists? {pinAExists}");
                builder.AppendLine($"  pinA currentSocketId={ValueOrNone(pinASocketId)}");
                builder.AppendLine($"  pinA nodeId={ValueOrNone(pinANodeId)}");
                builder.AppendLine($"  pinBId={resistor.PinBId}");
                builder.AppendLine($"  pinB exists? {pinBExists}");
                builder.AppendLine($"  pinB currentSocketId={ValueOrNone(pinBSocketId)}");
                builder.AppendLine($"  pinB nodeId={ValueOrNone(pinBNodeId)}");
                builder.AppendLine($"  isConnectedBothPins? {isConnectedBothPins}");
                builder.AppendLine($"  isSameNode? {isSameNode}");
                builder.AppendLine($"  isActiveCandidate? {isActiveCandidate}");
            }
        }

        private void AppendWireDetails(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Wire Detail Debug]");

            if (context == null || context.Wires == null || context.Wires.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            for (int i = 0; i < context.Wires.Count; i++)
            {
                CircuitWire wire = context.Wires[i];
                if (wire == null)
                {
                    builder.AppendLine("- wire: NULL");
                    continue;
                }

                GetPinDebugInfo(context, wire.PinAId, out bool pinAExists, out string pinASocketId, out string pinANodeId);
                GetPinDebugInfo(context, wire.PinBId, out bool pinBExists, out string pinBSocketId, out string pinBNodeId);

                bool isConnectedBothPins =
                    pinAExists &&
                    pinBExists &&
                    !string.IsNullOrEmpty(pinASocketId) &&
                    !string.IsNullOrEmpty(pinBSocketId) &&
                    !string.IsNullOrEmpty(pinANodeId) &&
                    !string.IsNullOrEmpty(pinBNodeId);

                bool isSameNode = isConnectedBothPins && pinANodeId == pinBNodeId;

                builder.AppendLine($"- wireId={wire.WireId}");
                builder.AppendLine($"  pinAId={wire.PinAId}");
                builder.AppendLine($"  pinA exists? {pinAExists}");
                builder.AppendLine($"  pinA currentSocketId={ValueOrNone(pinASocketId)}");
                builder.AppendLine($"  pinA nodeId={ValueOrNone(pinANodeId)}");
                builder.AppendLine($"  pinBId={wire.PinBId}");
                builder.AppendLine($"  pinB exists? {pinBExists}");
                builder.AppendLine($"  pinB currentSocketId={ValueOrNone(pinBSocketId)}");
                builder.AppendLine($"  pinB nodeId={ValueOrNone(pinBNodeId)}");
                builder.AppendLine($"  isConnectedBothPins? {isConnectedBothPins}");
                builder.AppendLine($"  isSameNode? {isSameNode}");
            }
        }

        private void AppendRailNodeDetails(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Rail Node Debug]");

            List<string> plusRailNodes = GetPlusRailNodes();
            List<string> minusRailNodes = GetMinusRailNodes();

            builder.AppendLine("Plus rail nodes:");
            AppendRailNodeGroup(builder, context, plusRailNodes);

            builder.AppendLine("Minus rail nodes:");
            AppendRailNodeGroup(builder, context, minusRailNodes);
        }

        private void AppendRailNodeGroup(StringBuilder builder, CircuitContext context, List<string> railNodes)
        {
            for (int i = 0; i < railNodes.Count; i++)
            {
                string nodeId = railNodes[i];
                List<string> connectedPinIds = GetConnectedPinIdsOnNode(context, nodeId);
                bool hasAnyConnectedPinOnNode = connectedPinIds.Count > 0;

                builder.AppendLine($"- nodeId={nodeId}");
                builder.AppendLine($"  hasAnyConnectedPinOnNode? {hasAnyConnectedPinOnNode}");
                builder.AppendLine($"  connectedPinIds={JoinListOrNone(connectedPinIds)}");
            }
        }

        private void AppendConnectedPinSummary(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Connected Pin Summary]");

            if (context == null || context.Pins == null || context.Pins.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            bool hasAnyConnectedPin = false;

            for (int i = 0; i < context.Pins.Count; i++)
            {
                CircuitPin pin = context.Pins[i];
                if (pin == null || string.IsNullOrEmpty(pin.CurrentSocketId))
                {
                    continue;
                }

                hasAnyConnectedPin = true;

                string nodeId = null;
                CircuitSocket socket = context.GetSocketById(pin.CurrentSocketId);
                if (socket != null)
                {
                    nodeId = socket.NodeId;
                }

                builder.AppendLine($"- pinId={pin.PinId}");
                builder.AppendLine($"  ownerType={pin.OwnerType}");
                builder.AppendLine($"  ownerId={ValueOrNone(pin.OwnerId)}");
                builder.AppendLine($"  currentSocketId={ValueOrNone(pin.CurrentSocketId)}");
                builder.AppendLine($"  nodeId={ValueOrNone(nodeId)}");
            }

            if (!hasAnyConnectedPin)
            {
                builder.AppendLine("- none");
            }
        }

        private void AppendDiagnosticWarnings(StringBuilder builder, CircuitContext context)
        {
            builder.AppendLine();
            builder.AppendLine("[Diagnostic Warnings]");

            bool hasWarning = false;

            if (context == null)
            {
                builder.AppendLine("- WARNING: Context is null.");
                return;
            }

            if (context.Wires.Count == 0)
            {
                hasWarning = true;
                builder.AppendLine("- WARNING: No CircuitWire registered. Jumper wires may not be registered into CircuitContext.");
            }

            int connectedPinCount = GetConnectedPinCount(context);
            if (connectedPinCount == 0)
            {
                hasWarning = true;
                builder.AppendLine("- WARNING: No connected pins found. BreadboardHoleBridge may not be receiving PartPin trigger events or may be using a different context.");
            }

            if (context.Resistors.Count > 0 && AllResistorPinsMissing(context))
            {
                hasWarning = true;
                builder.AppendLine("- WARNING: Resistors are registered, but resistor pin ids do not exist in CircuitContext.Pins. Check PartPin id resolution and registrar pin ids.");
            }

            if (AllRailNodesEmpty(context))
            {
                hasWarning = true;
                builder.AppendLine("- WARNING: No pins are connected to rail nodes. Check holeIndex mapping, socket registration, or bridge connection calls.");
            }

            if (!hasWarning)
            {
                builder.AppendLine("- none");
            }
        }

        private void GetPinDebugInfo(
            CircuitContext context,
            string pinId,
            out bool pinExists,
            out string currentSocketId,
            out string nodeId)
        {
            pinExists = false;
            currentSocketId = null;
            nodeId = null;

            if (context == null || string.IsNullOrEmpty(pinId))
            {
                return;
            }

            CircuitPin pin = context.GetPinById(pinId);
            if (pin == null)
            {
                return;
            }

            pinExists = true;
            currentSocketId = pin.CurrentSocketId;

            if (string.IsNullOrEmpty(currentSocketId))
            {
                return;
            }

            CircuitSocket socket = context.GetSocketById(currentSocketId);
            if (socket == null || string.IsNullOrEmpty(socket.NodeId))
            {
                return;
            }

            nodeId = socket.NodeId;
        }

        private List<string> GetConnectedPinIdsOnNode(CircuitContext context, string nodeId)
        {
            List<string> pinIds = new List<string>();

            if (context == null || string.IsNullOrEmpty(nodeId) || context.Pins == null)
            {
                return pinIds;
            }

            for (int i = 0; i < context.Pins.Count; i++)
            {
                CircuitPin pin = context.Pins[i];
                if (pin == null || string.IsNullOrEmpty(pin.CurrentSocketId))
                {
                    continue;
                }

                CircuitSocket socket = context.GetSocketById(pin.CurrentSocketId);
                if (socket == null || string.IsNullOrEmpty(socket.NodeId))
                {
                    continue;
                }

                if (socket.NodeId == nodeId)
                {
                    pinIds.Add(pin.PinId);
                }
            }

            return pinIds;
        }

        private List<string> GetPlusRailNodes()
        {
            List<string> nodes = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                nodes.Add($"TopPlus_{i}");
            }

            for (int i = 0; i < 5; i++)
            {
                nodes.Add($"BottomPlus_{i}");
            }

            return nodes;
        }

        private List<string> GetMinusRailNodes()
        {
            List<string> nodes = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                nodes.Add($"TopMinus_{i}");
            }

            for (int i = 0; i < 5; i++)
            {
                nodes.Add($"BottomMinus_{i}");
            }

            return nodes;
        }

        private int GetRegisteredResistorCount(CircuitContext context)
        {
            if (context == null || context.Resistors == null)
            {
                return 0;
            }

            return context.Resistors.Count;
        }

        private int GetConnectedPinCount(CircuitContext context)
        {
            if (context == null || context.Pins == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < context.Pins.Count; i++)
            {
                CircuitPin pin = context.Pins[i];
                if (pin != null && !string.IsNullOrEmpty(pin.CurrentSocketId))
                {
                    count++;
                }
            }

            return count;
        }

        private bool AllResistorPinsMissing(CircuitContext context)
        {
            if (context == null || context.Resistors == null || context.Resistors.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < context.Resistors.Count; i++)
            {
                CircuitResistor resistor = context.Resistors[i];
                if (resistor == null)
                {
                    continue;
                }

                bool pinAExists = context.GetPinById(resistor.PinAId) != null;
                bool pinBExists = context.GetPinById(resistor.PinBId) != null;

                if (pinAExists || pinBExists)
                {
                    return false;
                }
            }

            return true;
        }

        private bool AllRailNodesEmpty(CircuitContext context)
        {
            List<string> railNodes = GetPlusRailNodes();
            railNodes.AddRange(GetMinusRailNodes());

            for (int i = 0; i < railNodes.Count; i++)
            {
                if (GetConnectedPinIdsOnNode(context, railNodes[i]).Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void AppendFloatDictionary(
            StringBuilder builder,
            string title,
            IReadOnlyDictionary<string, float> values)
        {
            builder.AppendLine($"{title}:");

            if (values == null || values.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            foreach (KeyValuePair<string, float> pair in values)
            {
                builder.AppendLine($"- {pair.Key}: {pair.Value}");
            }
        }

        private string JoinListOrNone(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "none";
            }

            return string.Join(", ", values);
        }

        private string ValueOrNone(string value)
        {
            return string.IsNullOrEmpty(value) ? "none" : value;
        }
    }
}
