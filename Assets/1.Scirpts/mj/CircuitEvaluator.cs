using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRCircuit.Data;

namespace VRCircuit.Evaluation
{
    public class CircuitEvaluator
    {
        private const float DefaultRailSupplyVoltage = 3.0f;
        private const bool EnableGraphDebugLogs = false;

        private readonly CircuitContext context;

        public CircuitEvaluator(CircuitContext context)
        {
            this.context = context;
        }

        public CircuitState Evaluate()
        {
            if (context == null)
            {
                return CircuitState.Open;
            }

            Dictionary<string, List<string>> graph = BuildNodeGraph();
            List<string> plusNodes = GetPowerPlusNodes();
            List<string> minusNodes = GetPowerMinusNodes();

            if (EnableGraphDebugLogs)
            {
                DebugGraph(graph, plusNodes, minusNodes);
            }

            if (!HasRailCircuitPath(plusNodes, minusNodes, graph))
            {
                return CircuitState.Open;
            }

            if (IsValidLedPathFromRails(graph, plusNodes, minusNodes))
            {
                return CircuitState.LedOn;
            }

            return DecideNonLedCircuitState(plusNodes, minusNodes, graph);
        }

        public float GetRailSupplyVoltage()
        {
            return DefaultRailSupplyVoltage;
        }

        public int CountActiveLoads()
        {
            if (context == null)
            {
                return 0;
            }

            int loadCount = 0;

            for (int i = 0; i < context.Leds.Count; i++)
            {
                CircuitLed led = context.Leds[i];
                if (led == null)
                {
                    continue;
                }

                if (!TryGetNodeIdFromPin(led.AnodePinId, out _) ||
                    !TryGetNodeIdFromPin(led.CathodePinId, out _))
                {
                    continue;
                }

                loadCount++;
            }

            return loadCount;
        }

        public float CalculateSeriesVoltagePerLoad()
        {
            float supplyVoltage = GetRailSupplyVoltage();
            int loadCount = CountActiveLoads();

            if (supplyVoltage <= 0f || loadCount <= 0)
            {
                return 0f;
            }

            return supplyVoltage / loadCount;
        }

        public float CalculateParallelBranchVoltage()
        {
            return GetRailSupplyVoltage();
        }

        private List<string> GetPowerPlusNodes()
        {
            List<string> nodes = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                nodes.Add($"TopPlus_{i}");
                nodes.Add($"BottomPlus_{i}");
            }

            return nodes;
        }

        private List<string> GetPowerMinusNodes()
        {
            List<string> nodes = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                nodes.Add($"TopMinus_{i}");
                nodes.Add($"BottomMinus_{i}");
            }

