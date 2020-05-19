using System;
using UnityEngine;

using Unity.Jobs;
using Unity.Collections;

[Serializable]
public class Sparse3DVolumeMonoBehavior : MonoBehaviour {
  public BoundedFloatRange ResolutionRange;
  public BoundedFloatRange ChunksRange;
  public BoundedFloatRange ScaleRange;
  public BoundedFloatRange OffsetRange;
  public BoundedFloatRange FrequencyRange;
  public BoundedFloatRange MinRange;
  public Sparse3DVolume Volume = new Sparse3DVolume();

  private void Start() {
    var vertices  = new NativeArray<Vector3>(65535, Allocator.TempJob);
    var triangles = new NativeArray<int>(65535, Allocator.TempJob);

    #region geometryVertices
    var indexTracker     = new NativeArray<int>( 28, Allocator.TempJob);
    var geometryVertices = new NativeArray<int>(120, Allocator.TempJob);
    // new int[] {0, 1, 0, 2, 0, 3},
    // new int[] {1, 0, 1, 3, 1, 2},
    // new int[] {0, 3, 1, 2, 0, 2, 0, 3, 1, 3, 1, 2},
    // new int[] {2, 0, 2, 1, 2, 3},
    // new int[] {0, 1, 2, 3, 0, 3, 0, 1, 1, 2, 2, 3},
    // new int[] {0, 1, 1, 3, 2, 3, 0, 1, 2, 3, 0, 2},
    // new int[] {3, 0, 3, 1, 3, 2},
    // new int[] {3, 0, 3, 2, 3, 1},
    // new int[] {0, 1, 2, 3, 1, 3, 0, 1, 0, 2, 2, 3},
    // new int[] {0, 1, 0, 3, 2, 3, 0, 1, 2, 3, 1, 2},
    // new int[] {2, 0, 2, 3, 2, 1},
    // new int[] {0, 3, 0, 2, 1, 2, 0, 3, 1, 2, 1, 3},
    // new int[] {1, 0, 1, 2, 1, 3},
    // new int[] {0, 1, 0, 3, 0, 2}

    indexTracker[0] = 0;
    indexTracker[1] = 6;

    geometryVertices[0] = 0;
    geometryVertices[1] = 1;
    geometryVertices[2] = 0;
    geometryVertices[3] = 2;
    geometryVertices[4] = 0;
    geometryVertices[5] = 3;
    
    indexTracker[2] = 6;
    indexTracker[3] = 6;

    geometryVertices[ 6] = 1;
    geometryVertices[ 7] = 0;
    geometryVertices[ 8] = 1;
    geometryVertices[ 9] = 3;
    geometryVertices[10] = 1;
    geometryVertices[11] = 2;
    
    indexTracker[4] = 12;
    indexTracker[5] = 12;

    geometryVertices[12] = 0;
    geometryVertices[13] = 3;
    geometryVertices[14] = 1;
    geometryVertices[15] = 2;
    geometryVertices[16] = 0;
    geometryVertices[17] = 2;
    geometryVertices[18] = 0;
    geometryVertices[19] = 3;
    geometryVertices[20] = 1;
    geometryVertices[21] = 3;
    geometryVertices[22] = 1;
    geometryVertices[23] = 2;
    
    indexTracker[6] = 24;
    indexTracker[7] = 6;

    geometryVertices[24] = 2;
    geometryVertices[25] = 0;
    geometryVertices[26] = 2;
    geometryVertices[27] = 1;
    geometryVertices[28] = 2;
    geometryVertices[29] = 3;
    
    indexTracker[8] = 30;
    indexTracker[9] = 12;

    geometryVertices[30] = 0;
    geometryVertices[31] = 1;
    geometryVertices[32] = 2;
    geometryVertices[33] = 3;
    geometryVertices[34] = 0;
    geometryVertices[35] = 3;
    geometryVertices[36] = 0;
    geometryVertices[37] = 1;
    geometryVertices[38] = 1;
    geometryVertices[39] = 2;
    geometryVertices[40] = 2;
    geometryVertices[41] = 3;

    indexTracker[10] = 42;
    indexTracker[11] = 12;

    geometryVertices[42] = 0;
    geometryVertices[43] = 1;
    geometryVertices[44] = 1;
    geometryVertices[45] = 3;
    geometryVertices[46] = 2;
    geometryVertices[47] = 3;
    geometryVertices[48] = 0;
    geometryVertices[49] = 1;
    geometryVertices[50] = 2;
    geometryVertices[51] = 3;
    geometryVertices[52] = 0;
    geometryVertices[53] = 2;

    indexTracker[12] = 54;
    indexTracker[13] = 6;

    geometryVertices[54] = 3;
    geometryVertices[55] = 0;
    geometryVertices[56] = 3;
    geometryVertices[57] = 1;
    geometryVertices[58] = 3;
    geometryVertices[59] = 2;

    indexTracker[14] = 60;
    indexTracker[15] = 6;

    geometryVertices[60] = 3;
    geometryVertices[61] = 0;
    geometryVertices[62] = 3;
    geometryVertices[63] = 2;
    geometryVertices[64] = 3;
    geometryVertices[65] = 1;

    indexTracker[16] = 66;
    indexTracker[17] = 12;

    geometryVertices[66] = 0;
    geometryVertices[67] = 1;
    geometryVertices[68] = 2;
    geometryVertices[69] = 3;
    geometryVertices[70] = 1;
    geometryVertices[71] = 3;
    geometryVertices[72] = 0;
    geometryVertices[73] = 1;
    geometryVertices[74] = 0;
    geometryVertices[75] = 2;
    geometryVertices[76] = 2;
    geometryVertices[77] = 3;

    indexTracker[18] = 78;
    indexTracker[19] = 12;

    geometryVertices[78] = 0;
    geometryVertices[79] = 1;
    geometryVertices[80] = 0;
    geometryVertices[81] = 3;
    geometryVertices[82] = 2;
    geometryVertices[83] = 3;
    geometryVertices[84] = 0;
    geometryVertices[85] = 1;
    geometryVertices[86] = 2;
    geometryVertices[87] = 3;
    geometryVertices[88] = 1;
    geometryVertices[89] = 2;

    indexTracker[20] = 90;
    indexTracker[21] = 6;

    geometryVertices[90] = 2;
    geometryVertices[91] = 0;
    geometryVertices[92] = 2;
    geometryVertices[93] = 3;
    geometryVertices[94] = 2;
    geometryVertices[95] = 1;

    indexTracker[22] = 96;
    indexTracker[23] = 12;

    geometryVertices[ 96] = 0;
    geometryVertices[ 97] = 3;
    geometryVertices[ 98] = 0;
    geometryVertices[ 99] = 2;
    geometryVertices[100] = 1;
    geometryVertices[101] = 2;
    geometryVertices[102] = 0;
    geometryVertices[103] = 3;
    geometryVertices[104] = 1;
    geometryVertices[105] = 2;
    geometryVertices[106] = 1;
    geometryVertices[107] = 3;

    indexTracker[24] = 108;
    indexTracker[25] = 6;

    geometryVertices[108] = 1;
    geometryVertices[109] = 0;
    geometryVertices[110] = 1;
    geometryVertices[111] = 2;
    geometryVertices[112] = 1;
    geometryVertices[113] = 3;

    indexTracker[26] = 114;
    indexTracker[27] = 6;

    geometryVertices[114] = 0;
    geometryVertices[115] = 1;
    geometryVertices[116] = 0;
    geometryVertices[117] = 3;
    geometryVertices[118] = 0;
    geometryVertices[119] = 2;
    #endregion

    #region sampleIndexes
    var sampleIndexes = new NativeArray<int>(24, Allocator.TempJob);
    // new int[] {0, 2, 3, 7},
    // new int[] {0, 2, 7, 6},
    // new int[] {0, 4, 6, 7},
    // new int[] {0, 6, 1, 2},
    // new int[] {0, 4, 1, 6},
    // new int[] {5, 6, 1, 4}

    sampleIndexes[0] = 0;
    sampleIndexes[1] = 2;
    sampleIndexes[2] = 3;
    sampleIndexes[3] = 7;

    sampleIndexes[4] = 0;
    sampleIndexes[5] = 2;
    sampleIndexes[6] = 7;
    sampleIndexes[7] = 6;

    sampleIndexes[ 8] = 0;
    sampleIndexes[ 9] = 4;
    sampleIndexes[10] = 6;
    sampleIndexes[11] = 7;

    sampleIndexes[12] = 0;
    sampleIndexes[13] = 6;
    sampleIndexes[14] = 1;
    sampleIndexes[15] = 2;

    sampleIndexes[16] = 0;
    sampleIndexes[17] = 4;
    sampleIndexes[18] = 1;
    sampleIndexes[19] = 6;

    sampleIndexes[20] = 5;
    sampleIndexes[21] = 6;
    sampleIndexes[22] = 1;
    sampleIndexes[23] = 4;
    #endregion
    
    #region permutationTable
    var table = new int[] {
      151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
      140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
      247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
      57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
      74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
      60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
      65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
      200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
      52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
      207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
      119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
      129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
      218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
      81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
      184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
      222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
    };
    var permutationTable = new NativeArray<int>(table.Length, Allocator.TempJob);

    for (int i = 0; i < table.Length; i++) {
      permutationTable[i] = table[i];
    }
    #endregion

    #region gradients
    var gradients = new NativeArray<Vector3>(32, Allocator.TempJob);

		gradients[ 0] = new Vector3( 1f, 1f, 0f).normalized;
		gradients[ 1] = new Vector3(-1f, 1f, 0f).normalized;
		gradients[ 2] = new Vector3( 1f,-1f, 0f).normalized;
		gradients[ 3] = new Vector3(-1f,-1f, 0f).normalized;
		gradients[ 4] = new Vector3( 1f, 0f, 1f).normalized;
		gradients[ 5] = new Vector3(-1f, 0f, 1f).normalized;
		gradients[ 6] = new Vector3( 1f, 0f,-1f).normalized;
		gradients[ 7] = new Vector3(-1f, 0f,-1f).normalized;
		gradients[ 8] = new Vector3( 0f, 1f, 1f).normalized;
		gradients[ 9] = new Vector3( 0f,-1f, 1f).normalized;
		gradients[10] = new Vector3( 0f, 1f,-1f).normalized;
		gradients[11] = new Vector3( 0f,-1f,-1f).normalized;
		
		gradients[12] = new Vector3( 1f, 1f, 0f).normalized;
		gradients[13] = new Vector3(-1f, 1f, 0f).normalized;
		gradients[14] = new Vector3( 1f,-1f, 0f).normalized;
		gradients[15] = new Vector3(-1f,-1f, 0f).normalized;
		gradients[16] = new Vector3( 1f, 0f, 1f).normalized;
		gradients[17] = new Vector3(-1f, 0f, 1f).normalized;
		gradients[18] = new Vector3( 1f, 0f,-1f).normalized;
		gradients[19] = new Vector3(-1f, 0f,-1f).normalized;
		gradients[20] = new Vector3( 0f, 1f, 1f).normalized;
		gradients[21] = new Vector3( 0f,-1f, 1f).normalized;
		gradients[22] = new Vector3( 0f, 1f,-1f).normalized;
		gradients[23] = new Vector3( 0f,-1f,-1f).normalized;
		
		gradients[24] = new Vector3( 1f, 1f, 1f).normalized;
		gradients[25] = new Vector3(-1f, 1f, 1f).normalized;
		gradients[26] = new Vector3( 1f,-1f, 1f).normalized;
		gradients[27] = new Vector3(-1f,-1f, 1f).normalized;
		gradients[28] = new Vector3( 1f, 1f,-1f).normalized;
		gradients[29] = new Vector3(-1f, 1f,-1f).normalized;
		gradients[30] = new Vector3( 1f,-1f,-1f).normalized;
		gradients[31] = new Vector3(-1f,-1f,-1f).normalized;
    #endregion

    NativeArray<int> totalVertexes = new NativeArray<int>(1, Allocator.TempJob);

    var job = new NoiseRenderJob {
      GeometryVertexes = geometryVertices,
      IndexTracker = indexTracker,
      SampleIndexes = sampleIndexes,
      TotalVertexes = totalVertexes,
      PermutationTable = permutationTable,
      Gradients = gradients,
      Resolution = Volume.Resolution,
      Scale      = Volume.Scale,
      Offset     = Volume.Offset,
      Min        = Volume.Min,
      Frequency  = Volume.Frequency,
      Vertices   = vertices,
      Triangles  = triangles
    };

    var handle = job.Schedule();

    handle.Complete();

    Mesh mesh = new Mesh { name = $"Mesh" };

    var tris = triangles.Slice(0, totalVertexes[0]).ToArray();
    var verts = vertices.Slice(0, totalVertexes[0]).ToArray();

    mesh.vertices  = verts;
    mesh.triangles = tris;

    mesh.Optimize();
    mesh.RecalculateNormals();

    GetComponent<MeshFilter>().sharedMesh = mesh;

    totalVertexes.Dispose();

    permutationTable.Dispose();
    gradients.Dispose();

    geometryVertices.Dispose();
    indexTracker.Dispose();
    sampleIndexes.Dispose();

    vertices.Dispose();
    triangles.Dispose();
  }
}