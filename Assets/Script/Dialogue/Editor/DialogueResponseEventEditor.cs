using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueResponseEvent))]
public class DialogueResponseEventEditor : Editor
{

    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DialogueResponseEvent responseEvent = (DialogueResponseEvent)target;

        if(GUILayout.Button("Refresh"))
        {
            responseEvent.OnValidate();
        }
    }
}
