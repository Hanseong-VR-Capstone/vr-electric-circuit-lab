using UnityEngine;
using VRCircuit.Data;
using VRCircuit.Services;
using VRCircuit.Runtime;

namespace VRCircuit.Board
{
    public class BreadboardSocketBootstrap : MonoBehaviour
    {
        [SerializeField] private CircuitRuntimeRoot runtimeRoot;

        private CircuitContext context;
        private CircuitConnectionService connectionService;
        private bool isInitialized;

        public CircuitContext Context => context;
        public CircuitConnectionService ConnectionService => connectionService;
        public bool IsInitialized => isInitialized;

        private void Start()
        {
            InitializeBootstrap();
        }

        private void InitializeBootstrap()
        {
            if (runtimeRoot == null)
            {
                Debug.LogWarning("BreadboardSocketBootstrap: CircuitRuntimeRoot is not assigned.");
                return;
            }

            runtimeRoot.EnsureInitialized();

            context = runtimeRoot.Context;
            connectionService = runtimeRoot.ConnectionService;

            if (context == null || connectionService == null)
            {
                Debug.LogWarning("BreadboardSocketBootstrap: Context or Service is still null after EnsureInitialized.");
                return;
            }

            RegisterBreadboardSockets();

            Debug.Log($"[Bootstrap] Socket registration complete. Count={context.Sockets.Count}");

            isInitialized = true;
        }

        private void RegisterBreadboardSockets()
        {
            for (int holeIndex = BreadboardNodeMapper.MinHoleIndex; holeIndex <= BreadboardNodeMapper.MaxHoleIndex; holeIndex++)
            {
                if (!BreadboardNodeMapper.TryResolveHole(holeIndex, out string socketId, out string nodeId, out SocketType socketType))
                {
                    Debug.LogWarning($"BreadboardSocketBootstrap: Failed to resolve hole mapping. holeIndex={holeIndex}");
                    continue;
                }

                if (context.GetSocketById(socketId) != null)
                {
                    continue;
                }

                CircuitSocket socket = new CircuitSocket(socketId, socketType, nodeId);
                context.AddSocket(socket);
            }
        }
    }
}
