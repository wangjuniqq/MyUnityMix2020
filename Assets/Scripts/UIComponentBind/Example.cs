using UnityEngine;
using UnityEditor;

public class Example : MonoBehaviour
{
    void OnGUI()
    {
        if (Event.current.type == EventType.MouseDrag)
        {
            // Clear out drag data
            DragAndDrop.PrepareStartDrag();

            // Set up what we want to drag
            DragAndDrop.paths[0] = "Assets/Scripts/UIComponentBind/GameObject";

            // Start the actual drag
            DragAndDrop.StartDrag("Dragging title");

            // Make sure no one uses the event after us
            Event.current.Use();
        }
    }
}