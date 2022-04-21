using UnityEngine;
using UnityEditor;
using KH.UIBinding;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;

namespace KH.KHEditor
{
    [CustomEditor(typeof(UIComponentBind))]
    public class UIComponentBindEditor : OdinEditor
    {
        public static GUISkin skin = null;

        private static bool isAddListen = false;
        private UIComponentBind _targetData;
        private List<BindItemInfo> _BindItems;
        private List<BindItemDrawer> _bindItemDrawers;

        private Rect _dragArea = new Rect();
        private Object _activeObj = null;
        private Object[] _activeObjComs = null;
        private Rect[] _typeRects = null;
        private string[] _typeNames = null;
        private Object curSelectCom = null;
        private string _curSelectComName = string.Empty;

        void Awake()
        {
            if (skin == null)
            {
                skin = EditorGUIUtility.Load("UIComponentBind/UIComponentBindSkin.guiskin") as GUISkin;
            }

            if (!isAddListen)
            {
                EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
                // isAddListen = true;
            }
        }

        public override void OnInspectorGUI()
        {
            if (skin == null || skin.customStyles == null || skin.customStyles.Length == 0)
            {
                base.OnInspectorGUI();
                return;
            }

            _targetData = target as UIComponentBind;
            if (_targetData.BindItems == null)
            {
                _targetData.BindItems = new List<BindItemInfo>();
            }
            _BindItems = _targetData.BindItems;

            OnDropAreaGUI();

            CheckDrawers();

            #region Draw Items
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            foreach (var drawer in _bindItemDrawers)
            {
                GUILayout.Space(10f);
                if (!drawer.Draw())
                {
                    if (GUI.changed)
                    {
                        Repaint();
                    }
                    return;
                }
                GUILayout.Space(10f);
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            #endregion

            if (GUI.changed)
            {
                Repaint();
            }
        }

        public void OnDropAreaGUI()
        {
            Event evt = Event.current;
            _dragArea = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            string title = "拖动组件到此可快速绑定";

            GUI.Box(_dragArea, title, skin.customStyles[0]);

            DrawType();

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    bool isOk = false;
                    var index = GetCurRectIndex(evt.mousePosition);
                    isOk = index > -2;
                    if (!isOk)
                    {
                        _activeObj = null;
                        _typeRects = new Rect[0];
                        break;
                    }

                    if (index == -1)
                    {
                        _curSelectComName = "GameObject";
                        if (DragAndDrop.objectReferences.Length > 0)
                        {
                            curSelectCom = DragAndDrop.objectReferences[0];
                        }
                    }

                    if (index >= 0)
                    {
                        _curSelectComName = this._typeNames[index];
                        curSelectCom = this._activeObjComs[index];
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        var obj = DragAndDrop.objectReferences[0];
                        if (obj is GameObject)
                        {
                            var go = obj as GameObject;
                            if (_targetData.IsPartOfCurPrefab(go.transform))
                            {
                                this._activeObj = go;
                            }
                        }
                    }

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        if (_activeObj != null)
                        {
                            AddItemDragAndDrop(_activeObj, -1, _curSelectComName, curSelectCom);
                        }
                    }

                    evt.Use();
                    break;
                case EventType.DragExited:
                    _activeObj = null;
                    _typeRects = new Rect[0];
                    break;
                default:
                    break;
            }

            GUI.color = Color.white;
            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void DrawType()
        {
            if (_activeObj == null)
            {
                return;
            }

            GameObject go = _activeObj as GameObject;
            if (go == null)
            {
                _activeObj = null;
                return;
            }

            var comps = go.GetComponents<Component>();

            _typeNames = new string[comps.Length + 1];
            _typeNames[_typeNames.Length - 1] = "GameObject";

            _activeObjComs = new Object[comps.Length + 1];
            _activeObjComs[_activeObjComs.Length - 1] = go.GetComponent<GameObject>();

            for (var i = 0; i < _typeNames.Length - 1; i++)
            {
                _typeNames[i] = comps[i].GetType().Name;
                _activeObjComs[i] = comps[i];
            }

            _typeRects = new Rect[_typeNames.Length];

            GUILayout.BeginVertical();

            for (var i = 0; i < _typeNames.Length; i++)
            {
                var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(30));
                _typeRects[i] = r;
                GUI.color = r.Contains(Event.current.mousePosition) ? Color.green : Color.white;

                GUI.Box(r, _typeNames[i]);

                if (GUI.changed)
                {
                    Repaint();
                }
            }

