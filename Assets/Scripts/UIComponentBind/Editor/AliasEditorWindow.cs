using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using KH.UIBinding;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.IO;

public class AliasEditorWindow : OdinEditorWindow
{
    [ReadOnly]
    public string ConfigPath = "";

    [MenuItem("Tools/UI Binding/AliasConfig")]
    private static void OpenWindow()
    {
        GetWindow<AliasEditorWindow>().Show();
    }

    protected override void OnEnable()
    {
        ConfigPath = $"{Application.dataPath}/Editor/UIComponentBind/alias.bytes";
        // ConfigPath = $"{Application.dataPath}/Scripts/UI/Common/KHUITool/UIComponentBind/Editor/alias.bytes";

        ComAliasList = GetDataByConfig();

        if (ComAliasList == null)
        {
            ComAliasList = new List<ComAlias>{
                new ComAlias{TypeName = "UILabel", TypeAlias="Lbl"},
                new ComAlias{TypeName = "UIButton", TypeAlias="Btn"},
                new ComAlias{TypeName = "UISprite", TypeAlias="Spr"},
            };
        };
    }

    [TableList]
    public List<ComAlias> ComAliasList;

    // [Button("CreateConfig")]
    public void CreateConfig()
    {
        if (ComAliasList == null)
        {
            return;
        }
        List<ComAlias> configData = ComAliasList;
        byte[] bytes = SerializationUtility.SerializeValue(configData, DataFormat.Binary);
        if (!File.Exists(ConfigPath))
        {
            File.Create(ConfigPath).Close();
        }
        File.WriteAllBytes(ConfigPath, bytes);
    }

    [Button("SaveConfig")]
    public void SaveConfig()
    {
        if (ComAliasList == null)
        {
            return;
        }
        List<ComAlias> configData = ComAliasList;
        byte[] bytes = SerializationUtility.SerializeValue(configData, DataFormat.Binary);
        if (!File.Exists(ConfigPath))
        {
            File.Create(ConfigPath).Close();
        }
        File.WriteAllBytes(ConfigPath, bytes);
    }

    // [Button("GetDataByConfig")]
    public void GetData()
    {
        var list = GetDataByConfig();
        if (list == null)
        {
            Debug.Log("list == null");
        }
        foreach (var item in list)
        {
            Debug.Log(item.TypeAlias);
        }
    }

    public List<ComAlias> GetDataByConfig()
    {
        List<ComAlias> configData = null;
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfigPath.Replace(Application.dataPath, "Assets"));
        byte[] bytes = textAsset.bytes;
        if (bytes != null)
        {
            configData = SerializationUtility.DeserializeValue<object>(bytes, DataFormat.Binary) as List<ComAlias>;
        }
        return configData;
    }
}

[System.Serializable]
public class ComAlias
{
    [LabelText("变量名")]
    public string TypeName;
    [LabelText("变量别名")]
    public string TypeAlias;
}