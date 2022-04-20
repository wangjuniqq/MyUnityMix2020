using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;


[CustomEditor(typeof(TestOdin))]
public class TestOdinEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TestOdin data = target as TestOdin;


        DrawSeperateLine("Component Area");

        base.OnInspectorGUI();

    }

    private void DrawSeperateLine(string titleStr)
    {
        GUIStyle textStyle = new GUIStyle("HeaderLabel")
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };

        GUIHelper.PushColor(new Color(173.0f / 255, 216.0f / 255, 230.0f / 255));
        GUILayout.Label($"———————————————————— {titleStr} ————————————————————", textStyle);
        GUIHelper.PopColor();
    }
}