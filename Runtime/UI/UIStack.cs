using CommonBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI栈结构，在StackPro上有所拓展
/// </summary>
public class UIStack : StackPro<BaseUI>
{
    public BaseUI PopFirstActive()
    {
        if (items.Count > 0)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                BaseUI item = items[i];
                if (!item.escRemovable || !item.IsShowing)
                {
                    continue;
                }
                items.RemoveAt(items.Count - 1);
                return item;
            }
            return default;
        }
        else
        {
            return default;
        }
    }

    public BaseUI PeekFirstActive()
    {
        if (items.Count > 0)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                BaseUI item = items[i];
                if (!item.escRemovable || !item.IsShowing)
                {
                    continue;
                }
                return item;
            }
            return default;
        }
        else
        {
            return default;
        }
    }
}
