using System.Text;
using VRCircuit.Data;
using VRCircuit.Evaluation;
using VRCircuit.Services;

namespace VRCircuit.Debugging
{
    public class CircuitDebugValidator
    {
        private readonly CircuitContext context;
        private readonly CircuitConnectionService connectionService;
        private readonly CircuitEvaluator evaluator;

        public CircuitDebugValidator(
            CircuitContext context,
            CircuitConnectionService connectionService,
            CircuitEvaluator evaluator)
        {
            this.context = context;
            this.connectionService = connectionService;
            this.evaluator = evaluator;
        }

        public string GetPinConnectionSummary(string pinId)
        {
            if (context == null)
            {
                return "CircuitContext가 null입니다.";
            }

            if (string.IsNullOrEmpty(pinId))
            {
                return "Pin ID가 null이거나 비어 있습니다.";
            }

            CircuitPin pin = context.GetPinById(pinId);
            if (pin == null)
            {
                return $"Pin을 찾을 수 없습니다: {pinId}";
            }

            string socketText = string.IsNullOrEmpty(pin.CurrentSocketId) ? "연결 안 됨" : pin.CurrentSocketId;
            return $"Pin[{pin.PinId}] 소유자={pin.OwnerType}/{pin.OwnerId}, 연결 소켓={socketText}";
        }

        public string GetSocketConnectionSummary(string socketId)
        {
            if (context == null)
            {
                return "CircuitContext가 null입니다.";
            }

            if (string.IsNullOrEmpty(socketId))
            {
                return "Socket ID가 null이거나 비어 있습니다.";
            }

            CircuitSocket socket = context.GetSocketById(socketId);
            if (socket == null)
            {
                return $"Socket을 찾을 수 없습니다: {socketId}";
            }

            string pinText = string.IsNullOrEmpty(socket.ConnectedPinId) ? "비어 있음" : socket.ConnectedPinId;
            return $"Socket[{socket.SocketId}] 타입={socket.SocketType}, Node={socket.NodeId}, 연결 Pin={pinText}";
        }

        public string GetCurrentStateSummary()
        {
            CircuitState state = EvaluateCurrentState();
            return $"현재 회로 상태={state}";
        }

        public string ValidateBasicState()
        {
            if (context == null)
            {
                return "검사 실패: CircuitContext가 null입니다.";
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("검사 요약");

            int pinMismatchCount = 0;
            for (int i = 0; i < context.Pins.Count; i++)
            {
                CircuitPin pin = context.Pins[i];
                if (pin == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(pin.CurrentSocketId))
                {
                    continue;
                }

                CircuitSocket socket = context.GetSocketById(pin.CurrentSocketId);
                if (socket == null || socket.ConnectedPinId != pin.PinId)
                {
                    pinMismatchCount++;
                    builder.AppendLine($"Pin 연결 불일치: {pin.PinId} -> {pin.CurrentSocketId}");
                }
            }

            int socketMismatchCount = 0;
            for (int i = 0; i < context.Sockets.Count; i++)
            {
                CircuitSocket socket = context.Sockets[i];
                if (socket == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(socket.ConnectedPinId))
                {
                    continue;
                }

                CircuitPin pin = context.GetPinById(socket.ConnectedPinId);
                if (pin == null || pin.CurrentSocketId != socket.SocketId)
                {
                    socketMismatchCount++;
                    builder.AppendLine($"Socket 연결 불일치: {socket.SocketId} -> {socket.ConnectedPinId}");
                }
            }

            if (pinMismatchCount == 0 && socketMismatchCount == 0)
            {
                builder.AppendLine("Pin-Socket 연결 상태가 일관됩니다.");
            }

            builder.AppendLine(GetCurrentStateSummary());
            return builder.ToString().TrimEnd();
        }

        public string GetAllPinsSummary()
        {
            if (context == null)
            {
                return "CircuitContext가 null입니다.";
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("전체 Pin 상태");

            for (int i = 0; i < context.Pins.Count; i++)
            {
                CircuitPin pin = context.Pins[i];
                if (pin == null)
                {
                    continue;
                }

                builder.AppendLine(GetPinConnectionSummary(pin.PinId));
            }

            return builder.ToString().TrimEnd();
        }

        public string GetAllSocketsSummary()
        {
            if (context == null)
            {
                return "CircuitContext가 null입니다.";
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("전체 Socket 상태");

            for (int i = 0; i < context.Sockets.Count; i++)
            {
                CircuitSocket socket = context.Sockets[i];
                if (socket == null)
                {
                    continue;
                }

                builder.AppendLine(GetSocketConnectionSummary(socket.SocketId));
            }

            return builder.ToString().TrimEnd();
        }

        public string GetFullSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(GetCurrentStateSummary());
            builder.AppendLine();
            builder.AppendLine(GetAllPinsSummary());
            builder.AppendLine();
            builder.AppendLine(GetAllSocketsSummary());
            builder.AppendLine();
            builder.AppendLine(ValidateBasicState());
            return builder.ToString().TrimEnd();
        }

        private CircuitState EvaluateCurrentState()
        {
            if (connectionService != null)
            {
                return connectionService.CurrentState;
            }

            if (evaluator != null)
            {
                return evaluator.Evaluate();
            }

            return CircuitState.Open;
        }
    }
}
