using System.Collections.Generic;

namespace CommonBase
{
    /// <summary>
    /// 用于遮挡型UI隐藏后恢复现场
    /// </summary>
    public struct UIShowInfoList
    {
        public List<UIShowInfo> uiInfo;

        public UIShowInfoList(UIStack uiList)
        {
            this.uiInfo = new List<UIShowInfo>();
            foreach (var ui in uiList)
            {
                uiInfo.Add(new UIShowInfo(ui.GetInstanceID(), ui.IsShowing));
            }
        }

        public bool Contains(BaseUI ui)
        {
            return uiInfo.Exists(x => x.instanceId == ui.GetInstanceID());
        }

        public bool IsShowing(BaseUI ui)
        {
            var hasExisted = uiInfo.Exists(x => x.instanceId == ui.GetInstanceID());
            if (hasExisted)
            {
                var curUI = uiInfo.Find(x => x.instanceId == ui.GetInstanceID());
                return curUI.isShowing;
            }
            else
            {
                return false;
            }
        }
    }

    public struct UIShowInfo
    {
        public int instanceId;
        public bool isShowing;

        public UIShowInfo(int instanceId, bool isShowing)
        {
            this.instanceId = instanceId;
            this.isShowing = isShowing;
        }
    }
}

