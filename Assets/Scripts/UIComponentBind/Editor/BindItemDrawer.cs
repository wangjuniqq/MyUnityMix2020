using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using KH.UIBinding;

namespace KH.KHEditor
{
    public class BindItemDrawer
    {
        private UIComponentBindEditor _container;
        private BindItemInfo _itemData;
        private bool _foldout = true;
        private int _controlTypeIdx = 0;
        private readonly string MsgTip = "[组件绑定检查]: ";

        public BindItemDrawer(UIComponentBindEditor container, BindItemInfo item)
        {
            _container = container;
            _itemData = item;
        }

        public bool Draw()
        {
            Rect rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("变量名 ", UIComponentBindEditor.skin.label, GUILayout.Width(60f));
                _itemData.ItemName = EditorGUILayout.TextField(_itemData.ItemName, UIComponentBindEditor.skin.textField).Trim();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                _foldout = EditorGUILayout.Foldout(_foldout, _foldout ? "收起" : "展开", true);

                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    _container.AddItem(this);
                    return false;
                }

                if (GUILayout.Button("-", EditorStyles.miniButton))
                {
                    _container.RemoveItem(this);
                    return false;
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            if (_foldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("变量类型 ", UIComponentBindEditor.skin.label, GUILayout.Width(60f));

                    if (_controlTypeIdx == 0 && !string.IsNullOrEmpty(_itemData.ItemType))
                    {
                        _controlTypeIdx = FindTypeIdx(_itemData.ItemType);
                    }

                    EditorGUI.BeginChangeCheck();

                    GUIStyle popupAlignLeft = new GUIStyle("Popup");
                    popupAlignLeft.alignment = TextAnchor.MiddleLeft;
                    _controlTypeIdx = EditorGUILayout.Popup(_controlTypeIdx, _itemData.AllTypeNames, popupAlignLeft);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (!RefreshItemTypeByIndex(_controlTypeIdx))
                        {
                            _controlTypeIdx = _itemData.AllTypeNames.Length;
                        }

                        return false;
                    }

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space();
                for (int i = 0; i < _itemData.ItemTargets.Length; i++)
                {
                    UnityEngine.Object obj = _itemData.ItemTargets[i];

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginChangeCheck();
                    _itemData.ItemTargets[i] = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        var objTemp = _itemData.ItemTargets[i];
                        if (objTemp is GameObject)
                        {
                            var go = objTemp as GameObject;
                            _itemData.ItemName = go.name;
                            _itemData.ItemType = "GameObject";

                            var comps = go.GetComponents<Component>();
                            var tempTypes = new string[comps.Length + 1];
                            tempTypes[tempTypes.Length - 1] = "GameObject";

                            for (int j = 0; j < comps.Length; j++)
                            {
                                tempTypes[j] = comps[j].GetType().Name;
                            }

                            _itemData.AllTypeNames = tempTypes;
                            _controlTypeIdx = tempTypes.Length - 1;
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (GUILayout.Button("+", EditorStyles.miniButton))
                    {
                        InsertItem(i + 1);
                        return false;
                    }

                    if (GUILayout.Button("-", EditorStyles.miniButton))
                    {
                        if (_itemData.ItemTargets.Length == 1)
                        {
                            Debug.LogErrorFormat("{0}至少应保留一个", this.MsgTip);
                            return false;
                        }
                        else
                        {
                            RemoveItem(i);
                            return false;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.EndVertical();

            GUI.Box(new Rect(rect.x - 10f, rect.y - 5f, rect.width + 20f, rect.height + 15f), "");

            PostProcess();

            return true;
        }

        private void PostProcess()
        {
            CheckName();
        }

        private void CheckName()
        {
            if (_itemData.ItemTargets.Length > 0 && _itemData.ItemTargets[0] != null && string.IsNullOrEmpty(_itemData.ItemName))
            {
                _itemData.ItemName = _itemData.ItemTargets[0].name.Trim();
            }
        }

        private int FindTypeIdx(string typeName)
        {
            string[] allTypeNames = _itemData.AllTypeNames;

            for (int i = 0, imax = allTypeNames.Length; i < imax; i++)
            {
                if (allTypeNames[i] == typeName)
                {
                    return i;
                }
            }

            return 0;
        }

        private void InsertItem(int idx)
        {
            Object[] newArr = new Object[_itemData.ItemTargets.Length + 1];

            for (int i = 0; i < idx; i++)
            {
                newArr[i] = _itemData.ItemTargets[i];
            }

            newArr[idx] = null;

            for (int i = idx + 1; i < newArr.Length; i++)
            {
                newArr[i] = _itemData.ItemTargets[i - 1];
            }

            _itemData.ItemTargets = newArr;
        }

        private void RemoveItem(int idx)
        {
            Object[] newArr = new Object[_itemData.ItemTargets.Length - 1];

            for (int i = 0; i < idx; i++)
            {
                newArr[i] = _itemData.ItemTargets[i];
            }

            for (int i = idx; i < newArr.Length; i++)
            {
                newArr[idx] = _itemData.ItemTargets[i + 1];
            }

            _itemData.ItemTargets = newArr;
        }

        private bool RefreshItemTypeByIndex(int typeIdx)
        {
            string targetTypeName = _itemData.AllTypeNames[typeIdx];
            bool isGameObject = (targetTypeName == "GameObject");

            for (int i = 0; i < _itemData.ItemTargets.Length; i++)
            {
                Object obj = _itemData.ItemTargets[i];

                if (obj == null)
                {
                    Debug.LogErrorFormat("{0} {1}.{2} 组件[{3} 对象为空", this.MsgTip, _container.target.name, _itemData.ItemName, i);
                    return false;
                }

                if (!(obj is GameObject))
                {
                    if (obj is Component)
                    {
                        obj = (obj as Component).gameObject;
                    }
                    else
                    {
                        return false;
                    }
                }

                GameObject go = obj as GameObject;

                if (isGameObject)
                {
                    _itemData.ItemTargets[i] = go;
                }
                else
                {
                    Component comp = _itemData.TargetCom;
                    foreach (var item in go.GetComponents<Component>())
                    {
                        if (item.GetType().Name == targetTypeName)
                        {
                            comp = item;
                            break;
                        }
                    }

                    if (comp == null)
                    {
                        return false;
                    }

                    _itemData.ItemTargets[i] = comp;
                }
            }

            _itemData.ItemType = targetTypeName;
            return true;
        }
    }
}