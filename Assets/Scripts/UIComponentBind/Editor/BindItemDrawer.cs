using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using KH.UIBinding;

namespace KH.KHEditor
{
    public enum AllType
    {
        GameObject,
        Transform,
    }
    public class BindItemDrawer
    {
        private UIComponentBindEditor _container;
        private BindItemInfo _itemData;
        private bool _foldout = true;
        private int _controlTypeIdx = 0;

        GenericSelector<int> CustomGenericSelector = null;

        string[] tempAllTypeName = new string[]{
            "GameObject",
            "Transform"
        };

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
                _foldout = EditorGUILayout.Foldout(_foldout, _foldout ? "收起" : "展开", true);

                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    _container.AddControlAfter(this);
                    return false;
                }

                if (GUILayout.Button("-", EditorStyles.miniButton))
                {
                    _container.RemoveControl(this);
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
                        if (_controlTypeIdx != 0)
                        {
                            if (!ChangeControlsTypeTo(_controlTypeIdx))
                                _controlTypeIdx = 0;
                        }
                        else
                        {
                            _itemData.ItemType = string.Empty;
                        }

                        return false;
                    }

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();





                EditorGUILayout.Space();
                for (int i = 0, imax = _itemData.ItemTargets.Length; i < imax; i++)
                {
                    UnityEngine.Object obj = _itemData.ItemTargets[i];
                    EditorGUILayout.BeginHorizontal();
                    _itemData.ItemTargets[i] = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true);

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
                            Debug.LogError("至少应保留一个");
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

            if (EditorGUIUtility.isProSkin)
                GUI.Box(new Rect(rect.x - 10f, rect.y - 5f, rect.width + 20f, rect.height + 15f), "");
            else
                GUI.Box(new Rect(rect.x - 10f, rect.y - 5f, rect.width + 20f, rect.height + 15f), "", UIComponentBindEditor.skin.box);

            PostProcess();
            return true;
        }

        private void PostProcess()
        {

            if (_itemData.ItemTargets.Length > 0 && _itemData.ItemTargets[0] != null && string.IsNullOrEmpty(_itemData.ItemName))
            {
                _itemData.ItemName = _itemData.ItemTargets[0].name.Trim();

                GameObject go = _itemData.ItemTargets[0] as GameObject;
                List<string> temp = new List<string>();
                if (go == null)
                {
                    return;
                }
                foreach (var com in go.GetComponents<Component>())
                {
                    temp.Add(com.GetType().Name);
                }
                temp.Add("GameObject");
                temp.Add("Transform");

                tempAllTypeName = temp.ToArray();


            }
        }

        private int FindTypeIdx(string typeName)
        {
            string[] allTypeNames = _itemData.AllTypeNames;
            for (int i = 0, imax = allTypeNames.Length; i < imax; i++)
            {
                if (allTypeNames[i] == typeName)
                    return i;
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

        private bool ChangeControlsTypeTo(int typeIdx)
        {
            //System.Type targetType = _container.allTypes[typeIdx];
            string targetTypeName = _itemData.AllTypeNames[typeIdx];
            bool isGameObject = targetTypeName == "GameObject";


            for (int i = 0, imax = _itemData.ItemTargets.Length; i < imax; i++)
            {
                Object obj = _itemData.ItemTargets[i];
                if (obj == null)
                {
                    Debug.LogErrorFormat("[{0}.{1}] control[{2}] is null"
                        , _container.target.name, _itemData.ItemName, i);
                    return false;
                }

                if (obj.GetType() != typeof(GameObject))
                {
                    if ((obj as Component) == null)
                    {
                        Debug.LogErrorFormat("[{0}.{1}] control[{2}] [{3}] must be GameObject or a Component"
                            , _container.target.name, _itemData.ItemName, i, obj.name);
                        return false;
                    }
                    obj = (obj as Component).gameObject;
                }

                GameObject go = obj as GameObject;
                if (isGameObject)
                    _itemData.ItemTargets[i] = go;
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
                        Debug.LogErrorFormat("[{0}.{1}] control[{2}] [{3}] isn't a {4}"
                            , _container.target.name, _itemData.ItemName, i, go.name, targetTypeName);
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