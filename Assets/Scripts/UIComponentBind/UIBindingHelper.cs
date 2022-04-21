﻿using UnityEngine;
using UnityEditor;

# region Editor
#if UNITY_EDITOR

namespace KH.UIBinding
{
    public class UIBindingHelper : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            GameObject goInHierarchy = Selection.activeGameObject;
            if (goInHierarchy != null)
            {
                UIComponentBind[] bindData = goInHierarchy.GetComponentsInChildren<UIComponentBind>(true);
                if (bindData != null)
                {
                    foreach (var comp in bindData)
                    {
                        comp.CheckBinding();
                    }
                }
            }

            return paths;
        }

        public static void SavePrefab(GameObject goInHierarchy)
        {
            Object goPrefab = null;
            GameObject objValid = null;
            GameObject objToCheck = goInHierarchy;
            string prefabPath = null;

            do
            {
#pragma warning disable 618
                var curPrefab = PrefabUtility.GetPrefabParent(objToCheck);
                if (curPrefab == null)
                {
                    break;
                }

                string curPath = AssetDatabase.GetAssetPath(curPrefab);
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
                PrefabUtility.ReplacePrefab(goInHierarchy, goPrefab, ReplacePrefabOptions.ConnectToPrefab);
            }
            else
            {
                Debug.LogFormat("<color=red>当前对象不属于Prefab, 请将其保存为 Prefab</color>");
            }

        }
    }

}
#endif
#endregion