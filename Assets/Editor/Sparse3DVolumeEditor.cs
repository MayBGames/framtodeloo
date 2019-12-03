using UnityEngine;
using UnityEditor;

using UnityEngine.UIElements;

using VectorThreeIntField   = MayB.Games.UI.Elements.Bounded.Int.VectorThree;
using VectorThreeFloatField = MayB.Games.UI.Elements.Bounded.Float.VectorThree;

[CustomEditor(typeof(Sparse3DVolume))]
public class Sparse3DVolumeEditor : Editor {

  private Sparse3DVolume Volume;
  private VisualElement RootElement;

  public override VisualElement CreateInspectorGUI() {
    RootElement = new VisualElement { name = "3d-volume-editor" };

    Volume = GameObject.FindObjectOfType<Sparse3DVolume>();

    string Vector3Styles      = "Assets/Editor/UIElements/vector3.uss";
    string VolumeEditorStyles = "Assets/Editor/UIElements/3d-volume-editor.uss";

    RootElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Vector3Styles));
    RootElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(VolumeEditorStyles));

    string[] Dims = new string[] { "Width", "Height", "Depth" };
    string[] Axis = new string[] { "X",     "Y",      "Z"     };

    RootElement.Add(new   VectorThreeIntField("Dimensions", Dims, 1,        5, Volume.Dimensions   ));
    RootElement.Add(new VectorThreeFloatField("Density",    Dims, 0.0001f,  1, Volume.Density      ));
    RootElement.Add(new VectorThreeFloatField("Offset",     Axis, 0.0001f, 10, Volume.Offset       ));
    RootElement.Add(new VectorThreeFloatField("Perturb",    Axis, 0.0001f,  1, Volume.PerturbFactor));

    return RootElement;
  }
}