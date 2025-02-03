using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class MathfExtension
    {
        public static string ToString(this float value, int places)
        {
            return value.ToString($"f{places}");
        }

        public static float ToRound(this float value, int places)
        {
            return (Mathf.Round(value * Mathf.Pow(10, places)) / Mathf.Pow(10, places));
        }
    }
}
