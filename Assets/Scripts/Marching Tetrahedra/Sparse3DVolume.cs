using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using UnityEditor;

using Unity.Burst;

[ExecuteInEditMode]
[BurstCompile]
public class Sparse3DVolume : MonoBehaviour {
  public GameObject prefab;
  public bool CalculateNormals = false;
  public bool SkipEmptyAndFull = false;
  public Vector3 Dimensions = new Vector3(5f, 5f, 5f);
  public Vector3 Chunks = new Vector3(5f, 5f, 5f);
  public Vector3 Density = Vector3.one;
  public Vector3 Offset = Vector3.one;
  public Vector3 PerturbFactor = Vector3.one;
  public Vector3 PerturbFrequency = new Vector3(1000f, 1000f, 1000f);
  public Vector3 PerturbScale = new Vector3(0.005f, 0.005f, 0.005f);
  [Range(0.00001f, 1f)] public float Min = 0.1f;
  [Range(0.001f, 1000f)] public float Frequency = 250f;
  [Range(0.00001f, 0.1f)] public float ScaleFactor = 0.005f;
  public bool DrawMesh = false;
  public bool DrawDebugPoints = false;
  [Range(0.1f, 0.5f)] public float DebugPointScale = 0.25f;
  [Range(0f, 1f)] public float DebugPointsAlpha = 0.5f;
  public Color DupDebugPointColor;
  public bool ShowPointDups = false;
  public bool EnableCollider = false;
  [HideInInspector] public float ScaledFrequency = 1f;
  private int[][] geometryVertices = new int[][] {
    new int[] {0, 1, 0, 2, 0, 3},
    new int[] {1, 0, 1, 3, 1, 2},
    new int[] {0, 3, 1, 2, 0, 2, 0, 3, 1, 3, 1, 2},
    new int[] {2, 0, 2, 1, 2, 3},
    new int[] {0, 1, 2, 3, 0, 3, 0, 1, 1, 2, 2, 3},
    new int[] {0, 1, 1, 3, 2, 3, 0, 1, 2, 3, 0, 2},
    new int[] {3, 0, 3, 1, 3, 2},
    new int[] {3, 0, 3, 2, 3, 1},
    new int[] {0, 1, 2, 3, 1, 3, 0, 1, 0, 2, 2, 3},
    new int[] {0, 1, 0, 3, 2, 3, 0, 1, 2, 3, 1, 2},
    new int[] {2, 0, 2, 3, 2, 1},
    new int[] {0, 3, 0, 2, 1, 2, 0, 3, 1, 2, 1, 3},
    new int[] {1, 0, 1, 2, 1, 3},
    new int[] {0, 1, 0, 3, 0, 2}
  };
  private int[][] sampleIndexes = new int[][] {
    new int[] {0, 2, 3, 7},
    new int[] {0, 2, 7, 6},
    new int[] {0, 4, 6, 7},
    new int[] {0, 6, 1, 2},
    new int[] {0, 4, 1, 6},
    new int[] {5, 6, 1, 4}
  };
  [HideInInspector] public List<Vector3> Vertices = new List<Vector3>();
  private List<int> Triangles = new List<int>();
  private List<Vector4> DebugPoints = new List<Vector4>();
  private List<Color> DebugPointColors = new List<Color>();
  private List<Vector3> DrawnDebugPoints = new List<Vector3>();
  private Vector3 RenderDensity;
  private Vector3 Samples;
  private int PreviouslyRenderedLayer = -1;
  private int LayerCurrentlyRendering = 0;
  private bool Rendering = false;

  private void OnDrawGizmosSelected() {
    if (DrawDebugPoints) {
      for (int p = 0; p < DebugPoints.Count; p++) {
        float size = DebugPoints[p].w * DebugPointScale;

        float x = DebugPoints[p].x;
        float y = DebugPoints[p].y;
        float z = DebugPoints[p].z;

        Vector3 point = new Vector3(x, y, z);

        bool drawn = false;

        if (DrawnDebugPoints.Contains(point) && ShowPointDups) {
          Gizmos.color = DupDebugPointColor;

          Gizmos.DrawSphere(point, size);

          drawn = true;
        } else {
          Gizmos.color = DebugPointColors[p];
          
          DrawnDebugPoints.Add(point);
        }

        if (drawn == false)
          Gizmos.DrawSphere(point, size);
      }
    }
  }

