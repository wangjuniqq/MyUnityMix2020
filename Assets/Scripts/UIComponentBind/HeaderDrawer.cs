// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(HeaderAttribute))]
    internal sealed class HeaderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            if (!(attribute is HeaderAttribute headerAttribute))
            {
                return;
            }

            position = EditorGUI.IndentedRect(position);
            position.yMin += EditorGUIUtility.singleLineHeight * (headerAttribute.textHegihtIncrease - 0.5f);

            GUIStyle style = new GUIStyle(EditorStyles.label) { richText = true };
            GUIContent label = new GUIContent(
                $"<color = {headerAttribute.colorString}><size={style.fontSize + headerAttribute.textHegihtIncrease}><b>{headerAttribute.header}</b></size></color>"
            );

            EditorGUI.LabelField(position, label, style);
        }

        public override float GetHeight()
        {
            HeaderAttribute headerAttribute = attribute as HeaderAttribute;

            return EditorGUIUtility.singleLineHeight * (headerAttribute?.textHegihtIncrease + 0.5f ?? 0);
        }
    }
}