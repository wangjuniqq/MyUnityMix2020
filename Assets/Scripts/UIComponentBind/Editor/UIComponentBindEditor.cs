using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using KH.UIBinding;

namespace KH.KHEditor
{
    [CustomEditor(typeof(UIComponentBind))]
    public class UIComponentBindEditor : OdinEditor
    {
        public static GUISkin skin = null;

        private List<BindItemInfo> _BindItems;
        private List<BindItemDrawer> _bindItemDrawers;

        private Object _activeObj = null;
        private Object[] _activeObjComs = null;

        private Rect _dragArea = new Rect();
        private Rect[] _typeRects = null;
        private string[] _typeNames = null;
        private Object curSelectCom = null;
        private string curSelectComName = string.Empty;

        private const string DRAG_ID = "SceneDragAndDrop";
        private string title = "MyDrag";
        public UIComponentBind data;

        void Awake()
        {
            if (skin == null)
            {
                skin = EditorGUIUtility.Load("UIComponentBind/UIComponentBindSkin.guiskin") as GUISkin;
            }
        }


        public override void OnInspectorGUI()
        {
            if (skin == null || skin.customStyles == null || skin.customStyles.Length == 0)
            {
                base.OnInspectorGUI();
                return;
            }

            data = target as UIComponentBind;
            if (data.BindItems == null)
            {
                data.BindItems = new List<BindItemInfo>();
            }
            _BindItems = data.BindItems;

            OnDropAreaGUI();

            CheckDrawers();

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
            GUILayout.Space(10f);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

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


                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        var obj = DragAndDrop.objectReferences[0];
                        if (obj is GameObject && data.IsPartOfCurPrefab((obj as GameObject).transform))
                        {
                            this._activeObj = obj;
                        }
                    }

                    if (index == -1)
                    {
                        curSelectComName = "GameObject";
                        if (DragAndDrop.objectReferences.Length > 0)
                            curSelectCom = DragAndDrop.objectReferences[0];
                    }
                    if (index >= 0)
                    {
                        curSelectComName = this._typeNames[index];
                        curSelectCom = this._activeObjComs[index];
                    }


                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        if (_activeObj != null)
                        {
                            AddNew(_activeObj, -1, curSelectComName, curSelectCom);
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
            if (_activeObj != null)
            {
                GUILayout.BeginVertical();
                GameObject go = _activeObj as GameObject;
                if (go == null)
                {
                    _activeObj = null;
                    return;
                }
                var types = go.GetComponents<Component>();

                _typeNames = new string[types.Length + 1];
                _typeNames[_typeNames.Length - 1] = "GameObject";

                _activeObjComs = new Object[types.Length + 1];
                _activeObjComs[_typeNames.Length - 1] = (_activeObj as GameObject).GetComponent<GameObject>();


                for (var i = 0; i < _typeNames.Length - 1; i++)
                {
                    _typeNames[i] = types[i].GetType().Name;
                    _activeObjComs[i] = types[i];
                }

                _typeRects = new Rect[_typeNames.Length];
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

        public void AddControlAfter(BindItemDrawer drawer)
        {
            int idx = _bindItemDrawers.IndexOf(drawer);
            Debug.Assert(idx != -1);

            AddControlAfter(idx);
        }


        public void RemoveControl(BindItemDrawer drawer)
        {
            int idx = _bindItemDrawers.IndexOf(drawer);
            Debug.Assert(idx != -1);

            RemoveControl(idx);
        }

        private void CheckDrawers()
        {
            if (_bindItemDrawers == null)
            {
                _bindItemDrawers = new List<BindItemDrawer>(100);
                foreach (var item in _BindItems)
                {
                    BindItemDrawer drawer = new BindItemDrawer(this, item);
                    _bindItemDrawers.Add(drawer);
                }
            }
        }

        private void AddControlAfter(int idx)
        {
            BindItemInfo itemData = new BindItemInfo();
            _BindItems.Insert(idx + 1, itemData);

            BindItemDrawer drawer = new BindItemDrawer(this, itemData);
            _bindItemDrawers.Insert(idx + 1, drawer);
        }

        private void RemoveControl(int idx)
        {
            _BindItems.RemoveAt(idx);
            _bindItemDrawers.RemoveAt(idx);
        }

        private void AddNew(Object obj, int idx, string curSelectTypeName, Object mono)
        {
            BindItemInfo itemData = new BindItemInfo();
            if (obj is GameObject && data.IsPartOfCurPrefab((obj as GameObject).transform))
            {

                var types = (obj as GameObject).GetComponents<Component>();
                string[] typeNames = new string[types.Length + 1];
                typeNames[typeNames.Length - 1] = "GameObject";
                for (var i = 0; i < typeNames.Length - 1; i++)
                {
                    typeNames[i] = types[i].GetType().Name;
                }
                itemData.AllTypeNames = typeNames;
                itemData.ItemType = curSelectTypeName;
                itemData.TargetCom = (mono as Component);
                if (curSelectTypeName == "GameObject")
                {
                    itemData.ItemTargets[0] = obj as GameObject;
                }
                else
                {
                    itemData.ItemTargets[0] = itemData.TargetCom;
                }


                _BindItems.Insert(idx + 1, itemData);

                BindItemDrawer drawer = new BindItemDrawer(this, itemData);
                _bindItemDrawers.Insert(idx + 1, drawer);
            }

        }
    }

}