  private void OnValidate() {
    if (Rendering == false)
      SparseBuildMesh();
  }

  public void SparseBuildMesh() {
    Rendering = true;

    Vertices.Clear();
    Triangles.Clear();
    DebugPoints.Clear();
    DebugPointColors.Clear();
    DrawnDebugPoints.Clear();

    RenderDensity   = new Vector3(1f / Density.x, 1f / Density.y, 1f / Density.z);
    ScaledFrequency = ScaleFactor * Frequency;
    Samples         = new Vector3(
      Dimensions.x * RenderDensity.x,
      Dimensions.y * RenderDensity.y,
      Dimensions.z * RenderDensity.z
    );
    
    double startTime = EditorApplication.timeSinceStartup;

    Traverse(Vector3.zero, Chunks, Samples, delegate(float x, float y, float z) {
      RenderChunk(new Vector3(x, y, z));
    });

    #region Mesh Update
      if (DrawMesh) {
        Mesh mesh = new Mesh { name = $"Mesh" };

        mesh.vertices  = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();

        mesh.Optimize();

        if (CalculateNormals)
          mesh.RecalculateNormals();
        
        GetComponent<MeshFilter>().sharedMesh   = mesh;

        if (EnableCollider)
          GetComponent<MeshCollider>().sharedMesh = mesh;

        double executionTime        = EditorApplication.timeSinceStartup - startTime;
        double roundedExecutionTime = System.Math.Round(executionTime, 2);
      } else
        GetComponent<MeshFilter>().sharedMesh = null;
    #endregion
  
    Rendering = false;
  }

  private void RenderChunk(Vector3 chunk) {
    Traverse(chunk, Samples, RenderDensity, delegate(float x, float y, float z) {
      PerturbSamplePoints(new Vector3(x, y, z), RenderDensity);
    });
  }

  private void Traverse(Vector3 origin, Vector3 extent, Vector3 density, Action<float, float, float> action) {
    float startX = origin.x;
    float startY = origin.y;
    float startZ = origin.z;

    float endX = startX + (extent.x * density.x);
    float endY = startY + (extent.y * density.y);
    float endZ = startZ + (extent.z * density.z);
    
    for (float x = startX; x < endX; x += density.x)
      for (float y = startY; y < endY; y += density.y)
        for (float z = startZ; z < endZ; z += density.z)
          action(x, y, z);
  }

  private void PerturbSamplePoints(Vector3 sample, Vector3 step) {
    float nextRow    = sample.x + Offset.x + step.x;
    float nextColumn = sample.y + Offset.y + step.y;
    float nextIsle   = sample.z + Offset.z + step.z;

    float row    = sample.x + Offset.x;
    float column = sample.y + Offset.y;
    float isle   = sample.z + Offset.z;

    Vector3[] points = new Vector3[] {
      new Vector3(    row,     column, nextIsle),
      new Vector3(nextRow,     column, nextIsle),
      new Vector3(nextRow,     column,     isle),
      new Vector3(    row,     column,     isle),
      new Vector3(    row, nextColumn, nextIsle),
      new Vector3(nextRow, nextColumn, nextIsle),
      new Vector3(nextRow, nextColumn,     isle),
      new Vector3(    row, nextColumn,     isle)
    };

    Vector3[] perturbed = new Vector3[] {
      PerturbPoint(points[0]),
      PerturbPoint(points[1]),
      PerturbPoint(points[2]),
      PerturbPoint(points[3]),
      PerturbPoint(points[4]),
      PerturbPoint(points[5]),
      PerturbPoint(points[6]),
      PerturbPoint(points[7])
    };

    float[] values = new float[] {
      Noise.SimplexGradient3D(points[0], ScaledFrequency).value,
      Noise.SimplexGradient3D(points[1], ScaledFrequency).value,
      Noise.SimplexGradient3D(points[2], ScaledFrequency).value,
      Noise.SimplexGradient3D(points[3], ScaledFrequency).value,
      Noise.SimplexGradient3D(points[4], ScaledFrequency).value,
      Noise.SimplexGradient3D(points[5], ScaledFrequency).value,
      Noise.SimplexGradient3D(points[6], ScaledFrequency).value,
      Noise.SimplexGradient3D(points[7], ScaledFrequency).value
    };
    
    Polygonize(perturbed, values);
  }

