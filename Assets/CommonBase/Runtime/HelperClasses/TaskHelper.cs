using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonBase
{
    public static class TaskHelper
    {
        /// <summary>
        /// 等待bool条件达成
        /// </summary>
        /// <param name="func">条件函数</param>
        /// <returns></returns>
        public static async Task<bool> Wait(Func<bool> func)
        {
            try
            {
                if (func == null) return false;
                while (!func.Invoke())
                {
                    //Debug.Log("条件未达成!");
                    await Task.Delay(1000);
                }
                //Debug.Log("条件达成!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                throw;
            }
        }
    }
}

