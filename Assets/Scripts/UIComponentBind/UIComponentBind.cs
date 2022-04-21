using System;
using UnityEngine;
using System.Text;
// using KH.Lua;
// using LuaInterface;
using System.Reflection;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KH.UIBinding
{
    [DisallowMultipleComponent, HideMonoScript]
    public class UIComponentBind : MonoBehaviour, IUIMediator
    {
        public List<BindItemInfo> BindItems;

        #region  BindData
        public static Dictionary<Type, UIFieldsInfo> s_UIFieldsCache = new Dictionary<Type, UIFieldsInfo>();

        private void Awake()
        {
            var monos = gameObject.GetComponents<MonoBehaviour>();
            foreach (var mono in monos)
            {
                this.BindDataToMono(mono);
            }
        }

        public void BindDataToMono(MonoBehaviour mono)
        {
            UIFieldsInfo fieldInfos = GetUIFieldsInfo(mono.GetType());

            var controls = fieldInfos.BindedComponents;
            for (int i = 0, imax = controls.Count; i < imax; i++)
            {
                BindComponentToMono(mono, controls[i]);
            }
        }

        private static UIFieldsInfo GetUIFieldsInfo(Type type)
        {
            UIFieldsInfo uIFieldsInfo;
            if (s_UIFieldsCache.TryGetValue(type, out uIFieldsInfo))
            {
                return uIFieldsInfo;
            }

            uIFieldsInfo = new UIFieldsInfo() { UIType = type };
            FieldInfo[] fis = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0, imax = fis.Length; i < imax; i++)
            {
                FieldInfo fi = fis[i];

                if (fi.IsDefined(typeof(UIComponentBindingAttribute), false))
                {
                    uIFieldsInfo.BindedComponents.Add(fi);
                }
            }

            s_UIFieldsCache.Add(type, uIFieldsInfo);

            return uIFieldsInfo;
        }

        private void BindComponentToMono(MonoBehaviour mono, FieldInfo fi)
        {
            int itemIdx = GetBindComponentIndex(fi.Name);
            var objs = BindItems[itemIdx];

            Type fieldType = fi.FieldType;
            if (fieldType.IsArray)
            {
                Array arrObj = Array.CreateInstance(fieldType.GetElementType(), objs.ItemTargets.Length);
                for (int j = 0, jmax = objs.ItemTargets.Length; j < jmax; j++)
                {
                    if (objs.ItemTargets[j] != null)
                    {
                        arrObj.SetValue(objs.ItemTargets[j], j);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Component {0}[{1}] is null", objs.ItemName, j);
                    }
                }
                fi.SetValue(mono, arrObj);
            }
            else
            {
                UnityEngine.Object component = GetBindComponent(itemIdx);
                fi.SetValue(mono, component);
            }
        }

        private UnityEngine.Object GetBindComponent(int idx)
        {
            if (idx == -1 || idx >= BindItems.Count)
            {
                return null;
            }

            var targets = BindItems[idx].ItemTargets;
            if (targets.Length == 0)
            {
                return null;
            }

            return targets[0];
        }

        private int GetBindComponentIndex(string name)
        {
            for (int i = 0, imax = BindItems.Count; i < imax; i++)
            {
                BindItemInfo item = BindItems[i];
                if (item.ItemName == name)
                {
                    return i;
                }
            }

            return -1;
        }

        // public void BindDataToLua(LuaBehaviourWrapper wrapper)
        // {
        //     LuaTable luaTableArgs = CBDefine.GetCoreBridge().RuntimeLua_NewTable();
        //     LuaTable luaTable = CBDefine.GetCoreBridge().RuntimeLua_NewTable();
        //     for (int i = 0; i < BindItemInfos.Count; i++)
        //     {
        //         string strArgName = BindItemInfos[i].ItemName;
        //         string strTypeName = BindItemInfos[i].ItemType;
        //         if (BindItemInfos[i].ItemTargets.Length > 1)
        //         {
        //             int len = BindItemInfos[i].ItemTargets.Length;
        //             LuaTable lstArgs = CBDefine.GetCoreBridge().RuntimeLua_NewTable();
        //             luaTable[strArgName] = lstArgs;
        //             for (int j = 0; j < len; j++)
        //             {
        //                 lstArgs[j + 1] = BindItemInfos[i].ItemTargets[j];
        //             }
        //             luaTableArgs[strArgName] = strTypeName + "[]";
        //         }
        //         else
        //         {
        //             luaTable[strArgName] = BindItemInfos[i].ItemTargets[0];
        //             luaTableArgs[strArgName] = strTypeName;
        //         }
        //     }

        //     wrapper.CallLuaFunction("__SetBindArgsDefine", luaTableArgs);
        //     wrapper.CallLuaFunction("__SetBindUISerializeField", luaTable);
        // }
        #endregion

        #region Editor
#if UNITY_EDITOR
        public bool CheckBinding()
        {
            bool isOK = true;
            if (!CheckBindingName())
            {
                return false;
            }

            isOK = CheckBindingTarget();
            if (isOK)
            {
                Debug.LogFormat("[{0}]绑定完成", gameObject.name);
            }

            return isOK;
        }

        private bool CheckBindingName()
        {
            bool isOK = true;
            for (int i = 0; i < BindItems.Count; i++)
            {
                if (string.IsNullOrEmpty(BindItems[i].ItemName))
                {
                    Debug.LogErrorFormat("[{1}]第 {0} 个组件变量名为空", i + 1, gameObject.name);
                    return false;
                }

                for (int j = BindItems.Count - 1; j >= 0; j--)
                {
                    if (BindItems[i].ItemName == BindItems[j].ItemName && i != j)
                    {
                        Debug.LogErrorFormat("[{3}]组件变量名字重复 [{0}]: 第 {1} 项与第 {2} 项", BindItems[i].ItemName, i + 1, j + 1, gameObject.name);
                        return false;
                    }
                }
            }

            return isOK;
        }

        private bool CheckBindingTarget()
        {
            for (int i = 0; i < BindItems.Count; i++)
            {
                var objs = BindItems[i].ItemTargets;
                Type type = null;

                for (int j = 0; j < objs.Length; j++)
                {
                    if (objs[j] == null)
                    {
                        var err_msg = string.Format("[{2}]组件变量 [{0}] 绑定对象第 {1} 项为空", BindItems[i].ItemName, j + 1, gameObject.name);
                        Debug.LogException(new Exception(err_msg));
                        return false;
                    }

                    GameObject go = objs[j] as GameObject;
                    if (go == null)
                    {
                        go = (objs[j] as Component).gameObject;
                    }

                    if (!IsPartOfCurPrefab(go.transform))
                    {
                        Debug.LogErrorFormat("[{2}]组件变量 [{0}] 第 {1} 项不是当前 Prefab 下的组件", BindItems[i].ItemName, j + 1, gameObject.name);
                        return false;
                    }

                    UnityEngine.Object correctComponent = FindCorrectComponent(go, BindItems[i].ItemType);
                    if (correctComponent == null)
                    {
                        Debug.LogErrorFormat("[{3}]组件变量 [{0}] 第 {1} 项不是 {2} 类型", BindItems[i].ItemName, j + 1, BindItems[i].ItemType, gameObject.name);
                        return false;
                    }

                    if (type == null)
                    {
                        type = correctComponent.GetType();
                    }

                    objs[j] = correctComponent;
                }

                BindItems[i].ItemType = type.Name;
            }
            return true;
        }

        public bool IsPartOfCurPrefab(Transform t)
        {
            do
            {
                if (t == transform)
                {
                    return true;
                }

                t = t.parent;
            }
            while (t != null);

            return false;
        }

        private UnityEngine.Object FindCorrectComponent(GameObject go, string typeName)
        {
            if (typeName == "GameObject")
            {
                return go;
            }

            List<Component> components = new List<Component>();
            go.GetComponents(components);

            Func<string, Component> getSpecialTypeComp = (string _typeName) =>
            {
                foreach (var comp in components)
                {
                    Type compType = comp.GetType();
                    if (compType.Name == _typeName)
                    {
                        return comp;
                    }
                }
                return null;
            };

            Component newComp = getSpecialTypeComp(typeName);

            return newComp;
        }

        private bool IsNeedSave()
        {
            foreach (var ctrl in BindItems)
            {
                if (string.IsNullOrEmpty(ctrl.ItemType))
                {
                    return true;
                }
            }
            return false;
        }

        [ContextMenu("Copy Code (C#)")]
        public void CopyCodeToClipBoardPrivate()
        {
            CopyCodeToClipBoardImpl("private");
        }

        public void CopyCodeToClipBoardProtected()
        {
            CopyCodeToClipBoardImpl("protected");
        }

        public void CopyCodeToClipBoardPublic()
        {
            CopyCodeToClipBoardImpl("public");
        }

        private void CopyCodeToClipBoardImpl(string accessLevel)
        {
            if (IsNeedSave())
            {
                UIBindingHelper.SavePrefab(gameObject);
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("#region AutoBindUI");

            foreach (var ctrl in BindItems)
            {
                if (ctrl.ItemTargets.Length == 0)
                {
                    continue;
                }

                if (ctrl.ItemTargets.Length == 1)
                {
                    sb.AppendFormat("\t\t[UIComponentBinding]\r\n\t\t{0} {1} {2};\r\n", accessLevel, ctrl.ItemType, ctrl.ItemName);
                }
                else
                {
                    sb.AppendFormat("\t\t[UIComponentBinding]\r\n\t\t{0} {1}[] {2};\r\n", accessLevel, ctrl.ItemType, ctrl.ItemName);
                }
            }
            sb.Append("#endregion\r\n\r\n");

            GUIUtility.systemCopyBuffer = sb.ToString();
        }

        [ContextMenu("Copy Code (Lua)")]
        public void CopyCodeToClipBoardLua()
        {
            if (IsNeedSave())
            {
                UIBindingHelper.SavePrefab(gameObject);
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("-- AutoBindUI Begin\r\n");

            foreach (var ctrl in BindItems)
            {
                if (ctrl.ItemTargets.Length == 0)
                {
                    continue;
                }

                if (ctrl.ItemTargets.Length == 1)
                {
                    sb.AppendFormat("\t--__protected.{0} type:{1}\r\n", ctrl.ItemName, ctrl.ItemType);
                }
                else
                {
                    sb.AppendFormat("\t--__protected.{0} type:{1}[]\r\n", ctrl.ItemName, ctrl.ItemType);
                }
            }
            sb.Append("\t-- AutoBindUI End\r\n\r\n");

            GUIUtility.systemCopyBuffer = sb.ToString();
        }

        [ContextMenu("ShowPropertiesWindow")]
        public void ShowPropertiesWindow()
        {
            PropertiesWindow.ShowWindow(this);

        }



#endif
        #endregion
    }
}