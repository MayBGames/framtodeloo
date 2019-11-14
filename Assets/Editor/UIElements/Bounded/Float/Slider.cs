using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SliderFloat = UnityEngine.UIElements.Slider;

namespace MayB.Games.UI.Elements.Bounded.Float {
  public class Slider : VisualElement {
    private const string UssClass = "bounded-int range";

    private Label Display;
    private SliderFloat Range;
    private FloatField Min, Max;

    private EventCallback<ChangeEvent<float>> Callback;

    private float Value;

    public Slider(float val, float min, float max, EventCallback<ChangeEvent<float>> cb) {
      Callback = cb;
      Value    = val;

      contentContainer.AddToClassList(UssClass);

      Display = new Label { text = val.ToString() };

      Range = new SliderFloat {
        highValue = max,
        lowValue  = min,
        value     = val
      };

      Min = new FloatField { value = min };
      Max = new FloatField { value = max };

      Range.RegisterValueChangedCallback(DisplayChanged);
      Min.RegisterValueChangedCallback(MinChanged);
      Max.RegisterValueChangedCallback(MaxChanged);
      
      contentContainer.Add(Display);
      contentContainer.Add(Range);
      contentContainer.Add(Min);
      contentContainer.Add(Max);
    }

    private void DisplayChanged(ChangeEvent<float> evt) {
      Value = evt.newValue;

      Display.text = Value.ToString();

      Callback(evt);
    }

    private void MinChanged(ChangeEvent<float> evt) => Range.lowValue = evt.newValue;

    private void MaxChanged(ChangeEvent<float> evt) => Range.highValue = evt.newValue;
  }
}