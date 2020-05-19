using UnityEngine;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct NoiseRenderJob : IJob {
  public float3 Resolution;
  public float3 Scale;
  public float3 Offset;

  public float Min;
  public float Frequency;

  public NativeArray<int> GeometryVertexes;
  public NativeArray<int> IndexTracker;
  public NativeArray<int> SampleIndexes;

  public NativeArray<int> TotalVertexes;

  public NativeArray<Vector3> Vertices;
  public NativeArray<int> Triangles;

  public NativeArray<int> PermutationTable;
  public NativeArray<Vector3> Gradients;

  public void Execute() {
    var volume = new Sparse3DVolume();

    volume.GeometryVertexes = GeometryVertexes;
    volume.IndexTracker = IndexTracker;
    volume.SampleIndexes = SampleIndexes;
    
    volume.Resolution = Resolution;
    volume.Scale      = Scale;
    volume.Offset     = Offset;
    volume.Min        = Min;
    volume.Frequency  = Frequency;
    volume.Vertices   = Vertices;
    volume.Triangles  = Triangles;

    volume.PermutationTable = PermutationTable;
    volume.SimplexGradients3D = Gradients;

    volume.SparseBuildMesh();

    TotalVertexes[0] = volume.VertexCount;
  }
}