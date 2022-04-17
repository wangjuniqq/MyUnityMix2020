using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace KH.UIBinding
{
    //[CustomEditor(typeof(UIComponentBind))]
    public class UIComponentBindEditor : Editor
    {
        public static GUISkin skin = null;
        public static GUIStyle popupAlignLeft;

        public string[] allTypeNames;
        public Type[] allTypes;

        private List<BindItemInfo> _BindItemInfos;

        private List<BindItemDrawer> _ctrlItemDrawers;
        void Awake()
        {
            skin = EditorGUIUtility.Load("UIComponentBind/UIComponentBindSkin.guiskin") as GUISkin;



            allTypeNames = UIComponentBind.GetAllTypeNames();
            allTypes = UIComponentBind.GetAllTypes();
        }

        public override void OnInspectorGUI()
        {

            if (skin == null || skin.customStyles == null || skin.customStyles.Length == 0)
            {
                base.OnInspectorGUI();
                return;
            }

            if (popupAlignLeft == null)
            {
                popupAlignLeft = new GUIStyle("Popup");
                popupAlignLeft.alignment = TextAnchor.MiddleLeft;
            }

            UIComponentBind data = target as UIComponentBind;
            if (data.BindItemInfos == null)
                data.BindItemInfos = new List<BindItemInfo>();


            _BindItemInfos = data.BindItemInfos;
            CheckDrawers();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("组件绑定区", skin.customStyles[0]);
            if (_ctrlItemDrawers.Count == 0)
            {
                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    AddControlAfter(-1);
                    Repaint();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();
            foreach (var drawer in _ctrlItemDrawers)
            {
                GUILayout.Space(10f);
                if (!drawer.Draw())
                {
                    Repaint();
                    return;
                }
                GUILayout.Space(10f);
            }

            GUILayout.Space(10f);

            EditorGUILayout.Space(); EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            this.Repaint();
        }

        public void AddControlAfter(BindItemDrawer drawer)
        {
            int idx = _ctrlItemDrawers.IndexOf(drawer);
            Debug.Assert(idx != -1);

            AddControlAfter(idx);
        }


        public void RemoveControl(BindItemDrawer drawer)
        {
            int idx = _ctrlItemDrawers.IndexOf(drawer);
            Debug.Assert(idx != -1);

            RemoveControl(idx);
        }


        #region Private
        private void CheckDrawers()
        {
            if (_ctrlItemDrawers == null)
            {
                _ctrlItemDrawers = new List<BindItemDrawer>(100);
                foreach (var item in _BindItemInfos)
                {
                    BindItemDrawer drawer = new BindItemDrawer(this, item);
                    _ctrlItemDrawers.Add(drawer);
                }
            }
        }

        private void AddControlAfter(int idx)
        {
            BindItemInfo itemData = new BindItemInfo();
            _BindItemInfos.Insert(idx + 1, itemData);

            BindItemDrawer drawer = new BindItemDrawer(this, itemData);
            _ctrlItemDrawers.Insert(idx + 1, drawer);
        }

        private void RemoveControl(int idx)
        {
            _BindItemInfos.RemoveAt(idx);
            _ctrlItemDrawers.RemoveAt(idx);
        }

        #endregion
    }

}