            return nodes;
        }

        private bool TryGetNodeIdFromPin(string pinId, out string nodeId)
        {
            nodeId = null;

            if (context == null || string.IsNullOrEmpty(pinId))
            {
                return false;
            }

            CircuitPin pin = context.GetPinById(pinId);
            if (pin == null || string.IsNullOrEmpty(pin.CurrentSocketId))
            {
                return false;
            }

            CircuitSocket socket = context.GetSocketById(pin.CurrentSocketId);
            if (socket == null || string.IsNullOrEmpty(socket.NodeId))
            {
                return false;
            }

            nodeId = socket.NodeId;
            return true;
        }

        private Dictionary<string, List<string>> BuildNodeGraph()
        {
            Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

            AddWireEdges(graph);
            AddSwitchEdges(graph);
            AddResistorEdges(graph);

            return graph;
        }

        private void AddWireEdges(Dictionary<string, List<string>> graph)
        {
            if (context == null)
            {
                return;
            }

            for (int i = 0; i < context.Wires.Count; i++)
            {
                CircuitWire wire = context.Wires[i];
                if (wire == null)
                {
                    continue;
                }

                if (!TryGetNodeIdFromPin(wire.PinAId, out string nodeA) ||
                    !TryGetNodeIdFromPin(wire.PinBId, out string nodeB))
                {
                    continue;
                }

                AddBidirectionalEdge(graph, nodeA, nodeB);
            }
        }

        private void AddSwitchEdges(Dictionary<string, List<string>> graph)
        {
            if (context == null)
            {
                return;
            }

            for (int i = 0; i < context.Switches.Count; i++)
            {
                CircuitSwitch circuitSwitch = context.Switches[i];
                if (circuitSwitch == null || !circuitSwitch.IsOn)
                {
                    continue;
                }

                if (!TryGetNodeIdFromPin(circuitSwitch.PinAId, out string nodeA) ||
                    !TryGetNodeIdFromPin(circuitSwitch.PinBId, out string nodeB))
                {
                    continue;
                }

                AddBidirectionalEdge(graph, nodeA, nodeB);
            }
        }

        private void AddResistorEdges(Dictionary<string, List<string>> graph)
        {
            if (context == null || context.Resistors == null)
            {
                return;
            }

            for (int i = 0; i < context.Resistors.Count; i++)
            {
                CircuitResistor resistor = context.Resistors[i];
                if (resistor == null)
                {
                    continue;
                }

                if (!TryGetNodeIdFromPin(resistor.PinAId, out string nodeA) ||
                    !TryGetNodeIdFromPin(resistor.PinBId, out string nodeB))
                {
                    continue;
                }

                if (nodeA == nodeB)
                {
                    continue;
                }

                AddBidirectionalEdge(graph, nodeA, nodeB);
            }
        }

        private void AddBidirectionalEdge(Dictionary<string, List<string>> graph, string nodeA, string nodeB)
        {
            if (string.IsNullOrEmpty(nodeA) || string.IsNullOrEmpty(nodeB))
            {
                return;
            }

            AddNeighbor(graph, nodeA, nodeB);
            AddNeighbor(graph, nodeB, nodeA);
        }

        private void AddNeighbor(Dictionary<string, List<string>> graph, string fromNode, string toNode)
        {
            if (!graph.TryGetValue(fromNode, out List<string> neighbors))
            {
                neighbors = new List<string>();
                graph[fromNode] = neighbors;
            }

            if (!neighbors.Contains(toNode))
            {
                neighbors.Add(toNode);
            }
        }

        private bool HasRailCircuitPath(
            List<string> plusNodes,
            List<string> minusNodes,
            Dictionary<string, List<string>> graph)
        {
            return HasPathBetweenAnyNodes(plusNodes, minusNodes, graph);
        }

        private bool HasPathBetweenAnyNodes(
            List<string> startNodes,
            List<string> targetNodes,
            Dictionary<string, List<string>> graph)
        {
            if (startNodes == null || targetNodes == null || graph == null)
            {
                return false;
            }

            for (int i = 0; i < startNodes.Count; i++)
            {
                for (int j = 0; j < targetNodes.Count; j++)
                {
                    if (HasPath(startNodes[i], targetNodes[j], graph))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidLedPathFromRails(
            Dictionary<string, List<string>> graph,
            List<string> plusNodes,
            List<string> minusNodes)
        {
            if (context == null || plusNodes == null || minusNodes == null)
            {
                return false;
            }

            for (int i = 0; i < context.Leds.Count; i++)
            {
                CircuitLed led = context.Leds[i];
                if (led == null)
                {
                    continue;
                }

                if (!TryGetNodeIdFromPin(led.AnodePinId, out string anodeNodeId) ||
                    !TryGetNodeIdFromPin(led.CathodePinId, out string cathodeNodeId))
                {
                    continue;
                }

                bool plusToAnode = CanAnyReach(plusNodes, anodeNodeId, graph);
                bool cathodeToMinus = CanReachAny(cathodeNodeId, minusNodes, graph);

                if (plusToAnode && cathodeToMinus)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanAnyReach(List<string> startNodes, string targetNodeId, Dictionary<string, List<string>> graph)
        {
            if (startNodes == null || string.IsNullOrEmpty(targetNodeId))
            {
                return false;
            }

            for (int i = 0; i < startNodes.Count; i++)
            {
                if (HasPath(startNodes[i], targetNodeId, graph))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanReachAny(string startNodeId, List<string> targetNodes, Dictionary<string, List<string>> graph)
        {
            if (string.IsNullOrEmpty(startNodeId) || targetNodes == null)
            {
                return false;
            }

            for (int i = 0; i < targetNodes.Count; i++)
            {
                if (HasPath(startNodeId, targetNodes[i], graph))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasBranching(
            Dictionary<string, List<string>> graph,
            List<string> startNodeIds,
            List<string> endNodeIds)
        {
            if (graph == null)
            {
                return false;
            }

            foreach (KeyValuePair<string, List<string>> pair in graph)
            {
                string nodeId = pair.Key;
                if (ContainsNode(startNodeIds, nodeId) || ContainsNode(endNodeIds, nodeId))
                {
                    continue;
                }

                List<string> neighbors = pair.Value;
                if (neighbors != null && neighbors.Count >= 3)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsNode(List<string> nodes, string nodeId)
        {
            if (nodes == null || string.IsNullOrEmpty(nodeId))
            {
                return false;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == nodeId)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountPaths(string startNode, string endNode, Dictionary<string, List<string>> graph, int maxSearch = 2)
        {
            if (string.IsNullOrEmpty(startNode) ||
                string.IsNullOrEmpty(endNode) ||
                graph == null ||
                maxSearch <= 0)
            {
                return 0;
            }

            if (startNode == endNode)
            {
                return 1;
            }

            if (!graph.ContainsKey(startNode))
            {
                return 0;
            }

            HashSet<string> visited = new HashSet<string>();
            return CountPathsDepthFirst(startNode, endNode, graph, visited, maxSearch);
        }

        private int CountPathsDepthFirst(
            string currentNode,
            string targetNode,
            Dictionary<string, List<string>> graph,
            HashSet<string> visited,
            int remainingLimit)
        {
            if (remainingLimit <= 0)
            {
                return 0;
            }

            if (currentNode == targetNode)
            {
                return 1;
            }

            visited.Add(currentNode);

            int pathCount = 0;

            if (graph.TryGetValue(currentNode, out List<string> neighbors) && neighbors != null)
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    string nextNode = neighbors[i];
                    if (string.IsNullOrEmpty(nextNode) || visited.Contains(nextNode))
                    {
                        continue;
                    }

                    int foundCount = CountPathsDepthFirst(nextNode, targetNode, graph, visited, remainingLimit - pathCount);
                    pathCount += foundCount;

                    if (pathCount >= remainingLimit)
                    {
                        break;
                    }
                }
            }

            visited.Remove(currentNode);
            return pathCount;
        }

        private bool IsSeriesCircuit(
            List<string> startNodes,
            List<string> endNodes,
            Dictionary<string, List<string>> graph)
        {
            if (!HasPathBetweenAnyNodes(startNodes, endNodes, graph))
            {
                return false;
            }

            return !HasBranching(graph, startNodes, endNodes);
        }

        private bool IsParallelCircuit(
            List<string> startNodes,
            List<string> endNodes,
            Dictionary<string, List<string>> graph)
        {
            if (!HasPathBetweenAnyNodes(startNodes, endNodes, graph))
            {
                return false;
            }

            if (HasBranching(graph, startNodes, endNodes))
            {
                return true;
            }

            return CountRailPaths(startNodes, endNodes, graph, 2) >= 2;
        }

        private int CountRailPaths(
            List<string> startNodes,
            List<string> endNodes,
            Dictionary<string, List<string>> graph,
            int maxSearch)
        {
            if (startNodes == null || endNodes == null || maxSearch <= 0)
            {
                return 0;
            }

            int pathCount = 0;

            for (int i = 0; i < startNodes.Count; i++)
            {
                for (int j = 0; j < endNodes.Count; j++)
                {
                    pathCount += CountPaths(startNodes[i], endNodes[j], graph, maxSearch - pathCount);

                    if (pathCount >= maxSearch)
                    {
                        return pathCount;
                    }
                }
            }

            return pathCount;
        }

        private CircuitState DecideNonLedCircuitState(
            List<string> plusNodes,
            List<string> minusNodes,
            Dictionary<string, List<string>> graph)
        {
            if (IsParallelCircuit(plusNodes, minusNodes, graph))
            {
                return CircuitState.Parallel;
            }

            if (IsSeriesCircuit(plusNodes, minusNodes, graph))
            {
                return CircuitState.Series;
            }

            return CircuitState.Short;
        }

        private bool HasPath(string startNodeId, string targetNodeId, Dictionary<string, List<string>> graph)
        {
            if (string.IsNullOrEmpty(startNodeId) ||
                string.IsNullOrEmpty(targetNodeId) ||
                graph == null)
            {
                return false;
            }

            if (startNodeId == targetNodeId)
            {
                return true;
            }

            if (!graph.ContainsKey(startNodeId))
            {
                return false;
            }

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(startNodeId);
            visited.Add(startNodeId);

            while (queue.Count > 0)
            {
                string currentNodeId = queue.Dequeue();

                if (currentNodeId == targetNodeId)
                {
                    return true;
                }

                if (!graph.TryGetValue(currentNodeId, out List<string> neighbors) || neighbors == null)
                {
                    continue;
                }

                for (int i = 0; i < neighbors.Count; i++)
                {
                    string nextNodeId = neighbors[i];
                    if (string.IsNullOrEmpty(nextNodeId) || visited.Contains(nextNodeId))
                    {
                        continue;
                    }

                    if (nextNodeId == targetNodeId)
                    {
                        return true;
                    }

                    visited.Add(nextNodeId);
                    queue.Enqueue(nextNodeId);
                }
            }

            return false;
        }

        private void DebugGraph(
            Dictionary<string, List<string>> graph,
            List<string> plusNodes,
            List<string> minusNodes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("[CircuitEvaluator Graph Debug]");
            builder.AppendLine($"Pins.Count={context?.Pins.Count ?? 0}");
            builder.AppendLine($"Sockets.Count={context?.Sockets.Count ?? 0}");
            builder.AppendLine($"Wires.Count={context?.Wires.Count ?? 0}");
            builder.AppendLine($"Resistors.Count={context?.Resistors.Count ?? 0}");

            builder.AppendLine();
            builder.AppendLine("[Wire Edge Attempts]");
            AppendWireEdgeAttempts(builder);

            builder.AppendLine();
            builder.AppendLine("[Resistor Edge Attempts]");
            AppendResistorEdgeAttempts(builder);

            builder.AppendLine();
            builder.AppendLine("[Important Node Neighbors]");
            AppendNeighbors(builder, graph, "BottomPlus_2");
            AppendNeighbors(builder, graph, "LowerRow_17");
            AppendNeighbors(builder, graph, "LowerRow_18");
            AppendNeighbors(builder, graph, "LowerRow_19");
            AppendNeighbors(builder, graph, "BottomMinus_2");

            builder.AppendLine();
            builder.AppendLine("[Rail Path Test]");
            builder.AppendLine($"HasPath(BottomPlus_2, BottomMinus_2)={HasPath("BottomPlus_2", "BottomMinus_2", graph)}");
            builder.AppendLine($"HasRailCircuitPath(plusNodes, minusNodes, graph)={HasRailCircuitPath(plusNodes, minusNodes, graph)}");

            Debug.Log(builder.ToString());
        }

        private void AppendWireEdgeAttempts(StringBuilder builder)
        {
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
                    builder.AppendLine("- wire=NULL | edgeAdded=false | reason=wire is null");
                    continue;
                }

                bool pinANodeValid = TryGetNodeIdFromPin(wire.PinAId, out string nodeA);
                bool pinBNodeValid = TryGetNodeIdFromPin(wire.PinBId, out string nodeB);

                bool edgeAdded = pinANodeValid && pinBNodeValid && nodeA != nodeB;
                string failureReason = edgeAdded ? "none" : GetEdgeFailureReason(pinANodeValid, pinBNodeValid, nodeA, nodeB);

                builder.AppendLine(
                    $"- wireId={wire.WireId} | pinAId={wire.PinAId} | pinANode={ValueOrNone(nodeA)} | " +
                    $"pinBId={wire.PinBId} | pinBNode={ValueOrNone(nodeB)} | edgeAdded={edgeAdded} | reason={failureReason}");
            }
        }

        private void AppendResistorEdgeAttempts(StringBuilder builder)
        {
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
                    builder.AppendLine("- resistor=NULL | edgeAdded=false | reason=resistor is null");
                    continue;
                }

                bool pinANodeValid = TryGetNodeIdFromPin(resistor.PinAId, out string nodeA);
                bool pinBNodeValid = TryGetNodeIdFromPin(resistor.PinBId, out string nodeB);

                bool edgeAdded = pinANodeValid && pinBNodeValid && nodeA != nodeB;
                string failureReason = edgeAdded ? "none" : GetEdgeFailureReason(pinANodeValid, pinBNodeValid, nodeA, nodeB);

                builder.AppendLine(
                    $"- resistorId={resistor.ResistorId} | pinAId={resistor.PinAId} | pinANode={ValueOrNone(nodeA)} | " +
                    $"pinBId={resistor.PinBId} | pinBNode={ValueOrNone(nodeB)} | edgeAdded={edgeAdded} | reason={failureReason}");
            }
        }

        private void AppendNeighbors(StringBuilder builder, Dictionary<string, List<string>> graph, string nodeId)
        {
            builder.Append($"- {nodeId} neighbors: ");

            if (graph == null || string.IsNullOrEmpty(nodeId) || !graph.TryGetValue(nodeId, out List<string> neighbors) || neighbors == null || neighbors.Count == 0)
            {
                builder.AppendLine("none");
                return;
            }

            builder.AppendLine(string.Join(", ", neighbors));
        }

        private string GetEdgeFailureReason(bool pinANodeValid, bool pinBNodeValid, string nodeA, string nodeB)
        {
            if (!pinANodeValid && !pinBNodeValid)
            {
                return "pinA and pinB node resolution failed";
            }

            if (!pinANodeValid)
            {
                return "pinA node resolution failed";
            }

            if (!pinBNodeValid)
            {
                return "pinB node resolution failed";
            }

            if (nodeA == nodeB)
            {
                return "pinA and pinB resolve to same node";
            }

            return "unknown";
        }

        private string ValueOrNone(string value)
        {
            return string.IsNullOrEmpty(value) ? "none" : value;
        }
    }
}
