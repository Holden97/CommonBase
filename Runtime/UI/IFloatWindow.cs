
using UnityEngine;

namespace CommonBase
{
    public interface IFloatWindow
    {
        Transform FloatWindowTransform { get; }
        BaseUI parent { get; }
    }
}