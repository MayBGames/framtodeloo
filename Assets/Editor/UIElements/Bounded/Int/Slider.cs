using UnityEngine;

using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MayB.Games.UI.Elements.Bounded.Int {
  public class Slider : VisualElement {
    
    public int Value;

    private SliderInt Range;
    private IntegerField Min, Current, Max;
    private EventCallback<int> Callback;

    public Slider(int val, int min, int max, EventCallback<int> cb) {
      Callback = cb;
      Value    = val;

      contentContainer.AddToClassList("bounded-int");
      contentContainer.AddToClassList("range");

      Range = new SliderInt {
        highValue = max,
        lowValue  = min,
        value     = val
      };

      Min     = new IntegerField { value = min };
      Current = new IntegerField { value = val };
      Max     = new IntegerField { value = max };

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

    public Slider(float val, int min, int max, EventCallback<int> cb) :
      this(Mathf.RoundToInt(val), min, max, cb) { }

    private void CurrentChanged(ChangeEvent<int> evt) {
      Value = evt.newValue;

      Current.value = Value;
      Range.value   = Value;

      Callback(Value);
    }

    private void MinChanged(ChangeEvent<int> evt) => Range.lowValue = evt.newValue;

    private void MaxChanged(ChangeEvent<int> evt) => Range.highValue = evt.newValue;
  }
}