using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Text;
// using LuaInterface;
// using KH.Lua;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KH.UIBinding
{
    [DisallowMultipleComponent]
    [HideMonoScript]
    public class UIComponentBind : MonoBehaviour
    {
        [OnInspectorGUI("DrawSeperateLine", append: false)]
        // [ListDrawerSettings(DraggableItems = false, Expanded = true)]
        // [HideLabel]
        public List<BindItemInfo> BindItemInfos;
        private void DrawSeperateLine()
        {
            string titleStr = "Component Area";
            GUIStyle textStyle = new GUIStyle("HeaderLabel")
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

            GUIHelper.PushColor(new Color(173.0f / 255, 216.0f / 255, 230.0f / 255));
            GUILayout.Label($"———————————————————— {titleStr} ————————————————————", textStyle);
            GUIHelper.PopColor();

            if (BindItemInfos.Count == 0)
            {
                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    AddControlAfter(-1);
                    GUIHelper.RequestRepaint();
                    return;
                }
            }
        }

        private void AddControlAfter(int idx)
        {
            BindItemInfo itemData = new BindItemInfo();
            BindItemInfos.Insert(idx + 1, itemData);
        }

        private void RemoveControl(int idx)
        {
            BindItemInfos.RemoveAt(idx);
        }

        public static Dictionary<Type, UIFieldsInfo> s_UIFieldsCache = new Dictionary<Type, UIFieldsInfo>();



        public static Dictionary<string, Type> _typeMap = new Dictionary<string, Type>()
        {
            // { "UISprite", typeof(UISprite)},
            // { "UILabel", typeof(UILabel)},
            // { "UIButton", typeof(UIButton)},
            // { "UIToggle", typeof(UIToggle)},

            // {"KH.LuaBehaviourWrapper", typeof(KH.Lua.LuaBehaviourWrapper) },

            // {"KH.UIScrollViewController", typeof(KH.UIScrollViewController)},
            // {"KH.UIGridController", typeof(KH.UIGridController) },

            { "Transform", typeof(Transform)},
            { "GameObject", typeof(GameObject)},
        };

        public static string[] GetAllTypeNames()
        {
            string[] keys = new string[_typeMap.Count + 1];
            keys[0] = "自动";
            _typeMap.Keys.CopyTo(keys, 1);
            return keys;
        }

        public static Type[] GetAllTypes()
        {
            Type[] types = new Type[_typeMap.Count + 1];
            types[0] = typeof(UnityEngine.Object);
            _typeMap.Values.CopyTo(types, 1);
            return types;
        }

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
            var objs = BindItemInfos[itemIdx];

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
            if (idx == -1 || idx >= BindItemInfos.Count)
                return null;

            var targets = BindItemInfos[idx].ItemTargets;
            if (targets.Length == 0)
                return null;

            return targets[0];
        }

        private int GetBindComponentIndex(string name)
        {
            for (int i = 0, imax = BindItemInfos.Count; i < imax; i++)
            {
                BindItemInfo item = BindItemInfos[i];
                if (item.ItemName == name)
                    return i;
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

        #region For Editor
#if UNITY_EDITOR

        public bool CorrectComponents()
        {
            bool isOK = true;
            for (int i = 0, imax = BindItemInfos.Count; i < imax; i++)
            {
                if (string.IsNullOrEmpty(BindItemInfos[i].ItemName))
                {
                    Debug.LogErrorFormat("[{1}]第 {0} 个组件变量名不能为空, 请修正", i + 1, gameObject.name);
                    return false;
                }

                for (int j = BindItemInfos.Count - 1; j >= 0; j--)
                {
                    if (BindItemInfos[i].ItemName == BindItemInfos[j].ItemName && i != j)
                    {
                        Debug.LogErrorFormat("[{3}]组件变量名字 [{0}] 第 {1} 项与第 {2} 项重复, 请修正", BindItemInfos[i].ItemName, i + 1, j + 1, gameObject.name);
                        return false;
                    }
                }
            }

            isOK = ReplaceTargetsToUIComponent();
            if (isOK)
            {
                Debug.LogFormat("[{0}]组件绑定完成", gameObject.name);
            }

            return isOK;
        }

        private bool ReplaceTargetsToUIComponent()
        {
            for (int i = 0, imax = BindItemInfos.Count; i < imax; i++)
            {
                var objs = BindItemInfos[i].ItemTargets;
                Type type = null;
                for (int j = 0, jmax = objs.Length; j < jmax; j++)
                {
                    if (objs[j] == null)
                    {
                        Debug.LogErrorFormat("[{2}]组件变量名字 [{0}] 第 {1} 项为空，请修正", BindItemInfos[i].ItemName, j + 1, gameObject.name);
                        return false;
                    }

                    GameObject go = objs[j] as GameObject;
                    if (go == null)
                        go = (objs[j] as Component).gameObject;

                    if (!IsInCurrentPrefab(go.transform))
                    {
                        Debug.LogErrorFormat("[{2}]组件变量名字 [{0}] 第 {1} 项不是当前 Prefab 下的组件变量，请修正", BindItemInfos[i].ItemName, j + 1, gameObject.name);
                        return false;
                    }

                    UnityEngine.Object correctComponent = FindCorrectComponent(go, BindItemInfos[i].ItemType);
                    if (correctComponent == null)
                    {
                        Debug.LogErrorFormat("[{3}]组件变量 [{0}] 第 {1} 项不是 {2} 类型，请修正", BindItemInfos[i].ItemName, j + 1, BindItemInfos[i].ItemType, gameObject.name);
                        return false;
                    }

                    if (type == null)
                    {
                        if (string.IsNullOrEmpty(BindItemInfos[i].ItemType))
                        {
                            type = correctComponent.GetType();
                        }
                        else
                        {
                            if (!_typeMap.TryGetValue(BindItemInfos[i].ItemType, out type))
                            {
                                Debug.LogError("Internal Error, pls contact author");
                                return false;
                            }
                        }
                    }
                    else if (correctComponent.GetType() != type && !correctComponent.GetType().IsSubclassOf(type))
                    {
                        Debug.LogErrorFormat("[{2}]组件变量名字 [{0}] 第 {1} 项与第 1 项的类型不同，请修正", BindItemInfos[i].ItemName, j + 1, gameObject.name);
                        return false;
                    }

                    objs[j] = correctComponent;
                }

                BindItemInfos[i].ItemType = type.Name;
            }
            return true;
        }

        private bool IsInCurrentPrefab(Transform t)
        {
            do
            {
                if (t == transform)
                    return true;
                t = t.parent;
            } while (t != null);
            return false;
        }

        private UnityEngine.Object FindCorrectComponent(GameObject go, string typename)
        {
            if (typename == "GameObject")
                return go;

            List<Component> components = new List<Component>();
            go.GetComponents(components);

            Func<Type, Component> getSpecialTypeComp = (Type t) =>
            {
                foreach (var comp in components)
                {
                    Type compType = comp.GetType();
                    if (compType == t || compType.IsSubclassOf(t))
                    {
                        return comp;
                    }
                }
                return null;
            };

            Component newComp = null;

            if (string.IsNullOrEmpty(typename))
            {
                foreach (var kv in _typeMap)
                {
                    newComp = getSpecialTypeComp(kv.Value);
                    if (newComp != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                Type type = null;
                if (_typeMap.TryGetValue(typename, out type))
                {
                    newComp = getSpecialTypeComp(type);
                }
            }

            return newComp;
        }

        private bool IsNeedSave()
        {
            foreach (var ctrl in BindItemInfos)
            {
                if (string.IsNullOrEmpty(ctrl.ItemType))
                {
                    return true;
                }
            }
            return false;
        }

        [ContextMenu("ShowPropertiesWindow(Lua)")]
        public void ShowPropertiesWindow()
        {
            PropertiesWindow.ShowWindow(this);
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
                UIComponentBindHelper.SavePrefab(gameObject);

            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("#region AutoBindUI");

            foreach (var ctrl in BindItemInfos)
            {
                if (ctrl.ItemTargets.Length == 0)
                    continue;

                if (ctrl.ItemTargets.Length == 1)
                    sb.AppendFormat("\t\t[UIComponentBinding]\r\n\t\t{0} {1} {2};\r\n", accessLevel, ctrl.ItemType, ctrl.ItemName);
                else
                    sb.AppendFormat("\t\t[UIComponentBinding]\r\n\t\t{0} {1}[] {2};\r\n", accessLevel, ctrl.ItemType, ctrl.ItemName);
            }
            sb.Append("#endregion\r\n\r\n");

            GUIUtility.systemCopyBuffer = sb.ToString();
        }

        [ContextMenu("Copy Code (Lua)")]
        public void CopyCodeToClipBoardLua()
        {
            if (IsNeedSave())
            {
                UIComponentBindHelper.SavePrefab(gameObject);
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("-- AutoBindUI Begin\r\n");

            foreach (var ctrl in BindItemInfos)
            {
                if (ctrl.ItemTargets.Length == 0)
                    continue;

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
#endif
        #endregion
    }
}