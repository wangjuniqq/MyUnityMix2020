using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;

public class CheckComponentFrequency : OdinEditorWindow
{

    [MenuItem("Tools/UI Lib/CheckComponentFrequency")]
    private static void ShowWindow()
    {
        var window = GetWindow<CheckComponentFrequency>();
        window.titleContent = new GUIContent("CheckComponentFrequency");
        window.Show();
    }


    List<string> mAllPrefabList = new List<string>();
    Dictionary<string, int> resultDic = new Dictionary<string, int>();

    [LabelText("扫描路径")]
    public string Scanpath = "/Resources/";

    [Button]
    public void GetResult()
    {
        var sw = new Stopwatch();
        sw.Start();

        resultDic.Clear();

        if (mAllPrefabList.Count == 0)
        {
            mAllPrefabList = Directory.GetFiles(Application.dataPath + Scanpath, "*.prefab", SearchOption.AllDirectories).ToList();
        }

        int maxNum = mAllPrefabList.Count;
        for (int j = 0; j < maxNum; j++)
        {
            bool canceled = EditorUtility.DisplayCancelableProgressBar("Checking", j + "/" + maxNum, (float)j / maxNum);
            if (canceled)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            string item = mAllPrefabList[j];
            string regScriptStr = "m_Script: {fileID:\\s?[a-fA-F0-9]+, guid:\\s?([a-fA-F0-9]+), type: 3}";
            string regChunkStr = "luaAsset: {fileID:\\s?[a-fA-F0-9]+, guid:\\s?([a-fA-F0-9]+), type: 3}";
            Regex reg1 = new Regex(regScriptStr);
            Regex reg2 = new Regex(regChunkStr);
            MatchCollection matches1 = reg1.Matches(File.ReadAllText(item));
            MatchCollection matches2 = reg2.Matches(File.ReadAllText(item));

            foreach (Match m in matches1)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(m.Groups[1].Value);
                int index = scriptPath.LastIndexOf('/');
                string keyStr = scriptPath.Substring(index + 1, scriptPath.Length - index - 1);
                if (!resultDic.ContainsKey(keyStr))
                {
                    resultDic.Add(keyStr, 1);
                }
                else
                {
                    resultDic[keyStr] += 1;
                }
            }

            foreach (Match m in matches2)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(m.Groups[1].Value);
                if (string.IsNullOrEmpty(scriptPath))
                {
                    UnityEngine.Debug.LogError(item);
                    continue;
                }
                int index = scriptPath.LastIndexOf('/');
                string keyStr = scriptPath.Substring(index + 1, scriptPath.Length - index - 1);
                if (!resultDic.ContainsKey(keyStr))
                {
                    resultDic.Add(keyStr, 1);
                }
                else
                {
                    resultDic[keyStr] += 1;
                }
            }
        }

        EditorUtility.ClearProgressBar();

        UnityEngine.Debug.Log(sw.ElapsedMilliseconds + "ms");
    }

    [Button]
    public void SaveResultCS()
    {
        List<KeyValuePair<string, int>> lst = new List<KeyValuePair<string, int>>(resultDic);
        lst.Sort(delegate (KeyValuePair<string, int> s1, KeyValuePair<string, int> s2)
        {
            return s2.Value.CompareTo(s1.Value);
        });

        string result_path = "Assets/Editor/result1.txt";
        using (StreamWriter sw = new StreamWriter(result_path))
        {
            foreach (var item in lst)
            {
                sw.Write(item.Key + "," + item.Value + "\n");
            }
        }
    }

    [Button]
    public void SaveResultLua()
    {
        List<KeyValuePair<string, int>> lst = new List<KeyValuePair<string, int>>(resultDic);
        lst.Sort(delegate (KeyValuePair<string, int> s1, KeyValuePair<string, int> s2)
        {
            return s2.Value.CompareTo(s1.Value);
        });

        string result_path = "Assets/Editor/result2.txt";
        using (StreamWriter sw = new StreamWriter(result_path))
        {
            foreach (var item in lst)
            {
                if (!item.Key.StartsWith("Act_"))
                {
                    sw.Write(item.Key + "," + item.Value + "\n");
                }
            }
        }
    }
}