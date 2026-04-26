using UnityEngine;
using VRCircuit.Data;
using VRCircuit.Services;

namespace VRCircuit.Runtime
{
    public class CircuitRuntimeRoot : MonoBehaviour
    {
        private CircuitContext context;
        private CircuitConnectionService connectionService;

        public CircuitContext Context => context;
        public CircuitConnectionService ConnectionService => connectionService;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (context == null)
            {
                context = new CircuitContext();
            }

            if (connectionService == null)
            {
                connectionService = new CircuitConnectionService(context);
            }
        }
    }
}
