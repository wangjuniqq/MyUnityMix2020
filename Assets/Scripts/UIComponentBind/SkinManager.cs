using UnityEngine;
using UnityEditor;

namespace KH.UIBinding
{
    public class SkinManager
    {
        private static SkinManager _instance;
        public static SkinManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SkinManager();
            }
            return _instance;
        }

        public static string SkinPath = "UIComponentBind/UIComponentBindSkin.guiskin";

        private static GUISkin s_mySkin;
        public static GUISkin s_MySkin
        {
            get
            {
                if (s_mySkin == null)
                {
                    s_mySkin = EditorGUIUtility.Load(SkinPath) as GUISkin;
                }
                return s_mySkin;
            }

        }
        private static GUIStyle s_myPopupAlignLeft;
        public static GUIStyle s_MyPopupAlignLeft
        {
            get
            {
                if (s_myPopupAlignLeft == null)
                {
                    s_myPopupAlignLeft = new GUIStyle("Popup");
                    s_myPopupAlignLeft.alignment = TextAnchor.MiddleLeft;
                }
                return s_myPopupAlignLeft;
            }

        }
    }
}