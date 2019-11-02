using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Sparse3DVolume))]
public class Sparse3DVolumeEditor : Editor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    if (GUILayout.Button("Render"))
      ((Sparse3DVolume) target).SparseBuildMesh();
  }
}