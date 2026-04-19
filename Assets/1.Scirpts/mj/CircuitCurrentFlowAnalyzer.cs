using System.Collections.Generic;
using VRCircuit.Data;

namespace VRCircuit.Analysis
{
    public class CircuitCurrentFlowResult
    {
        private readonly bool isFlowing;
        private readonly List<string> activeWireIds;

        public bool IsFlowing => isFlowing;
        public IReadOnlyList<string> ActiveWireIds => activeWireIds;

        public CircuitCurrentFlowResult(bool isFlowing, List<string> activeWireIds)
        {
            this.isFlowing = isFlowing;
            this.activeWireIds = activeWireIds ?? new List<string>();
        }
    }

    public class CircuitCurrentFlowAnalyzer
    {
        private readonly CircuitContext context;

        public CircuitCurrentFlowAnalyzer(CircuitContext context)
        {
            this.context = context;
        }

        public CircuitCurrentFlowResult Analyze()
        {
            if (context == null)
            {
                return new CircuitCurrentFlowResult(false, new List<string>());
            }

            Dictionary<string, List<string>> graph = BuildNodeGraph();
            List<string> plusNodes = GetPowerPlusNodes();
            List<string> minusNodes = GetPowerMinusNodes();

            if (!TryFindRailPath(plusNodes, minusNodes, graph, out List<string> nodePath))
            {
                return new CircuitCurrentFlowResult(false, new List<string>());
            }

            List<string> activeWireIds = GetWireIdsFromNodePath(nodePath);
            return new CircuitCurrentFlowResult(activeWireIds.Count > 0, activeWireIds);
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

        private Dictionary<string, List<string>> BuildNodeGraph()
        {
            Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

            AddWireEdges(graph);
            AddSwitchEdges(graph);

            return graph;
        }

        private void AddWireEdges(Dictionary<string, List<string>> graph)
        {
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

        private bool TryGetNodeIdFromPin(string pinId, out string nodeId)
        {
            nodeId = null;

            if (string.IsNullOrEmpty(pinId))
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

        private bool TryFindRailPath(
            List<string> plusNodes,
            List<string> minusNodes,
            Dictionary<string, List<string>> graph,
            out List<string> nodePath)
        {
            nodePath = null;

            if (plusNodes == null || minusNodes == null || graph == null)
            {
                return false;
            }

            for (int i = 0; i < plusNodes.Count; i++)
            {
                for (int j = 0; j < minusNodes.Count; j++)
                {
                    if (TryFindPath(plusNodes[i], minusNodes[j], graph, out nodePath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryFindPath(
            string startNode,
            string targetNode,
            Dictionary<string, List<string>> graph,
            out List<string> nodePath)
        {
            nodePath = null;

            if (string.IsNullOrEmpty(startNode) ||
                string.IsNullOrEmpty(targetNode) ||
                graph == null)
            {
                return false;
            }

            if (startNode == targetNode)
            {
                nodePath = new List<string> { startNode };
                return true;
            }

            if (!graph.ContainsKey(startNode))
            {
                return false;
            }

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();
            Dictionary<string, string> previousNodeByNode = new Dictionary<string, string>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                string currentNode = queue.Dequeue();

                if (!graph.TryGetValue(currentNode, out List<string> neighbors) || neighbors == null)
                {
                    continue;
                }

                for (int i = 0; i < neighbors.Count; i++)
                {
                    string nextNode = neighbors[i];
                    if (string.IsNullOrEmpty(nextNode) || visited.Contains(nextNode))
                    {
                        continue;
                    }

                    visited.Add(nextNode);
                    previousNodeByNode[nextNode] = currentNode;

                    if (nextNode == targetNode)
                    {
                        nodePath = ReconstructPath(startNode, targetNode, previousNodeByNode);
                        return nodePath.Count > 0;
                    }

                    queue.Enqueue(nextNode);
                }
            }

            return false;
        }

        private List<string> ReconstructPath(
            string startNode,
            string targetNode,
            Dictionary<string, string> previousNodeByNode)
        {
            List<string> path = new List<string>();
            string currentNode = targetNode;

            path.Add(currentNode);

            while (currentNode != startNode)
            {
                if (!previousNodeByNode.TryGetValue(currentNode, out string previousNode))
                {
                    return new List<string>();
                }

                currentNode = previousNode;
                path.Add(currentNode);
            }

            path.Reverse();
            return path;
        }

        private List<string> GetWireIdsFromNodePath(List<string> nodePath)
        {
            List<string> wireIds = new List<string>();

            if (nodePath == null || nodePath.Count < 2)
            {
                return wireIds;
            }

            for (int i = 0; i < nodePath.Count - 1; i++)
            {
                string nodeA = nodePath[i];
                string nodeB = nodePath[i + 1];

                AddWireIdsBetweenNodes(nodeA, nodeB, wireIds);
            }

            return wireIds;
        }

        private void AddWireIdsBetweenNodes(string nodeA, string nodeB, List<string> wireIds)
        {
            if (string.IsNullOrEmpty(nodeA) || string.IsNullOrEmpty(nodeB) || wireIds == null)
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

                if (!TryGetNodeIdFromPin(wire.PinAId, out string wireNodeA) ||
                    !TryGetNodeIdFromPin(wire.PinBId, out string wireNodeB))
                {
                    continue;
                }

                bool sameDirection = wireNodeA == nodeA && wireNodeB == nodeB;
                bool reverseDirection = wireNodeA == nodeB && wireNodeB == nodeA;

                if ((sameDirection || reverseDirection) && !wireIds.Contains(wire.WireId))
                {
                    wireIds.Add(wire.WireId);
                }
            }
        }
    }
}
