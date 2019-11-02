using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(_3DVolume))]
public class _3DVolumeEditor : Editor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    if (GUILayout.Button("Render"))
      ((_3DVolume) target).BuildMesh();
  }
}