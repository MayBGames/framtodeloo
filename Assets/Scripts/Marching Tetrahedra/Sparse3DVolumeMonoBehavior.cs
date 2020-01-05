using System;
using UnityEngine;

[Serializable]
public class Sparse3DVolumeMonoBehavior : MonoBehaviour {
  public BoundedFloatRange ResolutionRange;
  public BoundedFloatRange ScaleRange;
  public BoundedFloatRange OffsetRange;
  public BoundedFloatRange FrequencyRange;
  public BoundedFloatRange MinRange;
  public Sparse3DVolume Volume = new Sparse3DVolume();
}