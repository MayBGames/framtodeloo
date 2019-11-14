using UnityEngine;
using UnityEditor;

using UnityEditor.UIElements;
using UnityEngine.UIElements;

using VectorThreeField = MayB.Games.UI.Elements.Bounded.Int.VectorThree;

[CustomEditor(typeof(Sparse3DVolume))]
public class Sparse3DVolumeEditor : Editor {

  private Sparse3DVolume Volume;
  private VisualElement RootElement;

  public override VisualElement CreateInspectorGUI() {
    RootElement = new VisualElement { name = "3d-volume-editor" };

    Volume = GameObject.FindObjectOfType<Sparse3DVolume>();

    RootElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/UIElements/styles.uss"));

    string[] Labels = new string[] { "Width", "Height", "Depth" };

    RootElement.Add(new VectorThreeField("Dimensions", Labels, 1, 5, Volume.Dimensions));

    return RootElement;
  }

  // public override void OnInspectorGUI() {
  //   base.OnInspectorGUI();

  //   if (GUILayout.Button("Render"))
  //     ((Sparse3DVolume) target).SparseBuildMesh();
  // }
}