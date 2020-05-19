using System;

using UnityEngine;

using Unity.Mathematics;
using Unity.Collections;

[Serializable]
public struct Sparse3DVolume {  
  public float3 Resolution;
  public float3 Scale;
  public float3 Offset;
  public float Min;
  public float Frequency;

  public float3 Chunks;

  public NativeArray<int> GeometryVertexes;
  public NativeArray<int> IndexTracker;
  public NativeArray<int> SampleIndexes;

  public NativeArray<Vector3> Vertices;
  public NativeArray<int> Triangles;

  public NativeArray<Vector3> SimplexGradients3D;
  public NativeArray<int> PermutationTable;

  public int VertexCount;

  private const int hashMask = 255;
  private const int simplexGradientsMask3D = 31;
  private static readonly float simplexScale3D = 8192f * Mathf.Sqrt(3f) / 375f;

  public void SparseBuildMesh() {
    VertexCount = 0;
    
    for (float x = 0f; x < Resolution.x; x++)
      for (float y = 0f; y < Resolution.y; y++)
        for (float z = 0f; z < Resolution.z; z++)
          GenerateSamplePoints(new float3(x, y, z));
  }

  private void GenerateSamplePoints(float3 sample) {
    float row    = sample.x + Offset.x;
    float column = sample.y + Offset.y;
    float isle   = sample.z + Offset.z;

    float nextRow    = row    + 1f;
    float nextColumn = column + 1f;
    float nextIsle   = isle   + 1f;

    NativeArray<float3> points = new NativeArray<float3>(8, Allocator.Temp);

    points[0] = new float3(    row,     column, nextIsle);
    points[1] = new float3(nextRow,     column, nextIsle);
    points[2] = new float3(nextRow,     column,     isle);
    points[3] = new float3(    row,     column,     isle);
    points[4] = new float3(    row, nextColumn, nextIsle);
    points[5] = new float3(nextRow, nextColumn, nextIsle);
    points[6] = new float3(nextRow, nextColumn,     isle);
    points[7] = new float3(    row, nextColumn,     isle);

    NativeArray<float> values = new NativeArray<float>(8, Allocator.Temp);

    for (int i = 0; i < 8; i++)
      values[i] = SimplexGradient3D(
        points[i],
        Frequency
      ).value;
    
    Polygonize(points, values);

    points.Dispose();
    values.Dispose();
  }

  private void Polygonize(NativeArray<float3> points, NativeArray<float> values) {
    for (int s = 0; s < 6; s++) {
      int mask = GenerateMask(s * 4, values);

      if (mask > 0 && mask < 15) {
        NativeArray<float3> samplePoints = new NativeArray<float3>(4, Allocator.Temp);
        NativeArray<float>  sampleValues = new NativeArray<float>(4, Allocator.Temp);

        for (int i = 0; i < 4; i ++) {
          int sample = SampleIndexes[s * 4 + i];
          
          samplePoints[i] = points[sample];
          sampleValues[i] = values[sample];
        }

        Geometry(samplePoints, sampleValues, mask - 1);

        samplePoints.Dispose();
        sampleValues.Dispose();
      }
    }
  }

  private int GenerateMask(int startAt, NativeArray<float> v) {
    int mask = 0;

    if (v[SampleIndexes[startAt    ]] > Min) mask |= 1;
    if (v[SampleIndexes[startAt + 1]] > Min) mask |= 2;
    if (v[SampleIndexes[startAt + 2]] > Min) mask |= 4;
    if (v[SampleIndexes[startAt + 3]] > Min) mask |= 8;

    return mask;
  }

  private void Geometry(NativeArray<float3> positions, NativeArray<float> values, int index) {
    int startAt = IndexTracker[index * 2];

    for (int i = 0; i < IndexTracker[index * 2 + 1];) {
      int current = GeometryVertexes[startAt + i++];
      int next    = GeometryVertexes[startAt + i++];
      
      BuildVertex(
        values   [current],
        values   [next   ],
        positions[current],
        positions[next   ]
      );
    }
  }

  private void BuildVertex(float v0, float v1, float3 p0, float3 p1) {
    float3 p = p0 + (p1 - p0) / (v1 - v0) * (Min - v0);

    p.x *= Scale.x;
    p.y *= Scale.y;
    p.z *= Scale.z;
    
    Vertices [VertexCount] = p;
    Triangles[VertexCount] = VertexCount++;
  }

  private NoiseSample SimplexGradient3D (Vector3 point, float frequency) {
		point *= frequency;
		
		float skew = (point.x + point.y + point.z) * (1f / 3f);
		float sx 	 = point.x + skew;
		float sy   = point.y + skew;
		float sz   = point.z + skew;
		
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		int iz = Mathf.FloorToInt(sz);
		
		NoiseSample sample = SimplexGradient3DPart(point, ix, iy, iz);
		
		sample += SimplexGradient3DPart(point, ix + 1, iy + 1, iz + 1);

		float x = sx - ix;
		float y = sy - iy;
		float z = sz - iz;
		
		if (x >= y) {
			if (x >= z) {
				sample += SimplexGradient3DPart(point, ix + 1, iy, iz);

				if (y >= z)
					sample += SimplexGradient3DPart(point, ix + 1, iy + 1, iz);
				else
					sample += SimplexGradient3DPart(point, ix + 1, iy, iz + 1);
			} else {
				sample += SimplexGradient3DPart(point, ix, iy, iz + 1);
				sample += SimplexGradient3DPart(point, ix + 1, iy, iz + 1);
			}
		} else {
			if (y >= z) {
				sample += SimplexGradient3DPart(point, ix, iy + 1, iz);

				if (x >= z)
					sample += SimplexGradient3DPart(point, ix + 1, iy + 1, iz);
				else
					sample += SimplexGradient3DPart(point, ix, iy + 1, iz + 1);
			} else {
				sample += SimplexGradient3DPart(point, ix, iy, iz + 1);

				sample += SimplexGradient3DPart(point, ix, iy + 1, iz + 1);
			}
		}
		
		return sample * simplexScale3D;
	}

  private NoiseSample SimplexGradient3DPart (Vector3 point, int ix, int iy, int iz) {
		float unskew = (ix + iy + iz) * (1f / 6f);
		float x 		 = point.x - ix + unskew;
		float y 		 = point.y - iy + unskew;
		float z 		 = point.z - iz + unskew;
		float f 		 = 0.5f - x * x - y * y - z * z;
		
		NoiseSample sample = new NoiseSample();
		
		if (f > 0f) {
			float f2 = f * f;
			float f3 = f * f2;
			
			Vector3 g = SimplexGradients3D[PermutationTable[PermutationTable[PermutationTable[ix & hashMask] + iy & hashMask] + iz & hashMask] & simplexGradientsMask3D];

			float v = Dot(g, x, y, z);

			sample.value = v * f3;
		}
		
		return sample;
	}

  private static float Dot (Vector3 g, float x, float y, float z) {
		return g.x * x + g.y * y + g.z * z;
	}
}