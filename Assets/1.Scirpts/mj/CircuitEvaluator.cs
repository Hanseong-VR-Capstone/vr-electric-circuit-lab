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
            if (context == null)
            {
                return CircuitState.Open;
            }

            CircuitBattery battery = GetPrimaryBattery();
            if (battery == null)
            {
                return CircuitState.Open;
            }

            if (!TryGetNodeIdFromPin(battery.PositivePinId, out string batteryPlusNode) ||
                !TryGetNodeIdFromPin(battery.NegativePinId, out string batteryMinusNode))
            {
                return CircuitState.Open;
            }

            Dictionary<string, HashSet<string>> graph = BuildNodeGraph();

            if (!HasPath(graph, batteryPlusNode, batteryMinusNode))
            {
                return CircuitState.Open;
            }

            if (IsLedOn(graph, batteryPlusNode, batteryMinusNode))
            {
                return CircuitState.LedOn;
            }

            return CircuitState.Short;
        }

        private bool TryGetNodeIdFromPin(string pinId, out string nodeId)
        {
            nodeId = null;

            if (string.IsNullOrEmpty(pinId) || context == null)
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

        private Dictionary<string, HashSet<string>> BuildNodeGraph()
        {
            Dictionary<string, HashSet<string>> graph = new Dictionary<string, HashSet<string>>();

            AddWireEdges(graph);
            AddSwitchEdges(graph);
            AddLedEdges(graph);

            return graph;
        }
        private void AddLedEdges(Dictionary<string, HashSet<string>> graph)
        {
            for (int i = 0; i < context.Leds.Count; i++)
            {
                CircuitLed led = context.Leds[i];
                if (led == null)
                {
                    continue;
                }

                if (!TryGetNodeIdFromPin(led.AnodePinId, out string nodeA) ||
                    !TryGetNodeIdFromPin(led.CathodePinId, out string nodeB))
                {
                    continue;
                }

                AddUndirectedEdge(graph, nodeA, nodeB);
            }
        }

        private void AddWireEdges(Dictionary<string, HashSet<string>> graph)
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

                AddUndirectedEdge(graph, nodeA, nodeB);
            }
        }

        private void AddSwitchEdges(Dictionary<string, HashSet<string>> graph)
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

                AddUndirectedEdge(graph, nodeA, nodeB);
            }
        }

        private void AddUndirectedEdge(Dictionary<string, HashSet<string>> graph, string nodeA, string nodeB)
        {
            if (string.IsNullOrEmpty(nodeA) || string.IsNullOrEmpty(nodeB))
            {
                return;
            }

            AddNode(graph, nodeA);
            AddNode(graph, nodeB);

            graph[nodeA].Add(nodeB);
            graph[nodeB].Add(nodeA);
        }

        private void AddNode(Dictionary<string, HashSet<string>> graph, string nodeId)
        {
            if (!graph.ContainsKey(nodeId))
            {
                graph[nodeId] = new HashSet<string>();
            }
        }

        private bool HasPath(Dictionary<string, HashSet<string>> graph, string startNode, string targetNode)
        {
            if (string.IsNullOrEmpty(startNode) || string.IsNullOrEmpty(targetNode))
            {
                return false;
            }

            if (startNode == targetNode)
            {
                return true;
            }

            if (!graph.ContainsKey(startNode) || !graph.ContainsKey(targetNode))
            {
                return false;
            }

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                string currentNode = queue.Dequeue();
                HashSet<string> neighbors = graph[currentNode];

                foreach (string neighbor in neighbors)
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    if (neighbor == targetNode)
                    {
                        return true;
                    }

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            return false;
        }

        private bool IsLedOn(Dictionary<string, HashSet<string>> graph, string batteryPlusNode, string batteryMinusNode)
        {
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

                bool plusToAnode = batteryPlusNode == anodeNode || HasPath(graph, batteryPlusNode, anodeNode);
                bool cathodeToMinus = cathodeNode == batteryMinusNode || HasPath(graph, cathodeNode, batteryMinusNode);

                if (plusToAnode && cathodeToMinus)
                {
                    return true;
                }
            }

            return false;
        }

        private CircuitBattery GetPrimaryBattery()
        {
            if (context.Batteries.Count == 0)
            {
                return null;
            }

            return context.Batteries[0];
        }
    }
}
