using System.Collections.Generic;
using VRCircuit.Data;

namespace VRCircuit.Analysis
{
    public class CurrentFlowWireDirection
    {
        private readonly string wireId;
        private readonly string fromNodeId;
        private readonly string toNodeId;
        private readonly string fromPinId;
        private readonly string toPinId;
        private readonly string fromSocketId;
        private readonly string toSocketId;

        public string WireId => wireId;
        public string FromNodeId => fromNodeId;
        public string ToNodeId => toNodeId;
        public string FromPinId => fromPinId;
        public string ToPinId => toPinId;
        public string FromSocketId => fromSocketId;
        public string ToSocketId => toSocketId;

        public CurrentFlowWireDirection(
            string wireId,
            string fromNodeId,
            string toNodeId,
            string fromPinId,
            string toPinId,
            string fromSocketId,
            string toSocketId)
        {
            this.wireId = wireId;
            this.fromNodeId = fromNodeId;
            this.toNodeId = toNodeId;
            this.fromPinId = fromPinId;
            this.toPinId = toPinId;
            this.fromSocketId = fromSocketId;
            this.toSocketId = toSocketId;
        }
    }

    public class CircuitCurrentFlowResult
    {
        private readonly bool isFlowing;
        private readonly List<string> activeWireIds;
        private readonly List<CurrentFlowWireDirection> wireDirections;

        public bool IsFlowing => isFlowing;
        public IReadOnlyList<string> ActiveWireIds => activeWireIds;
        public IReadOnlyList<CurrentFlowWireDirection> WireDirections => wireDirections;

        public CircuitCurrentFlowResult(bool isFlowing, List<string> activeWireIds)
            : this(isFlowing, activeWireIds, new List<CurrentFlowWireDirection>())
        {
        }

        public CircuitCurrentFlowResult(
            bool isFlowing,
            List<string> activeWireIds,
            List<CurrentFlowWireDirection> wireDirections)
        {
            this.isFlowing = isFlowing;
            this.activeWireIds = activeWireIds ?? new List<string>();
            this.wireDirections = wireDirections ?? new List<CurrentFlowWireDirection>();
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
                return new CircuitCurrentFlowResult(
                    false,
                    new List<string>(),
                    new List<CurrentFlowWireDirection>());
            }

            Dictionary<string, List<string>> graph = BuildNodeGraph();
            List<string> plusNodes = GetPowerPlusNodes();
            List<string> minusNodes = GetPowerMinusNodes();

            if (!IsLedPolarityFlowAllowed(plusNodes, minusNodes))
            {
                return new CircuitCurrentFlowResult(
                    false,
                    new List<string>(),
                    new List<CurrentFlowWireDirection>());
            }

            List<List<string>> nodePaths = FindAllRailPaths(plusNodes, minusNodes, graph);
            if (nodePaths.Count == 0)
            {
                return new CircuitCurrentFlowResult(
                    false,
                    new List<string>(),
                    new List<CurrentFlowWireDirection>());
            }

            List<string> activeWireIds = GetWireIdsFromNodePaths(nodePaths);
            List<CurrentFlowWireDirection> wireDirections = GetWireDirectionsFromNodePaths(nodePaths);

            return new CircuitCurrentFlowResult(true, activeWireIds, wireDirections);
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
            AddResistorEdges(graph);
            AddLedEdges(graph);

            return graph;
        }

