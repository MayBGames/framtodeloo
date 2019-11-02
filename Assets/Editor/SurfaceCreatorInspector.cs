using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SurfaceCreator))]
public class SurfaceCreatorInspector : Editor {

	private SurfaceCreator creator;

	private void OnEnable() => creator = target as SurfaceCreator;

	public override void OnInspectorGUI () {
		DrawDefaultInspector();
	  
    if (GUILayout.Button("Refresh"))
			creator.Refresh();
	}
}