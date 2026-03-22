using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRCircuit.Data
{
    [Serializable]
    public class CircuitContext
    {
        [SerializeField] private List<CircuitPin> pins = new List<CircuitPin>();
        [SerializeField] private List<CircuitSocket> sockets = new List<CircuitSocket>();
        [SerializeField] private List<CircuitWire> wires = new List<CircuitWire>();
        [SerializeField] private List<CircuitBattery> batteries = new List<CircuitBattery>();
        [SerializeField] private List<CircuitLed> leds = new List<CircuitLed>();
        [SerializeField] private List<CircuitSwitch> switches = new List<CircuitSwitch>();

        public IReadOnlyList<CircuitPin> Pins => pins;
        public IReadOnlyList<CircuitSocket> Sockets => sockets;
        public IReadOnlyList<CircuitWire> Wires => wires;
        public IReadOnlyList<CircuitBattery> Batteries => batteries;
        public IReadOnlyList<CircuitLed> Leds => leds;
        public IReadOnlyList<CircuitSwitch> Switches => switches;

        public CircuitPin GetPinById(string pinId)
        {
            if (string.IsNullOrEmpty(pinId))
            {
                return null;
            }

            for (int i = 0; i < pins.Count; i++)
            {
                if (pins[i] != null && pins[i].PinId == pinId)
                {
                    return pins[i];
                }
            }

            return null;
        }

        public CircuitSocket GetSocketById(string socketId)
        {
            if (string.IsNullOrEmpty(socketId))
            {
                return null;
            }

            for (int i = 0; i < sockets.Count; i++)
            {
                if (sockets[i] != null && sockets[i].SocketId == socketId)
                {
                    return sockets[i];
                }
            }

            return null;
        }

        public CircuitSwitch GetSwitchById(string switchId)
        {
            if (string.IsNullOrEmpty(switchId))
            {
                return null;
            }

            for (int i = 0; i < switches.Count; i++)
            {
                if (switches[i] != null && switches[i].SwitchId == switchId)
                {
                    return switches[i];
                }
            }

            return null;
        }

        public CircuitBattery GetBatteryById(string batteryId)
        {
            if (string.IsNullOrEmpty(batteryId))
            {
                return null;
            }

            for (int i = 0; i < batteries.Count; i++)
            {
                if (batteries[i] != null && batteries[i].BatteryId == batteryId)
                {
                    return batteries[i];
                }
            }

            return null;
        }

        public CircuitLed GetLedById(string ledId)
        {
            if (string.IsNullOrEmpty(ledId))
            {
                return null;
            }

            for (int i = 0; i < leds.Count; i++)
            {
                if (leds[i] != null && leds[i].LedId == ledId)
                {
                    return leds[i];
                }
            }

            return null;
        }

        public CircuitWire GetWireById(string wireId)
        {
            if (string.IsNullOrEmpty(wireId))
            {
                return null;
            }

            for (int i = 0; i < wires.Count; i++)
            {
                if (wires[i] != null && wires[i].WireId == wireId)
                {
                    return wires[i];
                }
            }

            return null;
        }

        //테스트용 코드
        public void ClearAll()
        {
            pins.Clear();
            sockets.Clear();
            wires.Clear();
            batteries.Clear();
            leds.Clear();
            switches.Clear();
        }

        public void AddPin(CircuitPin pin)
        {
            if (pin != null)
            {
                pins.Add(pin);
            }
        }

        public void AddSocket(CircuitSocket socket)
        {
            if (socket != null)
            {
                sockets.Add(socket);
            }
        }

        public void AddWire(CircuitWire wire)
        {
            if (wire != null)
            {
                wires.Add(wire);
            }
        }

        public void AddBattery(CircuitBattery battery)
        {
            if (battery != null)
            {
                batteries.Add(battery);
            }
        }

        public void AddLed(CircuitLed led)
        {
            if (led != null)
            {
                leds.Add(led);
            }
        }

        public void AddSwitch(CircuitSwitch circuitSwitch)
        {
            if (circuitSwitch != null)
            {
                switches.Add(circuitSwitch);
            }
        }
    }
}
