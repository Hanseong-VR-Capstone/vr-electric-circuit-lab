using System.Collections.Generic;
using VRCircuit.Data;

namespace VRCircuit.Evaluation
{
    public class CircuitEvaluator
    {
        private readonly CircuitContext context;

        public CircuitEvaluator(CircuitContext context)
        {
            this.context = context;
        }

        public CircuitState Evaluate()
        {
            if (!TryGetBatteryTerminalNodeIds(out string plusNodeId, out string minusNodeId))
            {
                return CircuitState.Open;
            }

            return EvaluateStateFromResolvedBattery(plusNodeId, minusNodeId);
        }

        // Returns the first battery voltage as the educational supply voltage.
        public float GetPrimaryBatteryVoltage()
        {
            CircuitBattery battery = GetPrimaryBattery();
            if (battery == null)
            {
                return 0f;
            }

            return battery.Voltage;
        }

        // Uses LED count as the current simplified load unit.
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

        // Educational simplification: divide supply voltage equally across loads.
        public float CalculateSeriesVoltagePerLoad()
        {
            float batteryVoltage = GetPrimaryBatteryVoltage();
            int loadCount = CountActiveLoads();

            if (batteryVoltage <= 0f || loadCount <= 0)
            {
                return 0f;
            }

            return batteryVoltage / loadCount;
        }

        // Educational simplification: each parallel branch gets full battery voltage.
        public float CalculateParallelBranchVoltage()
        {
            return GetPrimaryBatteryVoltage();
        }

        // Resolves the current battery terminal nodes from pin -> socket -> nodeId.
        private bool TryGetBatteryTerminalNodeIds(out string plusNodeId, out string minusNodeId)
        {
            plusNodeId = null;
            minusNodeId = null;

            if (context == null)
            {
                return false;
            }

            CircuitBattery battery = GetPrimaryBattery();
            if (battery == null)
            {
                return false;
            }

            return TryGetNodeIdFromPin(battery.PositivePinId, out plusNodeId) &&
                   TryGetNodeIdFromPin(battery.NegativePinId, out minusNodeId);
        }

        // Keeps final state decision flow explicit and ready for later extension.
        private CircuitState EvaluateStateFromResolvedBattery(string plusNodeId, string minusNodeId)
        {
            Dictionary<string, List<string>> graph = BuildNodeGraph();

            if (!HasResolvedCircuitPath(plusNodeId, minusNodeId, graph))
            {
                return CircuitState.Open;
            }

            if (IsValidLedPath(graph, plusNodeId, minusNodeId))
            {
                return CircuitState.LedOn;
            }

            return DecideNonLedCircuitState(plusNodeId, minusNodeId, graph);
        }

        // Kept intentionally to make the decision flow in Evaluate() explicit.
        private bool HasResolvedCircuitPath(string plusNodeId, string minusNodeId, Dictionary<string, List<string>> graph)
        {
            return HasCircuitPath(plusNodeId, minusNodeId, graph);
        }

        // Resolves pin -> connected socket -> nodeId.
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

        // Builds a bidirectional graph from wires and active switches only.
        private Dictionary<string, List<string>> BuildNodeGraph()
        {
            Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

            AddWireEdges(graph);
            AddSwitchEdges(graph);

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

        private bool HasCircuitPath(string batteryPlusNodeId, string batteryMinusNodeId, Dictionary<string, List<string>> graph)
        {
            return HasPath(batteryPlusNodeId, batteryMinusNodeId, graph);
        }

        private bool IsValidLedPath(Dictionary<string, List<string>> graph, string batteryPlusNodeId, string batteryMinusNodeId)
        {
            if (context == null)
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

                bool plusToAnode = CanReach(batteryPlusNodeId, anodeNodeId, graph);
                bool cathodeToMinus = CanReach(cathodeNodeId, batteryMinusNodeId, graph);

                if (plusToAnode && cathodeToMinus)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanReach(string startNodeId, string targetNodeId, Dictionary<string, List<string>> graph)
        {
            if (string.IsNullOrEmpty(startNodeId) || string.IsNullOrEmpty(targetNodeId))
            {
                return false;
            }

            if (startNodeId == targetNodeId)
            {
                return true;
            }

            return HasPath(startNodeId, targetNodeId, graph);
        }

        // Excludes battery endpoints from branching judgment for clearer series/parallel checks.
        private bool HasBranching(Dictionary<string, List<string>> graph, string startNodeId, string endNodeId)
        {
            if (graph == null)
            {
                return false;
            }

            foreach (KeyValuePair<string, List<string>> pair in graph)
            {
                string nodeId = pair.Key;
                if (nodeId == startNodeId || nodeId == endNodeId)
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

        // Counts distinct simple paths with early exit for lightweight educational checks.
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

        private bool IsSeriesCircuit(string startNode, string endNode, Dictionary<string, List<string>> graph)
        {
            if (!HasPath(startNode, endNode, graph))
            {
                return false;
            }

            return !HasBranching(graph, startNode, endNode);
        }

        private bool IsParallelCircuit(string startNode, string endNode, Dictionary<string, List<string>> graph)
        {
            if (!HasPath(startNode, endNode, graph))
            {
                return false;
            }

            bool hasBranching = HasBranching(graph, startNode, endNode);
            if (hasBranching)
            {
                return true;
            }

            return CountPaths(startNode, endNode, graph, 2) >= 2;
        }

        // Parallel is checked before Series. Short remains the fallback state.
        private CircuitState DecideNonLedCircuitState(
            string plusNodeId,
            string minusNodeId,
            Dictionary<string, List<string>> graph)
        {
            if (IsParallelCircuit(plusNodeId, minusNodeId, graph))
            {
                return CircuitState.Parallel;
            }

            if (IsSeriesCircuit(plusNodeId, minusNodeId, graph))
            {
                return CircuitState.Series;
            }

            return CircuitState.Short;
        }

        // Uses BFS for safe reachability checks.
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

        private CircuitBattery GetPrimaryBattery()
        {
            if (context == null || context.Batteries.Count == 0)
            {
                return null;
            }

            return context.Batteries[0];
        }
    }
}