            GUILayout.EndVertical();
        }

        private int GetCurRectIndex(Vector2 pos)
        {
            int index = -2;
            if (_dragArea != null && _dragArea.Contains(pos))
            {
                return -1;
            }

            if (_typeRects == null)
            {
                return index;
            }

            for (int i = 0; i < _typeRects.Length; i++)
            {
                if (_typeRects[i].Contains(pos))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private void CheckDrawers()
        {
            if (_bindItemDrawers == null)
            {
                _bindItemDrawers = new List<BindItemDrawer>(50);
                foreach (var item in _BindItems)
                {
                    BindItemDrawer drawer = new BindItemDrawer(this, item);
                    _bindItemDrawers.Add(drawer);
                }
            }
        }

        private void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            var data = target as UIComponentBind;
            if (data == null)
            {
                return;
            }

            foreach (var item in data.BindItems)
            {
                foreach (var target in item.ItemTargets)
                {
                    bool isOk = false;

                    if (target is GameObject)
                    {
                        isOk = (target == obj);
                    }

                    if (target is Component)
                    {
                        var com = target as Component;
                        isOk = (com.gameObject == obj);
                    }

                    if (isOk)
                    {
                        var r = new Rect(selectionRect);
                        r.x = 35;
                        r.width = 75;
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.yellow;
                        if (style != null && obj != null)
                        {
                            GUI.Label(r, "★", style);
                        }
                    }
                }
            }
        }

        private bool CheckObj(Object obj)
        {
            bool result = false;
            if (obj is GameObject)
            {
                var go = obj as GameObject;
                if (_targetData != null)
                {
                    if (_targetData.IsPartOfCurPrefab(go.transform))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private void AddItemDragAndDrop(Object obj, int idx, string name, Object com)
        {
            if (!CheckObj(obj))
            {
                return;
            }

            BindItemInfo itemData = new BindItemInfo();

            var go = obj as GameObject;
            var comps = go.GetComponents<Component>();

            string[] typeNames = new string[comps.Length + 1];
            typeNames[typeNames.Length - 1] = "GameObject";

            for (var i = 0; i < typeNames.Length - 1; i++)
            {
                typeNames[i] = comps[i].GetType().Name;
            }

            itemData.AllTypeNames = typeNames;
            itemData.ItemType = name;
            itemData.TargetCom = (com as Component);

            if (name == "GameObject")
            {
                itemData.ItemTargets[0] = go;
            }
            else
            {
                itemData.ItemTargets[0] = itemData.TargetCom;
            }

            _BindItems.Insert(idx + 1, itemData);

            BindItemDrawer drawer = new BindItemDrawer(this, itemData);
            _bindItemDrawers.Insert(idx + 1, drawer);
        }

        public void AddItem(BindItemDrawer drawer)
        {
            int idx = _bindItemDrawers.IndexOf(drawer);
            Debug.Assert(idx != -1);

            AddItemByIndex(idx);
        }

        public void RemoveItem(BindItemDrawer drawer)
        {
            int idx = _bindItemDrawers.IndexOf(drawer);
            Debug.Assert(idx != -1);

            RemoveItemByIndex(idx);
        }

        private void AddItemByIndex(int idx)
        {
            BindItemInfo itemData = new BindItemInfo();
            _BindItems.Insert(idx + 1, itemData);

            BindItemDrawer drawer = new BindItemDrawer(this, itemData);
            _bindItemDrawers.Insert(idx + 1, drawer);
        }

        private void RemoveItemByIndex(int idx)
        {
            _BindItems.RemoveAt(idx);
            _bindItemDrawers.RemoveAt(idx);
        }
    }
}