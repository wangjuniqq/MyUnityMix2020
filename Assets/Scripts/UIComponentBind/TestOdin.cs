using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class TestOdin : MonoBehaviour
{
    //[BoxGroup]
    [OnInspectorGUI("DrawColoredHeader", append: false)]
    public int A;

    [BoxGroup]
    public int B;

#if UNITY_EDITOR
    private void DrawColoredHeader()
    {
        GUIStyle textStyle = new GUIStyle("HeaderLabel")
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            //imagePosition = ImagePosition.ImageAbove,
            fontStyle = FontStyle.Bold,

            //fixedWidth = 60
        };

        GUIHelper.PushColor(new Color(173.0f / 255, 216.0f / 255, 230.0f / 255));
        GUILayout.Label("———————————————————— Component Area ————————————————————", textStyle);
        GUIHelper.PopColor();
    }
#endif
    [GUIColor(173 / 255, 216 / 255, 230 / 255, 1)]
    [Title("[ffcc00]Titles and Headers", titleAlignment: TitleAlignments.Centered, bold: true)]
    public string HeaderStr;
    [ColoredFoldoutGroup("group1", 0f, 1f, 0f)]
    public string Top;
    [ColoredFoldoutGroup("group1")]
    public string Middle;
    [ColoredFoldoutGroup("group1")]
    public string Bottom;

    [ColoredFoldoutGroup("group2", 1f, 0f, 0f)]
    public int first = 1;
    [ColoredFoldoutGroup("group2")]
    public int second = 2;
    [ColoredFoldoutGroup("group2")]
    public int third = 3;

    [OnInspectorGUI("GUIBefore", "GUIAfter")]
    public int MyField;

    private void GUIBefore()
    {
        GUILayout.Label("Label before My Field property");
    }

    private void GUIAfter()
    {
        GUILayout.Label("Label after My Field property");
    }

    [OnInspectorInit("@Texture = EditorIcons.OdinInspectorLogo")]
    [OnInspectorGUI("DrawPreview", append: true)]
    public Texture2D Texture;

    private void DrawPreview()
    {
        if (this.Texture == null) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(this.Texture);
        GUILayout.EndVertical();
    }

    [OnInspectorGUI]
    private void OnInspectorGUI()
    {
        UnityEditor.EditorGUILayout.HelpBox("OnInspectorGUI can also be used on both methods and properties", UnityEditor.MessageType.Info);
    }

    public bool SomeCondition;
}

public class ColoredFoldoutGroupAttribute : PropertyGroupAttribute
{
    public float R, G, B, A;

    public ColoredFoldoutGroupAttribute(string path)
        : base(path)
    {
    }

    public ColoredFoldoutGroupAttribute(string path, float r, float g, float b, float a = 1f)
        : base(path)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    protected override void CombineValuesWith(PropertyGroupAttribute other)
    {
        var otherAttr = (ColoredFoldoutGroupAttribute)other;

        this.R = Math.Max(otherAttr.R, this.R);
        this.G = Math.Max(otherAttr.G, this.G);
        this.B = Math.Max(otherAttr.B, this.B);
        this.A = Math.Max(otherAttr.A, this.A);
    }
}



public class ColoredFoldoutGroupAttributeDrawer : OdinGroupDrawer<ColoredFoldoutGroupAttribute>
{
    private LocalPersistentContext<bool> isExpanded;

    protected override void Initialize()
    {
        this.isExpanded = this.GetPersistentValue<bool>(
            "ColoredFoldoutGroupAttributeDrawer.isExpanded",
            GeneralDrawerConfig.Instance.ExpandFoldoutByDefault);
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        GUIHelper.PushColor(new Color(this.Attribute.R, this.Attribute.G, this.Attribute.B, this.Attribute.A));
        SirenixEditorGUI.BeginBox();
        SirenixEditorGUI.BeginBoxHeader();
        GUIHelper.PopColor();
        this.isExpanded.Value = SirenixEditorGUI.Foldout(this.isExpanded.Value, label);
        SirenixEditorGUI.EndBoxHeader();

        if (SirenixEditorGUI.BeginFadeGroup(this, this.isExpanded.Value))
        {
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                this.Property.Children[i].Draw();
            }
        }

        SirenixEditorGUI.EndFadeGroup();
        SirenixEditorGUI.EndBox();
    }
}