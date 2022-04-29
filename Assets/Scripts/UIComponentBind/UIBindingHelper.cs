using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;

# region Editor
#if UNITY_EDITOR

namespace KH.UIBinding
{
    public class UIBindingHelper
    {
        public static void SavePrefab(GameObject goInHierarchy)
        {
            Object goPrefab = null;
            GameObject objValid = null;
            GameObject objToCheck = goInHierarchy;
            string prefabPath = null;

            do
            {
#pragma warning disable 618
                var curPrefab = UnityEditor.PrefabUtility.GetPrefabParent(objToCheck);
                if (curPrefab == null)
                {
                    break;
                }

                string curPath = UnityEditor.AssetDatabase.GetAssetPath(curPrefab);
                if (prefabPath == null)
                {
                    prefabPath = curPath;
                }

                if (curPath != prefabPath)
                {
                    break;
                }

                goPrefab = curPrefab;
                objValid = objToCheck;

                var t = objToCheck.transform.parent;
                if (t != null)
                {
                    objToCheck = t.gameObject;
                }
                else
                {
                    break;
                }

            }
            while (true);

            if (objValid != null)
            {
                UnityEditor.PrefabUtility.ReplacePrefab(goInHierarchy, goPrefab, UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
            }
            else
            {
                Debug.LogFormat("<color=red>当前对象不属于Prefab, 请将其保存为 Prefab</color>");
            }

        }

        public static string GetCodeVarName(string itemName, string itemType, bool isArray)
        {
            return "";
        }


    }



}
#endif
#endregion