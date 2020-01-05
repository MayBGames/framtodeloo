using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class Sparse3DVolume {
  private static int[][] geometryVertices = new int[][] {
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
  private static int[][] sampleIndexes = new int[][] {
    new int[] {0, 2, 3, 7},
    new int[] {0, 2, 7, 6},
    new int[] {0, 4, 6, 7},
    new int[] {0, 6, 1, 2},
    new int[] {0, 4, 1, 6},
    new int[] {5, 6, 1, 4}
  };
  
  public Vector3 Resolution;
  public Vector3 Scale;
  public Vector3 Offset;
  public float Min;
  public float Frequency;
  
  private List<Vector3> Vertices = new List<Vector3>();
  private List<int> Triangles = new List<int>();

  public Mesh SparseBuildMesh() {
    Vertices.Clear();
    Triangles.Clear();

    for (float x = 0f; x < Resolution.x; x++)
      for (float y = 0f; y < Resolution.y; y++)
        for (float z = 0f; z < Resolution.z; z++)
          GenerateSamplePoints(new Vector3(x, y, z));

    Mesh mesh = new Mesh { name = $"Mesh" };

    mesh.vertices  = Vertices.ToArray();
    mesh.triangles = Triangles.ToArray();

    mesh.Optimize();
    mesh.RecalculateNormals();

    return mesh;
  }

  private void GenerateSamplePoints(Vector3 sample) {
    float row    = sample.x + Offset.x;
    float column = sample.y + Offset.y;
    float isle   = sample.z + Offset.z;

    float nextRow    = row    + 1f;
    float nextColumn = column + 1f;
    float nextIsle   = isle   + 1f;

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

    float[] values = new float[8];

    for (int i = 0; i < 8; i++)
      values[i] = Noise.SimplexGradient3D(
        points[i],
        Frequency
      ).value;
    
    Polygonize(points, values);
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
    for (int i = 0; i < indexes.Length; ++i)
      BuildVertex(
        values   [indexes[  i    ]],
        values   [indexes[  i + 1]],
        positions[indexes[  i    ]],
        positions[indexes[++i    ]]
      );
  }

  public void BuildVertex(float v0, float v1, Vector3 p0, Vector3 p1) {
    Vector3 p = p0 + (p1 - p0) / (v1 - v0) * (Min - v0);

    p.x *= Scale.x;
    p.y *= Scale.y;
    p.z *= Scale.z;
    
    Vertices.Add(p);

    Triangles.Add(Triangles.Count);
  }
}