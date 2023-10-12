using UnityEngine.UI;

namespace CommonBase
{
    public class ImportantInfoConfirmPanel : BaseUI
    {
        public Text contentText;
        public Button confirmBtn;
        public override void UpdateView(object o)
        {
            var content = o as string;
            contentText.text = content;
        }

        public override void Initialize()
        {
            confirmBtn.onClick.AddListener(() => { this.Close(); });
        }
    }
}