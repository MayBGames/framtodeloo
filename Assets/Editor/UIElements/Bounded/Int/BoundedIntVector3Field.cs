using System;

using UnityEngine;

using UnityEngine.UIElements;

namespace MayB.Games.UI.Elements.Bounded.Int {
  public class VectorThree : VisualElement {
    private Vector3 Source;

    private Label Property;

    private string[] Labels = new string[3];
    private Slider[] Sliders = new Slider[3];
    private VisualElement[] Buttons = new VisualElement[3];
    private Action[] ButtonClickCallbacks = new Action[3];
    private EventCallback<int>[] SliderValueChangeCallbacks = new EventCallback<int>[3];

    public VectorThree(string prop, string[] labels, int min, int max, Vector3 src) {
      string property               = prop.ToLower().Trim();
      string PropertFieldDescriptor = $"{property}-bounded-int-vector3";
      
      ButtonClickCallbacks = new Action[] {
        XButtonClicked,
        YButtonClicked,
        ZButtonClicked
      };

      SliderValueChangeCallbacks = new EventCallback<int>[] {
        XValueChanged,
        YValueChanged,
        ZValueChanged
      };

      Source = src;

      contentContainer.name = PropertFieldDescriptor;

      contentContainer.AddToClassList(property);
      contentContainer.AddToClassList("bounded-int");
      contentContainer.AddToClassList("vector3"); 

      Property = new Label {
        name = $"{PropertFieldDescriptor}-header",
        text = prop
      };

      for (int i = 0; i < 3; i++) {
        Labels[i] = labels[i];

        Sliders[i] = new Slider(Source[i], min, max, SliderValueChangeCallbacks[i]);

        Buttons[i] = new VisualElement();
        Buttons[i].Add(new Button {
          name = $"{PropertFieldDescriptor}-{Labels[i].ToLower()}-tab",
          text = FormatButtonText(i)
        });

        Buttons[i].AddToClassList("tab");

        ((Button) Buttons[i].ElementAt(0)).clicked += ButtonClickCallbacks[i];
      }

      var TabBar = new VisualElement {
        name = $"{PropertFieldDescriptor}-tab-bar"
      };

      TabBar.AddToClassList("tab-bar");

      foreach (var btn in Buttons)
        TabBar.Add(btn);
      
      contentContainer.Add(Property);
      contentContainer.Add(TabBar);

      foreach (var slider in Sliders)
        contentContainer.Add(slider);
    }

    private string FormatButtonText(int i) => $"{Labels[i]}\n{Sliders[i].Value}";

    private void Activate(int i) {
      for (int s = 0; s < Sliders.Length; s++)
        Sliders[s].EnableInClassList("active", s == i);
      
      for (int b = 0; b < Buttons.Length; b++) {
        Buttons[b].EnableInClassList("active",        b == i);
        Buttons[b].EnableInClassList("right-rounded", b == i - 1);
        Buttons[b].EnableInClassList("left-rounded",  b == i + 1);

        ((Button) Buttons[b].ElementAt(0)).text = FormatButtonText(b);
      }
    }

    private void XButtonClicked() => Activate(0);

    private void YButtonClicked() => Activate(1);

    private void ZButtonClicked() => Activate(2);

    private void XValueChanged(int val) {
      ((Button) Buttons[0].ElementAt(0)).text = FormatButtonText(0);
      
      Source.x = val;
    }

    private void YValueChanged(int val) {
      ((Button) Buttons[1].ElementAt(0)).text = FormatButtonText(1);
      
      Source.y = val;
    }

    private void ZValueChanged(int val) {
      ((Button) Buttons[2].ElementAt(0)).text = FormatButtonText(2);
      
      Source.z = val;
    }
  }
}