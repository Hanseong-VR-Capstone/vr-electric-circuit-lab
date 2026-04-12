using VRCircuit.Data;

namespace VRCircuit.Board
{
    public static class BreadboardNodeMapper
    {
        public const int MinHoleIndex = 0;
        public const int MaxHoleIndex = 459;

        public static bool TryResolveHole(int holeIndex, out string socketId, out string nodeId, out SocketType socketType)
        {
            socketId = null;
            nodeId = null;
            socketType = SocketType.BreadboardRow;

            if (!IsValidHoleIndex(holeIndex))
            {
                return false;
            }

            socketId = GetSocketId(holeIndex);
            socketType = GetSocketType(holeIndex);

            return TryGetNodeId(holeIndex, out nodeId);
        }

        public static string GetSocketId(int holeIndex)
        {
            return $"BB_H{holeIndex}";
        }

        public static bool TryGetNodeId(int holeIndex, out string nodeId)
        {
            nodeId = null;

            if (!IsValidHoleIndex(holeIndex))
            {
                return false;
            }

            if (holeIndex >= 0 && holeIndex <= 24)
            {
                nodeId = $"TopMinus_{holeIndex / 5}";
                return true;
            }

            if (holeIndex >= 25 && holeIndex <= 49)
            {
                nodeId = $"TopPlus_{(holeIndex - 25) / 5}";
                return true;
            }

            if (holeIndex >= 50 && holeIndex <= 229)
            {
                nodeId = $"UpperRow_{(holeIndex - 50) / 5}";
                return true;
            }

            if (holeIndex >= 230 && holeIndex <= 409)
            {
                nodeId = $"LowerRow_{(holeIndex - 230) / 5}";
                return true;
            }

            if (holeIndex >= 410 && holeIndex <= 434)
            {
                nodeId = $"BottomMinus_{(holeIndex - 410) / 5}";
                return true;
            }

            if (holeIndex >= 435 && holeIndex <= 459)
            {
                nodeId = $"BottomPlus_{(holeIndex - 435) / 5}";
                return true;
            }

            return false;
        }

        public static SocketType GetSocketType(int holeIndex)
        {
            if (holeIndex >= 25 && holeIndex <= 49)
            {
                return SocketType.PowerRailPlus;
            }

            if (holeIndex >= 435 && holeIndex <= 459)
            {
                return SocketType.PowerRailPlus;
            }

            if ((holeIndex >= 0 && holeIndex <= 24) ||
                (holeIndex >= 410 && holeIndex <= 434))
            {
                return SocketType.PowerRailMinus;
            }

            return SocketType.BreadboardRow;
        }

        public static bool IsValidHoleIndex(int holeIndex)
        {
            return holeIndex >= MinHoleIndex && holeIndex <= MaxHoleIndex;
        }
    }
}
