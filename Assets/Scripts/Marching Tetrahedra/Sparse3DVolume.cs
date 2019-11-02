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
  public Vector3 Samples = new Vector3(5f, 5f, 5f);
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
  public bool DrawPerturbedPoints = false;
  [Range(0.1f, 0.5f)] public float DebugPerturbedPointScale = 0.25f;
  [Range(0f, 1f)] public float DebugPerturbPointsAlpha = 0.5f;
  public Color DupDebugPerturbPointColor;
  public bool ShowPerturbedPointDups = false;
  public bool RenderByLayer = false;
  [HideInInspector] public float ScaledFrequency = 1f;
  private int[][] geometryVertices = new int[][] {
    new int[] { },
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
    new int[] {0, 1, 0, 3, 0, 2},
    new int[] { }
  };
  private int[][] sampleIndexes = new int[][] {
    new int[] { },
    new int[] {0, 2, 3, 7},
    new int[] {0, 2, 7, 6},
    new int[] {0, 4, 6, 7},
    new int[] {0, 6, 1, 2},
    new int[] {0, 4, 1, 6},
    new int[] {5, 6, 1, 4}
  };
  private List<Vector3> Vertices = new List<Vector3>();
  private List<int> Triangles = new List<int>();
  private List<Vector4> DebugPoints = new List<Vector4>();
  private List<Color> DebugPointColors = new List<Color>();
  private List<Vector4> DebugPerturbedPoints = new List<Vector4>();
  private List<Color> DebugPerturbedPointColors = new List<Color>();
  private List<Vector3> DrawnDebugPoints = new List<Vector3>();
  private List<Vector3> DrawnPerturbDebugPoints = new List<Vector3>();
  private Vector3 RenderDensity;
  private int PreviouslyRenderedLayer = -1;
  private int LayerCurrentlyRendering = 0;

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

    if (DrawPerturbedPoints) {      
      for (int p = 0; p < DebugPerturbedPoints.Count; p++) {
        
        float size = DebugPerturbedPoints[p].w * DebugPerturbedPointScale;

        float x = DebugPerturbedPoints[p].x;
        float y = DebugPerturbedPoints[p].y;
        float z = DebugPerturbedPoints[p].z;

        Vector3 point = new Vector3(x, y, z);

        bool drawn = false;
        
        if (DrawnPerturbDebugPoints.Contains(point) && ShowPerturbedPointDups) {
          Gizmos.color = DupDebugPerturbPointColor;

          Gizmos.DrawSphere(point, size);

          drawn = true;
        } else {
          Gizmos.color = DebugPerturbedPointColors[p];
          
          DrawnPerturbDebugPoints.Add(point);
        }

        if (drawn == false)
          Gizmos.DrawSphere(point, size);
      }
    }
  }

  private void OnValidate() =>
    SparseBuildMesh();

  public void SparseBuildMesh() {
    Vertices.Clear();
    Triangles.Clear();
    DebugPoints.Clear();
    DebugPointColors.Clear();
    DebugPerturbedPoints.Clear();
    DebugPerturbedPointColors.Clear();
    DrawnDebugPoints.Clear();
    DrawnPerturbDebugPoints.Clear();

    RenderDensity   = new Vector3(1f / Density.x, 1f / Density.y, 1f / Density.z);
    ScaledFrequency = ScaleFactor * Frequency;
    
    double startTime = EditorApplication.timeSinceStartup;

    Traverse(Vector3.zero, Chunks, Vector3.one, delegate(float x, float y, float z) {
      Vector3 denseChunk = new Vector3(x, y, z);
      
      RenderIfNeeded(SamplePoints(denseChunk, RenderDensity), delegate(Dictionary<Vector3, float> points, Dictionary<int[], int> renderMask) {
        RenderChunk(denseChunk, renderMask);
      });
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
        GetComponent<MeshCollider>().sharedMesh = mesh;

        double executionTime        = EditorApplication.timeSinceStartup - startTime;
        double roundedExecutionTime = System.Math.Round(executionTime, 2);
      } else
        GetComponent<MeshFilter>().sharedMesh = null;
    #endregion
  }

  private void RenderChunk(Vector3 chunk, Dictionary<int[], int> _) {
    Traverse(chunk, Samples, RenderDensity, delegate(float x, float y, float z) {
      RenderIfNeeded(PerturbSamplePoints(new Vector3(x, y, z), RenderDensity), delegate(Dictionary<Vector3, float> points, Dictionary<int[], int> renderMask) {
        int startAddingAt = Triangles.Count;
        
        for (int i = 0; i < Polygonize(points, renderMask); i++)
          Triangles.Add(startAddingAt + i);
      });
    });
  }

  private void RenderIfNeeded(Dictionary<Vector3, float> points, Action<Dictionary<Vector3, float>, Dictionary<int[], int>> action) {
    Dictionary<int[], int> renderMask;

    bool renderNeeded = NeedsRender(points, out renderMask);
    
    if (renderNeeded)
      action(points, renderMask);
  }

  private void Traverse(Vector3 origin, Vector3 extent, Vector3 density, Action<float, float, float> action) {
    float startX = origin.x * density.x;
    float startY = origin.y * density.y;
    float startZ = origin.z * density.z;

    float endX = density.x * extent.x + origin.x;
    float endY = density.y * extent.y + origin.y;
    float endZ = density.z * extent.z + origin.z;
    
    for (float x = startX; x < endX; x += density.x)
      for (float y = startY; y < endY; y += density.y)
        for (float z = startZ; z < endZ; z += density.z)
          action(x, y, z);
  }

  private bool NeedsRender(Dictionary<Vector3, float> points, out Dictionary<int[], int> renderMask) {
    Vector3[] p = new Vector3[8];
    float[]   v = new float[8];
    
    points.Keys.CopyTo(p, 0);
    points.Values.CopyTo(v, 0);

    renderMask = new Dictionary<int[], int>();

    for (int s = 0; s < sampleIndexes.Length; s++)
      if (sampleIndexes[s].Length > 0) {
        int mask = GenerateMask(sampleIndexes[s], v);

        if (mask > 0 && mask < 15)
          renderMask.Add(sampleIndexes[s], mask);
      }

    bool haveMask = renderMask.Count > 0;

    return haveMask;
  }

  private Dictionary<Vector3, float> PerturbSamplePoints(Vector3 sample, Vector3 step) {
    float nextRow    = sample.x + Offset.x + 1f;
    float nextColumn = sample.y + Offset.y + 1f;
    float nextIsle   = sample.z + Offset.z + 1f;

    float row    = sample.x + Offset.x;
    float column = sample.y + Offset.y;
    float isle   = sample.z + Offset.z;

    Vector3 p0 = PerturbPoint(new Vector3(    row,     column, nextIsle));
    Vector3 p1 = PerturbPoint(new Vector3(nextRow,     column, nextIsle));
    Vector3 p2 = PerturbPoint(new Vector3(nextRow,     column,     isle));
    Vector3 p3 = PerturbPoint(new Vector3(    row,     column,     isle));
    Vector3 p4 = PerturbPoint(new Vector3(    row, nextColumn, nextIsle));
    Vector3 p5 = PerturbPoint(new Vector3(nextRow, nextColumn, nextIsle));
    Vector3 p6 = PerturbPoint(new Vector3(nextRow, nextColumn,     isle));
    Vector3 p7 = PerturbPoint(new Vector3(    row, nextColumn,     isle));
    
    float offsetRow    = step.x * sample.x - (step.x * 0.5f * sample.y);
    float offsetColumn = step.y * sample.y - (step.y * 0.5f * sample.y);
    float offsetIsle   = step.z * sample.z - (step.z * 0.5f * sample.z);
    
    float posX = Math.Abs(step.x * 0.5f);
    float posY = Math.Abs(step.y * 0.5f);
    float posZ = Math.Abs(step.z * 0.5f);

    float negX = -posX;
    float negY = -posY;
    float negZ = -posZ;

    float posOffX = posX + offsetRow;
    float posOffY = posY + offsetColumn;
    float posOffZ = posZ + offsetIsle;
    
    float negOffX = negX + offsetRow;
    float negOffY = negY + offsetColumn;
    float negOffZ = negZ + offsetIsle;

    Vector3 perturbedP0 = new Vector3(p0.x + negOffX, p0.y + negOffY, p0.z + posOffZ);
    Vector3 perturbedP1 = new Vector3(p1.x + posOffX, p1.y + negOffY, p1.z + posOffZ);
    Vector3 perturbedP2 = new Vector3(p2.x + posOffX, p2.y + negOffY, p2.z + negOffZ);
    Vector3 perturbedP3 = new Vector3(p3.x + negOffX, p3.y + negOffY, p3.z + negOffZ);
    Vector3 perturbedP4 = new Vector3(p4.x + negOffX, p4.y + posOffY, p4.z + posOffZ);
    Vector3 perturbedP5 = new Vector3(p5.x + posOffX, p5.y + posOffY, p5.z + posOffZ);
    Vector3 perturbedP6 = new Vector3(p6.x + posOffX, p6.y + posOffY, p6.z + negOffZ);
    Vector3 perturbedP7 = new Vector3(p7.x + negOffX, p7.y + posOffY, p7.z + negOffZ);

    float n0 = Noise.SimplexGradient3D(p0, ScaledFrequency).value;
    float n1 = Noise.SimplexGradient3D(p1, ScaledFrequency).value;
    float n2 = Noise.SimplexGradient3D(p2, ScaledFrequency).value;
    float n3 = Noise.SimplexGradient3D(p3, ScaledFrequency).value;
    float n4 = Noise.SimplexGradient3D(p4, ScaledFrequency).value;
    float n5 = Noise.SimplexGradient3D(p5, ScaledFrequency).value;
    float n6 = Noise.SimplexGradient3D(p6, ScaledFrequency).value;
    float n7 = Noise.SimplexGradient3D(p7, ScaledFrequency).value;

    
    if (DrawPerturbedPoints) {
      if (DrawDebugPoints) {
        DebugPoints.Add(new Vector4(p3.x, p3.y, p3.z, n3));
        DebugPointColors.Add(new Color((sample.x / Samples.x) * 0.25f, (sample.y / Samples.y) * 0.25f, (sample.z / Samples.z) * 0.25f, DebugPointsAlpha));
      }
      
      DebugPerturbedPoints.Add(new Vector4(perturbedP3.x, perturbedP3.y, perturbedP3.z, n3));
      DebugPerturbedPointColors.Add(new Color(sample.z / Samples.z, sample.x / Samples.x, sample.y / Samples.y, DebugPerturbPointsAlpha));
    }
    
    return new Dictionary<Vector3, float> {
      [perturbedP0] = n0,
      [perturbedP1] = n1,
      [perturbedP2] = n2,
      [perturbedP3] = n3,
      [perturbedP4] = n4,
      [perturbedP5] = n5,
      [perturbedP6] = n6,
      [perturbedP7] = n7
    };
  }

  private Dictionary<Vector3, float> SamplePoints(Vector3 sample, Vector3 step) {
    float nextRow    = sample.x + Offset.x + 1f;
    float nextColumn = sample.y + Offset.y + 1f;
    float nextIsle   = sample.z + Offset.z + 1f;

    float row    = sample.x + Offset.x;
    float column = sample.y + Offset.y;
    float isle   = sample.z + Offset.z;

    Vector3 p0 = new Vector3(    row,     column, nextIsle);
    Vector3 p1 = new Vector3(nextRow,     column, nextIsle);
    Vector3 p2 = new Vector3(nextRow,     column,     isle);
    Vector3 p3 = new Vector3(    row,     column,     isle);
    Vector3 p4 = new Vector3(    row, nextColumn, nextIsle);
    Vector3 p5 = new Vector3(nextRow, nextColumn, nextIsle);
    Vector3 p6 = new Vector3(nextRow, nextColumn,     isle);
    Vector3 p7 = new Vector3(    row, nextColumn,     isle);

    float n0 = Noise.SimplexGradient3D(p0, ScaledFrequency).value;
    float n1 = Noise.SimplexGradient3D(p1, ScaledFrequency).value;
    float n2 = Noise.SimplexGradient3D(p2, ScaledFrequency).value;
    float n3 = Noise.SimplexGradient3D(p3, ScaledFrequency).value;
    float n4 = Noise.SimplexGradient3D(p4, ScaledFrequency).value;
    float n5 = Noise.SimplexGradient3D(p5, ScaledFrequency).value;
    float n6 = Noise.SimplexGradient3D(p6, ScaledFrequency).value;
    float n7 = Noise.SimplexGradient3D(p7, ScaledFrequency).value;
    
    if (DrawDebugPoints && DrawPerturbedPoints == false) {
      DebugPoints.Add(new Vector4(p3.x, p3.y, p3.z, n3));
      DebugPointColors.Add(new Color((sample.x / Samples.x) * 0.25f, (sample.y / Samples.y) * 0.25f, (sample.z / Samples.z) * 0.25f, DebugPointsAlpha));
    }
    
    return new Dictionary<Vector3, float> {
      [p0] = n0,
      [p1] = n1,
      [p2] = n2,
      [p3] = n3,
      [p4] = n4,
      [p5] = n5,
      [p6] = n6,
      [p7] = n7
    };
  }

  private float PerturbValue(Vector3 point) =>
    Noise.SimplexGradient3D(PerturbPoint(point), ScaledFrequency).value;

  private Vector3 PerturbPoint(Vector3 point) {
    float x = point.x + Noise.SimplexGradient3D(point + PerturbFactor, PerturbFrequency.x * PerturbScale.x * ScaledFrequency).value;
    float y = point.y + Noise.SimplexGradient3D(point + PerturbFactor, PerturbFrequency.y * PerturbScale.y * ScaledFrequency).value;
    float z = point.z + Noise.SimplexGradient3D(point + PerturbFactor, PerturbFrequency.z * PerturbScale.z * ScaledFrequency).value;

    Vector3 ret = new Vector3(x, y, z);
    
    return ret;
  }

  private int Polygonize(Dictionary<Vector3, float> points, Dictionary<int[], int> renderMask) {
    Vector3[] p = new Vector3[8];
    float[]   v = new float[8];
    
    points.Keys.CopyTo(p, 0);
    points.Values.CopyTo(v, 0);

    int verticesCreated = 0;

    foreach (var mask in renderMask) {
      Vector3[] positions = new Vector3[] {
        p[mask.Key[0]],
        p[mask.Key[1]],
        p[mask.Key[2]],
        p[mask.Key[3]]
      };

      float[] values = new float[] {
        v[mask.Key[0]],
        v[mask.Key[1]],
        v[mask.Key[2]],
        v[mask.Key[3]]
      };

      verticesCreated += Geometry(positions, values, geometryVertices[mask.Value]);
    }

    return verticesCreated;
  }

  private int GenerateMask(int[] keys, float[] v) {
    int mask = 0;

    if (v[keys[0]] > Min) mask |= 1;
    if (v[keys[1]] > Min) mask |= 2;
    if (v[keys[2]] > Min) mask |= 4;
    if (v[keys[3]] > Min) mask |= 8;

    return mask;
  }

  private int Geometry(Vector3[] p, float[] v, int[] indexes) {
    for (int i = 0; i < indexes.Length; ++i) {
      int current = indexes[  i];
      int next    = indexes[++i];
      
      Vertices.Add(BuildVertex(v[current], v[next], p[current], p[next]));
    }

    return indexes.Length / 2;
  }

  public Vector3 BuildVertex(float v0, float v1, Vector3 p0, Vector3 p1) {
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

    return new Vector3(deltaX, deltaY, deltaZ);
  }
}