using UnityEngine;

using UnityEngine.UIElements;

namespace MayB.Games.UI.Elements.Bounded.Float {
  public class VectorThree : VisualElement {
    private Vector3 Source;

    public VectorThree(string prop, float min, float max, Vector3 src) {
      string property               = prop.ToLower().Trim();
      string PropertFieldDescriptor = $"{property}-bounded-float-vector3";

      Source = src;

      contentContainer.name = PropertFieldDescriptor;

      contentContainer.AddToClassList(property);
      contentContainer.AddToClassList("bounded-float");
      contentContainer.AddToClassList("vector3");


      var X = new Slider(Source.x, min, max, XValueChanged);
      var Y = new Slider(Source.y, min, max, YValueChanged);
      var Z = new Slider(Source.z, min, max, ZValueChanged);
      
      contentContainer.Add(new Label {
        name = $"{PropertFieldDescriptor}-header",
        text = prop
      });

      contentContainer.Add(X);
      contentContainer.Add(Y);
      contentContainer.Add(Z);
    }

    private void XValueChanged(ChangeEvent<float> evt) => Source.x = evt.newValue;

    private void YValueChanged(ChangeEvent<float> evt) => Source.y = evt.newValue;

    private void ZValueChanged(ChangeEvent<float> evt) => Source.z = evt.newValue;
  }
}