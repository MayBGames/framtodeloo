using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class _3DVolume : MonoBehaviour {
  public GameObject prefab;
  public Vector3 Dimensions = new Vector3(5, 5, 5);
  public Vector3 Chunks = new Vector3(5, 5, 5);
  [Range(0.00001f, 1f)] public float Min = 0.1f;
  [Range(0.1f, 20f)] public float Frequency = 1f;
  [Range(0.1f, 3f)] public float Density = 1f;
  [Range(1, 10)] public int Size = 1;
  [Range(0.0001f, 0.005f)] public float ScaleFactor = 0.001f;
  public Vector3 Offset = Vector3.one;

  private List<GameObject> volumes = new List<GameObject>();

  public void BuildMesh() {
    double startTime = EditorApplication.timeSinceStartup;
    
    float scaledFrequency = Frequency * ScaleFactor;
    
    Vector3 offset = (Chunks * 0.5f) + (Dimensions * Density * 0.5f);

    foreach (var volume in volumes)
      DestroyImmediate(volume);

    volumes.Clear();

    int passes = 0;
    
    for (int _x = 0; _x < Chunks.x; _x++) {
      float chunkX = _x * (Dimensions.x * Density);
      
      for (int _y = 0; _y < Chunks.y; _y++) {
        float chunkY = _y * (Dimensions.y * Density);
        
        for (int _z = 0; _z < Chunks.z; _z++) {
          float chunkZ = _z * (Dimensions.z * Density);
          
          List<Vector3> vertices  = new List<Vector3>();
          List<int>     triangles = new List<int>();
          
          for (float x = 0; x < Dimensions.x; x += Density) {
            float row = x + chunkX;
            
            for (float y = 0; y < Dimensions.y; y += Density) {
              float column = y + chunkY;
              
              for (float z = 0; z < Dimensions.z; z += Density) {
                float isle = z + chunkZ;
                
                #region Samples
                  Vector3 _0 = new Vector3(
                    row          ,
                    column          ,
                    isle + Density
                  );
                  
                  Vector3 _1 = new Vector3(
                    row + Density,
                    column          ,
                    isle + Density
                  );

                  Vector3 _2 = new Vector3(
                    row + Density,
                    column          ,
                    isle
                  );

                  Vector3 _3 = new Vector3(
                    row,
                    column,
                    isle
                  );

                  Vector3 _4 = new Vector3(
                    row          ,
                    column + Density,
                    isle + Density
                  );

                  Vector3 _5 = new Vector3(
                    row + Density,
                    column + Density,
                    isle + Density
                  );

                  Vector3 _6 = new Vector3(
                    row + Density,
                    column + Density,
                    isle
                  );

                  Vector3 _7 = new Vector3(
                    row          ,
                    column + Density,
                    isle
                  );
                #endregion
                
                float __0 = Noise.SimplexGradient3D(_0, scaledFrequency).value;
                float __1 = Noise.SimplexGradient3D(_1, scaledFrequency).value;
                float __2 = Noise.SimplexGradient3D(_2, scaledFrequency).value;
                float __3 = Noise.SimplexGradient3D(_3, scaledFrequency).value;
                float __4 = Noise.SimplexGradient3D(_4, scaledFrequency).value;
                float __5 = Noise.SimplexGradient3D(_5, scaledFrequency).value;
                float __6 = Noise.SimplexGradient3D(_6, scaledFrequency).value;
                float __7 = Noise.SimplexGradient3D(_7, scaledFrequency).value;

                int verticesAdded = 0;
                int startAddingAt = triangles.Count;

                _0 *= Size;
                _1 *= Size;
                _2 *= Size;
                _3 *= Size;
                _4 *= Size;
                _5 *= Size;
                _6 *= Size;
                _7 *= Size;

                verticesAdded += Polygonize(ref vertices, __0, __2, __3, __7, _0, _2, _3, _7);
                verticesAdded += Polygonize(ref vertices, __0, __2, __7, __6, _0, _2, _7, _6);
                verticesAdded += Polygonize(ref vertices, __0, __4, __6, __7, _0, _4, _6, _7);
                verticesAdded += Polygonize(ref vertices, __0, __6, __1, __2, _0, _6, _1, _2);
                verticesAdded += Polygonize(ref vertices, __0, __4, __1, __6, _0, _4, _1, _6);
                verticesAdded += Polygonize(ref vertices, __5, __6, __1, __4, _5, _6, _1, _4);

                for (int i = 0; i < verticesAdded; i++)
                  triangles.Add(startAddingAt + i);

                ++passes;
              }
            }
          }

          Mesh mesh = new Mesh {
            name      = $"Mesh x:{_x}, y:{_y}, z:{_z}",
            vertices  = vertices.ToArray(),
            triangles = triangles.ToArray()
          };

          mesh.RecalculateNormals();

          GameObject obj = Instantiate(prefab, transform);

          obj.name = $"Chunk x:{_x}, y:{_y}, z:{_z}";
          
          obj.GetComponent<MeshFilter>().sharedMesh = mesh;

          volumes.Add(obj);
        }
      }
    }

    double executionTime        = EditorApplication.timeSinceStartup - startTime;
    double roundedExecutionTime = System.Math.Round(executionTime, 2);

    Debug.Log($"Passes: {passes}, Time: {roundedExecutionTime} seconds");
  }

  private int Polygonize(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    int mask = 0;

    if (v0 > Min) mask |= 1;
    if (v1 > Min) mask |= 2;
    if (v2 > Min) mask |= 4;
    if (v3 > Min) mask |= 8;

    switch(mask) {
      case 0x00:
      case 0x0F:
        return 0;
      case 0x0E: return x0E(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x01: return x01(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x0D: return x0D(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x02: return x02(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x0C: return x0C(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x03: return x03(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x0B: return x0B(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x04: return x04(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x0A: return x0A(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x05: return x05(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x09: return x09(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x06: return x06(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x07: return x07(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
      case 0x08: return x08(ref v, v0, v1, v2, v3, p0, p1, p2, p3);
    }
    
    return 0;
  }

  private int x0E(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v0, v3, p0, p3));
    v.Add(BuildVertex(v0, v2, p0, p2));

    return 3;
  }

  private int x01(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v0, v2, p0, p2));
    v.Add(BuildVertex(v0, v3, p0, p3));

    return 3;
  }

  private int x0D(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v1, v0, p1, p0));
    v.Add(BuildVertex(v1, v2, p1, p2));
    v.Add(BuildVertex(v1, v3, p1, p3));

    return 3;
  }

  private int x02(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v1, v0, p1, p0));
    v.Add(BuildVertex(v1, v3, p1, p3));
    v.Add(BuildVertex(v1, v2, p1, p2));

    return 3;
  }

  private int x0C(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v3, p0, p3));
    v.Add(BuildVertex(v0, v2, p0, p2));
    v.Add(BuildVertex(v1, v2, p1, p2));
    v.Add(BuildVertex(v0, v3, p0, p3));
    v.Add(BuildVertex(v1, v2, p1, p2));
    v.Add(BuildVertex(v1, v3, p1, p3));

    return 6;
  }

  private int x03(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v3, p0, p3));
    v.Add(BuildVertex(v1, v2, p1, p2));
    v.Add(BuildVertex(v0, v2, p0, p2));
    v.Add(BuildVertex(v0, v3, p0, p3));
    v.Add(BuildVertex(v1, v3, p1, p3));
    v.Add(BuildVertex(v1, v2, p1, p2));

    return 6;
  }

  private int x0B(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v2, v0, p2, p0));
    v.Add(BuildVertex(v2, v3, p2, p3));
    v.Add(BuildVertex(v2, v1, p2, p1));

    return 3;
  }

  private int x04(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v2, v0, p2, p0));
    v.Add(BuildVertex(v2, v1, p2, p1));
    v.Add(BuildVertex(v2, v3, p2, p3));

    return 3;
  }

  private int x0A(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v0, v3, p0, p3));
    v.Add(BuildVertex(v2, v3, p2, p3));
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v2, v3, p2, p3));
    v.Add(BuildVertex(v1, v2, p1, p2));

    return 6;
  }

  private int x05(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v2, v3, p2, p3));
    v.Add(BuildVertex(v0, v3, p0, p3));
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v1, v2, p1, p2));
    v.Add(BuildVertex(v2, v3, p2, p3));

    return 6;
  }

  private int x09(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v2, v3, p2, p3));
    v.Add(BuildVertex(v1, v3, p1, p3));
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v0, v2, p0, p2));
    v.Add(BuildVertex(v2, v3, p2, p3));

    return 6;
  }

  private int x06(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v1, v3, p1, p3));
    v.Add(BuildVertex(v2, v3, p2, p3));
    v.Add(BuildVertex(v0, v1, p0, p1));
    v.Add(BuildVertex(v2, v3, p2, p3));
    v.Add(BuildVertex(v0, v2, p0, p2));

    return 6;
  }

  private int x07(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v3, v0, p3, p0));
    v.Add(BuildVertex(v3, v1, p3, p1));
    v.Add(BuildVertex(v3, v2, p3, p2));

    return 3;
  }

  private int x08(
    ref List<Vector3> v,
    float   v0, float   v1, float   v2, float   v3,
    Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3
  ) {
    v.Add(BuildVertex(v3, v0, p3, p0));
    v.Add(BuildVertex(v3, v2, p3, p2));
    v.Add(BuildVertex(v3, v1, p3, p1));

    return 3;
  }

  private Vector3 BuildVertex(
    float   v0, float   v1,
    Vector3 p0, Vector3 p1
  ) {
    if (Mathf.Abs(Min - v0) < 0.0000001f || Mathf.Abs(v0 - v1) < 0.0000001f)
      return p0;
   
    if (Mathf.Abs(Min - v1) < 0.0000001f)
      return p1;
   
    float mu = (Min - v0) / (v1 - v0);
   
    float x = p0.x + mu * (p1.x - p0.x);
    float y = p0.y + mu * (p1.y - p0.y);
    float z = p0.z + mu * (p1.z - p0.z);

    return new Vector3(x, y, z);
  }
}