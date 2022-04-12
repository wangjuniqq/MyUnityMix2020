using UnityEngine;
using UnityEditor;

public class PropertiesWindow : EditorWindow
{
    private static Editor m_scriptEditor;

    //[MenuItem("MyControlBind/PropertiesWindow")]
    public static void ShowWindow(MonoBehaviour data)
    {
        if (m_scriptEditor == null || m_scriptEditor.target == null)
        {
            m_scriptEditor = Editor.CreateEditor(data);
        }

        var window = GetWindow<PropertiesWindow>();
        window.titleContent = new GUIContent("PropertiesWindow");
        window.Show();
    }

    private void OnGUI()
    {
        m_scriptEditor.OnInspectorGUI();
    }
}