using System;

namespace Digiphy.IoT
{
    [Serializable]
    public class StatusResponse
    {
        public StatusSNS StatusSNS;
    }

    [Serializable]
    public class StatusSNS
    {
        public ENERGY ENERGY;
    }

    [Serializable]
    public class ENERGY
    {
        public string TotalStartTime;
        public float Total;
        public float Yesterday;
        public float Today;
        public float Power;
        public float ApparentPower;
        public float ReactivePower;
        public float Factor;
        public float Voltage;
        public float Current;
    }
}