        private Dictionary<string, List<string>> BuildPolarityValidationGraph()
        {
            Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

            AddWireEdges(graph);
            AddSwitchEdges(graph);
            AddResistorEdges(graph);

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
                if (circuitSwitch == null)
                {
                    continue;
                }

                IReadOnlyList<CircuitSwitchContactPair> contactPairs = circuitSwitch.GetActiveConnectedPairs();
                if (contactPairs == null)
                {
                    continue;
                }

                for (int j = 0; j < contactPairs.Count; j++)
                {
                    CircuitSwitchContactPair pair = contactPairs[j];
                    if (pair == null)
                    {
                        continue;
                    }

                    if (!TryGetNodeIdFromPin(pair.PinAId, out string nodeA) ||
                        !TryGetNodeIdFromPin(pair.PinBId, out string nodeB))
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
        }

        private void AddResistorEdges(Dictionary<string, List<string>> graph)
        {
            if (context.Resistors == null)
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

        private void AddLedEdges(Dictionary<string, List<string>> graph)
        {
            if (context == null || context.Leds == null)
            {
                return;
            }

            for (int i = 0; i < context.Leds.Count; i++)
            {
                CircuitLed led = context.Leds[i];
                if (led == null)
                {
                    continue;
                }

                if (!TryGetNodeIdFromPin(led.AnodePinId, out string anodeNode) ||
                    !TryGetNodeIdFromPin(led.CathodePinId, out string cathodeNode))
                {
                    continue;
                }

                if (anodeNode == cathodeNode)
                {
                    continue;
                }

                AddBidirectionalEdge(graph, anodeNode, cathodeNode);
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

        private bool TryGetPinSocketNode(
            string pinId,
            out string socketId,
            out string nodeId)
        {
            socketId = null;
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

            socketId = pin.CurrentSocketId;
            nodeId = socket.NodeId;
            return true;
        }

        private bool IsLedPolarityFlowAllowed(List<string> plusNodes, List<string> minusNodes)
        {
            if (context == null || context.Leds == null || context.Leds.Count == 0)
            {
                return true;
            }

            Dictionary<string, List<string>> polarityGraph = BuildPolarityValidationGraph();

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

                bool hasReversedPolarity =
                    CanAnyReach(plusNodes, cathodeNodeId, polarityGraph) &&
                    CanReachAny(anodeNodeId, minusNodes, polarityGraph);

                if (hasReversedPolarity)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CanAnyReach(
            List<string> startNodes,
            string targetNodeId,
            Dictionary<string, List<string>> graph)
        {
            if (startNodes == null || string.IsNullOrEmpty(targetNodeId) || graph == null)
            {
                return false;
            }

            for (int i = 0; i < startNodes.Count; i++)
            {
                if (TryFindPath(startNodes[i], targetNodeId, graph, out _))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanReachAny(
            string startNodeId,
            List<string> targetNodes,
            Dictionary<string, List<string>> graph)
        {
            if (string.IsNullOrEmpty(startNodeId) || targetNodes == null || graph == null)
            {
                return false;
            }

            for (int i = 0; i < targetNodes.Count; i++)
            {
                if (TryFindPath(startNodeId, targetNodes[i], graph, out _))
                {
                    return true;
                }
            }

            return false;
        }

        private List<List<string>> FindAllRailPaths(
            List<string> plusNodes,
            List<string> minusNodes,
            Dictionary<string, List<string>> graph)
        {
            List<List<string>> paths = new List<List<string>>();

            if (plusNodes == null || minusNodes == null || graph == null)
            {
                return paths;
            }

            HashSet<string> minusNodeSet = new HashSet<string>(minusNodes);
            HashSet<string> pathKeys = new HashSet<string>();

            for (int i = 0; i < plusNodes.Count; i++)
            {
                string startNode = plusNodes[i];
                if (string.IsNullOrEmpty(startNode) || !graph.ContainsKey(startNode))
                {
                    continue;
                }

                List<string> currentPath = new List<string>();
                HashSet<string> visited = new HashSet<string>();
                CollectRailPathsDepthFirst(startNode, minusNodeSet, graph, visited, currentPath, paths, pathKeys);
            }

            return paths;
        }

        private void CollectRailPathsDepthFirst(
            string currentNode,
            HashSet<string> minusNodeSet,
            Dictionary<string, List<string>> graph,
            HashSet<string> visited,
            List<string> currentPath,
            List<List<string>> paths,
            HashSet<string> pathKeys)
        {
            if (string.IsNullOrEmpty(currentNode) ||
                minusNodeSet == null ||
                graph == null ||
                visited == null ||
                currentPath == null ||
                paths == null ||
                pathKeys == null)
            {
                return;
            }

            visited.Add(currentNode);
            currentPath.Add(currentNode);

            if (minusNodeSet.Contains(currentNode))
            {
                string pathKey = string.Join("|", currentPath);
                if (!pathKeys.Contains(pathKey))
                {
                    paths.Add(new List<string>(currentPath));
                    pathKeys.Add(pathKey);
                }

                currentPath.RemoveAt(currentPath.Count - 1);
                visited.Remove(currentNode);
                return;
            }

            if (graph.TryGetValue(currentNode, out List<string> neighbors) && neighbors != null)
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    string nextNode = neighbors[i];
                    if (string.IsNullOrEmpty(nextNode) || visited.Contains(nextNode))
                    {
                        continue;
                    }

                    CollectRailPathsDepthFirst(nextNode, minusNodeSet, graph, visited, currentPath, paths, pathKeys);
                }
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            visited.Remove(currentNode);
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

        private List<string> GetWireIdsFromNodePaths(List<List<string>> nodePaths)
        {
            List<string> wireIds = new List<string>();

            if (nodePaths == null || nodePaths.Count == 0)
            {
                return wireIds;
            }

            for (int i = 0; i < nodePaths.Count; i++)
            {
                List<string> pathWireIds = GetWireIdsFromNodePath(nodePaths[i]);
                for (int j = 0; j < pathWireIds.Count; j++)
                {
                    string wireId = pathWireIds[j];
                    if (!string.IsNullOrEmpty(wireId) && !wireIds.Contains(wireId))
                    {
                        wireIds.Add(wireId);
                    }
                }
            }

            return wireIds;
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

        private List<CurrentFlowWireDirection> GetWireDirectionsFromNodePaths(List<List<string>> nodePaths)
        {
            List<CurrentFlowWireDirection> wireDirections = new List<CurrentFlowWireDirection>();
            HashSet<string> directionKeys = new HashSet<string>();

            if (nodePaths == null || nodePaths.Count == 0)
            {
                return wireDirections;
            }

            for (int i = 0; i < nodePaths.Count; i++)
            {
                List<CurrentFlowWireDirection> pathDirections = GetWireDirectionsFromNodePath(nodePaths[i]);
                for (int j = 0; j < pathDirections.Count; j++)
                {
                    CurrentFlowWireDirection direction = pathDirections[j];
                    if (direction == null)
                    {
                        continue;
                    }

                    string directionKey = $"{direction.WireId}|{direction.FromNodeId}|{direction.ToNodeId}";
                    if (directionKeys.Contains(directionKey))
                    {
                        continue;
                    }

                    wireDirections.Add(direction);
                    directionKeys.Add(directionKey);
                }
            }

            return wireDirections;
        }
        private List<CurrentFlowWireDirection> GetWireDirectionsFromNodePath(List<string> nodePath)
        {
            List<CurrentFlowWireDirection> wireDirections = new List<CurrentFlowWireDirection>();

            if (nodePath == null || nodePath.Count < 2)
            {
                return wireDirections;
            }

            for (int i = 0; i < nodePath.Count - 1; i++)
            {
                string fromNodeId = nodePath[i];
                string toNodeId = nodePath[i + 1];

                if (TryGetWireDirectionBetweenNodes(fromNodeId, toNodeId, out CurrentFlowWireDirection wireDirection))
                {
                    wireDirections.Add(wireDirection);
                }
            }

            return wireDirections;
        }

        private bool TryGetWireDirectionBetweenNodes(
            string fromNodeId,
            string toNodeId,
            out CurrentFlowWireDirection wireDirection)
        {
            wireDirection = null;

            if (string.IsNullOrEmpty(fromNodeId) || string.IsNullOrEmpty(toNodeId))
            {
                return false;
            }

            for (int i = 0; i < context.Wires.Count; i++)
            {
                CircuitWire wire = context.Wires[i];
                if (wire == null)
                {
                    continue;
                }

                if (!TryGetPinSocketNode(wire.PinAId, out string socketAId, out string wireNodeA) ||
                    !TryGetPinSocketNode(wire.PinBId, out string socketBId, out string wireNodeB))
                {
                    continue;
                }

                if (wireNodeA == fromNodeId && wireNodeB == toNodeId)
                {
                    wireDirection = new CurrentFlowWireDirection(
                        wire.WireId,
                        fromNodeId,
                        toNodeId,
                        wire.PinAId,
                        wire.PinBId,
                        socketAId,
                        socketBId);

                    return true;
                }

                if (wireNodeA == toNodeId && wireNodeB == fromNodeId)
                {
                    wireDirection = new CurrentFlowWireDirection(
                        wire.WireId,
                        fromNodeId,
                        toNodeId,
                        wire.PinBId,
                        wire.PinAId,
                        socketBId,
                        socketAId);

                    return true;
                }
            }

            return false;
        }
    }
}






