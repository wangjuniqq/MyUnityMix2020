using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace KH.UIBinding
{
    [Serializable]
    public class BindItemInfo
    {
        public string ItemName = string.Empty;
        public string ItemType = string.Empty;
        public UnityEngine.Object[] ItemTargets = new UnityEngine.Object[1];

#if UNITY_EDITOR
        [HideInInspector]
        public Component TargetCom = null;
        [HideInInspector]
        public string[] AllTypeNames = new string[2]
        {
            "GameObject",
            "Transform",
        };
#endif

        public override string ToString()
        {
            return ItemName;
        }
    }
}