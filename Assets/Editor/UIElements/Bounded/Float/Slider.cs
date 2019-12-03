using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SliderFloat = UnityEngine.UIElements.Slider;

namespace MayB.Games.UI.Elements.Bounded.Float {
  public class Slider : VisualElement {
    
    public float Value;

    private SliderFloat Range;
    private FloatField Min, Current, Max;
    private EventCallback<float> Callback;

    public Slider(float val, float min, float max, EventCallback<float> cb) {
      Callback = cb;
      Value    = val;

      contentContainer.AddToClassList("bounded-float");
      contentContainer.AddToClassList("range");

      Range = new SliderFloat {
        highValue = max,
        lowValue  = min,
        value     = val
      };

      Min     = new FloatField { value = min };
      Current = new FloatField { value = val };
      Max     = new FloatField { value = max };

      Min.AddToClassList("min-value");
      Current.AddToClassList("current-value");
      Max.AddToClassList("max-value");

      var Fields = new VisualElement();

      Fields.AddToClassList("input-fields");

      Fields.Add(Min);
      Fields.Add(Current);
      Fields.Add(Max);

      contentContainer.Add(Range);
      contentContainer.Add(Fields);

      Range.RegisterValueChangedCallback(CurrentChanged);
      Min.RegisterValueChangedCallback(MinChanged);
      Current.RegisterValueChangedCallback(CurrentChanged);
      Max.RegisterValueChangedCallback(MaxChanged);
    }

    private void CurrentChanged(ChangeEvent<float> evt) {
      Value = evt.newValue;

      Current.value = Value;
      Range.value   = Value;

      Callback(Value);
    }

    private void MinChanged(ChangeEvent<float> evt) => Range.lowValue = evt.newValue;

    private void MaxChanged(ChangeEvent<float> evt) => Range.highValue = evt.newValue;
  }
}