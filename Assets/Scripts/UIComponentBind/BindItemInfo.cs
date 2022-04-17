using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace KH.UIBinding
{
    [Serializable]
    public class BindItemInfo
    {
        [LabelText("变量名")]
        public string ItemName = string.Empty;
        [LabelText("变量类型")]
        [ValueDropdown("AllTypeNames")]
        public string ItemType = string.Empty;
        private static ValueDropdownList<string> AllTypeNames = new ValueDropdownList<string>()
        {
            {"GameObject"},
            {"UILabel"},
            {"UISprite"},
        };
        public UnityEngine.Object[] ItemTargets = new UnityEngine.Object[1];

        public override string ToString()
        {
            return ItemName;
        }
    }
}