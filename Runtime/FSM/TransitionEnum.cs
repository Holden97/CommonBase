//Author：GuoYiBo
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataLayer
{
    public enum TransitionEnum
    {
        /// <summary>
        /// 无条件
        /// </summary>
        NONE,
        /// <summary>
        /// 结束准备
        /// </summary>
        END_PREPARE,
        /// <summary>
        /// 结束战斗
        /// </summary>
        END_BATTLE_TO_SHOPPING,
        END_BATTLE_TO_END,
        /// <summary>
        /// 结束购物
        /// </summary>
        END_SHOPPING,
        /// <summary>
        /// 进入结束阶段
        /// </summary>
        TO_END_VICTORY,
        TO_END_DEFEAT,
        TO_READY,
    }

}
