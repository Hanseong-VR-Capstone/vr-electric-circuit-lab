using System.Collections.Generic;
using VRCircuit.Data;
using VRCircuit.Evaluation;

namespace VRCircuit.Calculation
{
    public class CircuitCalculationHelper
    {
        private readonly CircuitContext context;
        private readonly CircuitEvaluator evaluator;

        public CircuitCalculationHelper(CircuitContext context)
        {
            this.context = context;
            evaluator = new CircuitEvaluator(context);
        }

        public CircuitCalculationResult Calculate()
        {
            if (context == null)
            {
                return CreateInvalidResult();
            }

            CircuitState state = evaluator.Evaluate();
            List<CircuitResistor> activeResistors = GetActiveResistors();

            if (activeResistors.Count == 0)
            {
                return CreateInvalidResult();
            }

            if (state != CircuitState.Series &&
                state != CircuitState.Parallel &&
                state != CircuitState.LedOn)
            {
                return CreateInvalidResult();
            }

            CircuitState calculationMode = GetCalculationMode(state);
            float totalVoltage = evaluator.GetRailSupplyVoltage();
            float totalResistance = CalculateTotalResistance(calculationMode, activeResistors);
            float totalCurrent = CalculateTotalCurrent(totalVoltage, totalResistance);

            if (totalVoltage <= 0f || totalResistance <= 0f || totalCurrent <= 0f)
            {
                return CreateInvalidResult();
            }

            CircuitCalculationResult result = new CircuitCalculationResult(
                true,
                totalVoltage,
                totalCurrent,
                totalResistance,
                activeResistors.Count);

            FillPerLoadValues(result, calculationMode, totalVoltage, totalCurrent, activeResistors);

            return result;
        }

        public float CalculateTotalResistance(CircuitState state)
        {
            List<CircuitResistor> activeResistors = GetActiveResistors();
            CircuitState calculationMode = GetCalculationMode(state);

            return CalculateTotalResistance(calculationMode, activeResistors);
        }

        public float CalculateTotalCurrent(float totalVoltage, float totalResistance)
        {
            if (totalVoltage <= 0f || totalResistance <= 0f)
            {
                return 0f;
            }

            return totalVoltage / totalResistance;
        }

        private CircuitState GetCalculationMode(CircuitState state)
        {
            if (state == CircuitState.Parallel)
            {
                return CircuitState.Parallel;
            }

            // LedOn is allowed when active resistors exist, using simple series-style aggregation for now.
            return CircuitState.Series;
        }

        private float CalculateTotalResistance(CircuitState state, List<CircuitResistor> activeResistors)
        {
            if (activeResistors == null || activeResistors.Count == 0)
            {
                return 0f;
            }

            if (state == CircuitState.Parallel)
            {
                return CalculateParallelResistance(activeResistors);
            }

            return CalculateSeriesResistance(activeResistors);
        }

        private float CalculateSeriesResistance(List<CircuitResistor> activeResistors)
        {
            float totalResistance = 0f;

            for (int i = 0; i < activeResistors.Count; i++)
            {
                CircuitResistor resistor = activeResistors[i];
                if (resistor == null)
                {
                    continue;
                }

                totalResistance += resistor.ResistanceOhms;
            }

            return totalResistance;
        }

        private float CalculateParallelResistance(List<CircuitResistor> activeResistors)
        {
            float reciprocalSum = 0f;

            for (int i = 0; i < activeResistors.Count; i++)
            {
                CircuitResistor resistor = activeResistors[i];
                if (resistor == null || resistor.ResistanceOhms <= 0f)
                {
                    continue;
                }

                reciprocalSum += 1f / resistor.ResistanceOhms;
            }

            if (reciprocalSum <= 0f)
            {
                return 0f;
            }

            return 1f / reciprocalSum;
        }

        private void FillPerLoadValues(
            CircuitCalculationResult result,
            CircuitState state,
            float totalVoltage,
            float totalCurrent,
            List<CircuitResistor> activeResistors)
        {
            if (result == null || activeResistors == null)
            {
                return;
            }

            for (int i = 0; i < activeResistors.Count; i++)
            {
                CircuitResistor resistor = activeResistors[i];
                if (resistor == null)
                {
                    continue;
                }

                if (state == CircuitState.Parallel)
                {
                    float branchCurrent = totalVoltage / resistor.ResistanceOhms;
                    result.SetPerLoadVoltage(resistor.ResistorId, totalVoltage);
                    result.SetPerBranchCurrent(resistor.ResistorId, branchCurrent);
                    continue;
                }

                float loadVoltage = totalCurrent * resistor.ResistanceOhms;
                result.SetPerLoadVoltage(resistor.ResistorId, loadVoltage);
            }
        }

        private List<CircuitResistor> GetActiveResistors()
        {
            List<CircuitResistor> activeResistors = new List<CircuitResistor>();

            if (context == null)
            {
                return activeResistors;
            }

            for (int i = 0; i < context.Resistors.Count; i++)
            {
                CircuitResistor resistor = context.Resistors[i];
                if (IsActiveResistor(resistor))
                {
                    activeResistors.Add(resistor);
                }
            }

            return activeResistors;
        }

        private bool IsActiveResistor(CircuitResistor resistor)
        {
            if (resistor == null ||
                string.IsNullOrEmpty(resistor.ResistorId) ||
                string.IsNullOrEmpty(resistor.PinAId) ||
                string.IsNullOrEmpty(resistor.PinBId) ||
                resistor.ResistanceOhms <= 0f)
            {
                return false;
            }

            return TryGetNodeIdFromPin(resistor.PinAId, out _) &&
                   TryGetNodeIdFromPin(resistor.PinBId, out _);
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

        private CircuitCalculationResult CreateInvalidResult()
        {
            return new CircuitCalculationResult(
                false,
                0f,
                0f,
                0f,
                0);
        }
    }
}
