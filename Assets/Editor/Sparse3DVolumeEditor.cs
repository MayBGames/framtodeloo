using UnityEngine;
using UnityEditor;

using UnityEngine.UIElements;

using VectorThreeFloatField = MayB.Games.UI.Elements.Bounded.Float.VectorThree;
using ValueFloatField       = MayB.Games.UI.Elements.Bounded.Float.Value;

[CustomEditor(typeof(Sparse3DVolumeMonoBehavior))]
public class Sparse3DVolumeEditor : Editor {

  private static string StyleRoot = "Assets/Editor/UIElements";
  private static string StyleName = "3d-volume-editor";

  private Sparse3DVolumeMonoBehavior MB;
  private VisualElement RootElement;

  public override VisualElement CreateInspectorGUI() {
    RootElement = new VisualElement { name = StyleName };

    MB = (Sparse3DVolumeMonoBehavior) target;

    string Vector3Styles      = $"{StyleRoot}/vector3.uss";
    string VolumeEditorStyles = $"{StyleRoot}/{StyleName}.uss";

    var Vector3StyleAsset      = AssetDatabase.LoadAssetAtPath<StyleSheet>(Vector3Styles);
    var VolumeEditorStyleAsset = AssetDatabase.LoadAssetAtPath<StyleSheet>(VolumeEditorStyles);

    RootElement.styleSheets.Add(Vector3StyleAsset);
    RootElement.styleSheets.Add(VolumeEditorStyleAsset);

    AddResolution();
    AddScale();
    AddOffset();
    AddFrequency();
    AddMin();    

    var RenderButton = new Button(delegate () {
      MB.GetComponent<MeshFilter>().sharedMesh = MB.Volume.SparseBuildMesh();
    });

    RenderButton.text = "Render";

    RootElement.Add(RenderButton);

    return RootElement;
  }

  private void AddResolution() {
    EventCallback<Vector3> resolutionValueCallback = delegate(Vector3 updated) { MB.Volume.Resolution   = updated; };
    EventCallback<float>   resolutionMinCallback   = delegate(float   updated) { MB.ResolutionRange.Min = updated; };
    EventCallback<float>   resolutionMaxCallback   = delegate(float   updated) { MB.ResolutionRange.Max = updated; };

    RootElement.Add(new VectorThreeFloatField(
      "Resolution",
      new string[] { "Width", "Height", "Depth" },
      MB.ResolutionRange.Min,
      MB.ResolutionRange.Max,
      MB.Volume.Resolution,
      resolutionValueCallback,
      resolutionMinCallback,
      resolutionMaxCallback
    ));
  }

  private void AddScale() {
    EventCallback<Vector3> scaleValueCallback = delegate(Vector3 updated) { MB.Volume.Scale   = updated; };
    EventCallback<float>   scaleMinCallback   = delegate(float   updated) { MB.ScaleRange.Min = updated; };
    EventCallback<float>   scaleMaxCallback   = delegate(float   updated) { MB.ScaleRange.Max = updated; };

    RootElement.Add(new VectorThreeFloatField(
      "Scale",
      new string[] { "X", "Y", "Z" },
      MB.ScaleRange.Min,
      MB.ScaleRange.Max,
      MB.Volume.Scale,
      scaleValueCallback,
      scaleMinCallback,
      scaleMaxCallback
    ));
  }

  private void AddOffset() {
    EventCallback<Vector3> offsetValueCallback = delegate(Vector3 updated) { MB.Volume.Offset   = updated; };
    EventCallback<float>   offsetMinCallback   = delegate(float   updated) { MB.OffsetRange.Min = updated; };
    EventCallback<float>   offsetMaxCallback   = delegate(float   updated) { MB.OffsetRange.Max = updated; };
    
    RootElement.Add(new VectorThreeFloatField(
      "Offset",
      new string[] { "X", "Y", "Z" },
      MB.OffsetRange.Min,
      MB.OffsetRange.Max,
      MB.Volume.Offset,
      offsetValueCallback,
      offsetMinCallback,
      offsetMaxCallback
    ));
  }

  private void AddFrequency() {
    EventCallback<float> frequencyValueCallback = delegate(float updated) { MB.Volume.Frequency   = updated; };
    EventCallback<float> frequencyMinCallback   = delegate(float updated) { MB.FrequencyRange.Min = updated; };
    EventCallback<float> frequencyMaxCallback   = delegate(float updated) { MB.FrequencyRange.Max = updated; };
    
    RootElement.Add(new ValueFloatField(
      "Frequency",
      MB.FrequencyRange.Min,
      MB.FrequencyRange.Max,
      MB.Volume.Frequency,
      frequencyValueCallback,
      frequencyMinCallback,
      frequencyMaxCallback
    ));
  }

  private void AddMin() {
    EventCallback<float> minValueCallback = delegate(float updated) { MB.Volume.Min   = updated; };
    EventCallback<float> minMinCallback   = delegate(float updated) { MB.MinRange.Min = updated; };
    EventCallback<float> minMaxCallback   = delegate(float updated) { MB.MinRange.Max = updated; };

    RootElement.Add(new ValueFloatField(
      "Min",
      MB.MinRange.Min,
      MB.MinRange.Max,
      MB.Volume.Min,
      minValueCallback,
      minMinCallback,
      minMaxCallback
    ));
  }
}