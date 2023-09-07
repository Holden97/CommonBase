using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataLayer
{
    public enum GameState
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        UNINIT,
        /// <summary>
        /// 准备
        /// </summary>
        PREPARING,
        /// <summary>
        /// 正在游戏-战斗中
        /// </summary>
        PLAYING_BATTLING,
        /// <summary>
        /// 正在游戏-购物中
        /// </summary>
        PLAYING_SHOPPING,
        /// <summary>
        /// 结束游戏
        /// </summary>
        END,
    }

}
