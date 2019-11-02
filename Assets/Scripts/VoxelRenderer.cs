using UnityEngine;

[ExecuteInEditMode]
public class VoxelRenderer : MonoBehaviour {
  [Range(2, 12)] public int Width = 3;
  [Range(2, 12)] public int Height = 3;
  [Range(2, 12)] public int Depth = 3;
}
