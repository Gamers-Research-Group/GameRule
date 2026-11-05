using UnityEngine;
using UnityEditor;

class LoadWindow : EditorWindow {
    string fileName = "game.json";

    [MenuItem("Infograf/Load File")]
    public static void Init() {
        // Get existing open window or if none, make a new one:
        LoadWindow window = (LoadWindow)EditorWindow.GetWindow(typeof(LoadWindow), true, "INFOGRAF");
        window.Show();
    }

    private void OnGUI() {
        GUILayout.Label("Load File", EditorStyles.boldLabel);
        GUILayout.Space(8);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        GUILayout.Space(8);
        if (GUILayout.Button("Load")) {
            Loader.LoadJson(fileName);
        }
        GUIUtility.ExitGUI();
    }
}
