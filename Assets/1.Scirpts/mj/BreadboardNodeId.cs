namespace VRCircuit.Data
{
    public static class BreadboardNodeId
    {
        // Same nodeId means sockets belong to the same electrical node.
        public static string GetRowNodeId(int rowIndex, bool isLeft)
        {
            string side = isLeft ? "L" : "R";
            return $"Row{rowIndex}_{side}";
        }

        public static string GetRailPlusNodeId()
        {
            return "RailPlus";
        }

        public static string GetRailMinusNodeId()
        {
            return "RailMinus";
        }
    }
}