  private Vector3 PerturbPoint(Vector3 point) {
    Vector3 pp = point + PerturbFactor;

    float psx = PerturbFrequency.x * PerturbScale.x * ScaledFrequency;
    float psy = PerturbFrequency.y * PerturbScale.y * ScaledFrequency;
    float psz = PerturbFrequency.z * PerturbScale.z * ScaledFrequency;

    float x = point.x + Noise.SimplexGradient3D(pp, psx).value;
    float y = point.y + Noise.SimplexGradient3D(pp, psy).value;
    float z = point.z + Noise.SimplexGradient3D(pp, psz).value;

    Vector3 ret = new Vector3(x, y, z);
    
    return ret;
  }

  public void Polygonize(Vector3[] points, float[] values) {
    for (int s = 0; s < 6; s++) {
      int mask = GenerateMask(sampleIndexes[s], values);

      if (mask > 0 && mask < 15) {
        int[] indexesToSample = sampleIndexes[s];

        Vector3[] samplePoints = new Vector3[4];
        float[]   sampleValues = new float[4];

        for (int i = 0; i < 4; i ++) {
          int sample = indexesToSample[i];
          
          samplePoints[i] = points[sample];
          sampleValues[i] = values[sample];
        }

        Geometry(samplePoints, sampleValues, geometryVertices[--mask]);

        if (DrawDebugPoints) {
          DebugPoints.Add(new Vector4(samplePoints[0].x, samplePoints[0].y, samplePoints[0].z, values[0]));
          DebugPointColors.Add(new Color(values[1], values[2], values[3], DebugPointsAlpha));
        }
      }
    }
  }

  private int GenerateMask(int[] keys, float[] v) {
    int mask = 0;

    if (v[keys[0]] > Min) mask |= 1;
    if (v[keys[1]] > Min) mask |= 2;
    if (v[keys[2]] > Min) mask |= 4;
    if (v[keys[3]] > Min) mask |= 8;

    return mask;
  }

  public void Geometry(Vector3[] positions, float[] values, int[] indexes) {
    for (int i = 0; i < indexes.Length; ++i) {
      int current = indexes[  i];
      int next    = indexes[++i];

      BuildVertex(
        values[current],
        values[next],
        positions[current],
        positions[next]
      );
    }
  }

  public void BuildVertex(float v0, float v1, Vector3 p0, Vector3 p1) {
    float bigX = p1.x > p0.x ? p1.x : p0.x;
    float bigY = p1.y > p0.y ? p1.y : p0.y;
    float bigZ = p1.z > p0.z ? p1.z : p0.z;

    float smallX = p1.x < p0.x ? p1.x : p0.x;
    float smallY = p1.y < p0.y ? p1.y : p0.y;
    float smallZ = p1.z < p0.z ? p1.z : p0.z;

    float deltaPoint = (v0 + v1) * 0.5f;

    float deltaX = Mathf.Lerp(smallX, bigX, deltaPoint);
    float deltaY = Mathf.Lerp(smallY, bigY, deltaPoint);
    float deltaZ = Mathf.Lerp(smallZ, bigZ, deltaPoint);

    Vertices.Add(new Vector3(deltaX, deltaY, deltaZ));

    Triangles.Add(Triangles.Count);
  }
}