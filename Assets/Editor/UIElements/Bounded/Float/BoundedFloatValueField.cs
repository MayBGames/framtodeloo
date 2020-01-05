using UnityEngine;

using UnityEngine.UIElements;

namespace MayB.Games.UI.Elements.Bounded.Float {
  public class Value : VisualElement {
    private Label Property;

    private string Label;
    private Slider Slider;
    private VisualElement[] Buttons = new VisualElement[3];

    public Value(
      string prop,
      float min,
      float max,
      float src,
      EventCallback<float> valueCB,
      EventCallback<float> minCB,
      EventCallback<float> maxCB
    ) {
      string property               = prop.ToLower().Trim();
      string PropertFieldDescriptor = $"{property}-bounded-float-value";

      contentContainer.name = PropertFieldDescriptor;

      contentContainer.AddToClassList(property);
      contentContainer.AddToClassList("bounded-float");
      contentContainer.AddToClassList("value"); 

      Property = new Label {
        name = $"{PropertFieldDescriptor}-header",
        text = prop
      };

      Slider = new Slider(src, min, max, delegate(float val) {
        valueCB(val);

        src = val;
      }, minCB, maxCB);
      
      contentContainer.Add(Property);
      contentContainer.Add(Slider);
    }
  }
}