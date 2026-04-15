using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubePuzzleBuilder))]
public class CubePuzzleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CubePuzzleBuilder puzzleBuilder = (CubePuzzleBuilder) target;

        if (GUILayout.Button("Build"))
        {
            puzzleBuilder.BuildCube();
        }
    }
}
