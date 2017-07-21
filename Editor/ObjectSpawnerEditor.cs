using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawnerScript))]

public class ObjectSpawnerEditor : Editor {
    private GameObject go;
    public string args = "";
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ObjectSpawnerScript spawner = (ObjectSpawnerScript)target;
        if(GUILayout.Button("Room 2x2")) {
            args = "2x2";
            go = spawner.BuildRoom(args);
        }
        else if (GUILayout.Button("Room 2x1")) {
            args = "2x1";
            go = spawner.BuildRoom(args);
        }
        else if(GUILayout.Button("Room 1x2")) {
            args = "1x2";
            go = spawner.BuildRoom(args);
        }
        else if (GUILayout.Button("Room 1x1")) {
            args = "1x1";
            go = spawner.BuildRoom(args);
        }
        else if (GUILayout.Button("NPC_Officer_Player")) {
            args = "MyOfficer";
            go = spawner.BuildRoom(args);
        }

        if (go!=null) {
            Selection.activeGameObject = go;
        }
    }
}
