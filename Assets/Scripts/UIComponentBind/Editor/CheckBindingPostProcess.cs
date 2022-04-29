using UnityEngine;
using UnityEditor;

#region Editor

namespace KH.UIBinding
{
    public class UIBindingHelper : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            GameObject goInHierarchy = Selection.activeGameObject;
            if (goInHierarchy != null)
            {
                UIComponentBind[] bindData = goInHierarchy.GetComponentsInChildren<UIComponentBind>(true);
                if (bindData != null)
                {
                    foreach (var comp in bindData)
                    {
                        comp.CheckBinding();
                    }
                }
            }

            return paths;
        }
    }
}
#endregion