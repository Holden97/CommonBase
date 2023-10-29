using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    /// <summary>
    /// 扇形生成器
    /// </summary>
    public class Fan : AbstractMeshGenerator
    {
        public float beginAngle;
        public float endAngle;
        public float radius;

        public FanFoldCenter fanFoldType;


        public enum FanFoldCenter
        {
            LEFT,
            CENTER,
            RIGHT,
        }
    }
}
