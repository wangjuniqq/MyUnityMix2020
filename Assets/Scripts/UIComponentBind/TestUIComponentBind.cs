using UnityEngine;
using UnityEngine.UI;

namespace KH.UIBinding
{
    public class TestUIComponentBind : UIWindow
    {

        #region AutoBindUI
        [UIComponentBinding]
        private UILabel NameLb;
        #endregion






        private void Start()
        {
            this.NameLb.text = "1";
        }

    }
}