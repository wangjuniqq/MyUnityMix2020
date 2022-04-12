using System;
using UnityEngine;

namespace KH.UIBinding
{
    [Serializable]
    public class BindItemInfo
    {
        public string ItemName = string.Empty;
#if UNITY_EDITOR
        [HideInInspector]
        public string ItemType = string.Empty;
#endif
        public UnityEngine.Object[] ItemTargets = new UnityEngine.Object[1];

        public override string ToString()
        {
            return ItemName;
        }
    }
}