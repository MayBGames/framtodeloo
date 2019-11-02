using UnityEngine;

using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

[BurstCompile]
struct RenderChunkJob : IJobParallelForBatch {
  [ReadOnly] public NativeArray<Vector3> positions;
  [ReadOnly] public NativeArray<float> values;
  
  public void Execute(int start, int count) {
    
  }
}