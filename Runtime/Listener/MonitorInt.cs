using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public delegate void OnValueChange(int value);
    public class MonitorInt
    {
        private int v;
        public int Value
        {
            get { return v; }
            set
            {
                v = value;
                onChange?.Invoke(v);
            }
        }
        public event OnValueChange onChange;
        public MonitorInt(int value)
        {
            this.Value = value;
        }

        public static implicit operator int(MonitorInt monitor)
        {
            return monitor.Value;
        }

        public static implicit operator MonitorInt(int value)
        {
            return new MonitorInt(value);
        }
    }
}
