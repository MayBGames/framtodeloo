using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoxelRenderer))]
public class VoxelRendererEditor : Editor {  
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    if (GUILayout.Button("Render")) {
      var container = (VoxelRenderer) target;
      var vertices  = new List<Vector3>();
      var triangles = new List<int>();
      var uvs       = new List<Vector2>();
      var voxels    = new bool[container.Width * container.Depth * container.Height];

      int t = 0;

      for (int x = 0; x < container.Width; x++)
        for (int z = 0; z < container.Depth; z++)
          for (int y = 0; y < container.Height; y++) {
            int  cell   = x + container.Width * (y + container.Height * z);
            bool facing = Random.Range(0f, 1f) > 0.5f;

            voxels[cell] = facing;

            t = ProcessX(x, y, z, vertices, triangles, uvs, t, container, facing, voxels);
            t = ProcessZ(x, y, z, vertices, triangles, uvs, t, container, facing, voxels);
            t = ProcessY(x, y, z, vertices, triangles, uvs, t, container, facing, voxels);
          }

      var mesh = new Mesh {
        vertices  = vertices.ToArray(),
        triangles = triangles.ToArray(),
        uv        = uvs.ToArray()
      };

      mesh.RecalculateNormals();

      container.GetComponent<MeshFilter>().mesh = mesh;
    }
  }

  private int ProcessZ(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t,
    VoxelRenderer container,
    bool currentForward,
    bool[] voxels
  ) {
    if (z == 0) {
      if (currentForward == true)
        t += BuildFrontFace(x, y, z, vertices, triangles, uvs, t);
    } else {
      int previousCell = x + container.Width * (y + container.Height * (z - 1));
      
      bool previousForward = voxels[previousCell];

      if (currentForward == true) {
        if (previousForward == false)
          t += BuildFrontFace(x, y, z, vertices, triangles, uvs, t);
        
        if (z == container.Depth - 1)
          t += BuildBackFace(x, y, z + 1, vertices, triangles, uvs, t);
      } else if (currentForward == false) {
        if (previousForward == true)
          t += BuildBackFace(x, y, z, vertices, triangles, uvs, t);
      }
    }

    return t;
  }

  private int BuildFrontFace(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t
  ) {
    vertices.Add(new Vector3(x + 1, y + 1, z));
    vertices.Add(new Vector3(x + 1, y,     z));
    vertices.Add(new Vector3(x,     y,     z));
    vertices.Add(new Vector3(x,     y,     z));
    vertices.Add(new Vector3(x,     y + 1, z));
    vertices.Add(new Vector3(x + 1, y + 1, z));

    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);

    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 0));
    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(0, 1));
    uvs.Add(new Vector2(0, 1));

    return 6;
  }

  private int BuildBackFace(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t
  ) {
    vertices.Add(new Vector3(x,     y,     z));
    vertices.Add(new Vector3(x + 1, y,     z));
    vertices.Add(new Vector3(x + 1, y + 1, z));
    vertices.Add(new Vector3(x + 1, y + 1, z));
    vertices.Add(new Vector3(x,     y + 1, z));
    vertices.Add(new Vector3(x,     y,     z));

    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);

    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(1, 0));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(0, 1));
    uvs.Add(new Vector2(0, 0));

    return 6;
  }

  private int ProcessX(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t,
    VoxelRenderer container,
    bool currentLeft,
    bool[] voxels
  ) {
    if (x == 0) {
      if (currentLeft == true)
        t += BuildLeftFace(x, y, z, vertices, triangles, uvs, t);
    } else {
      int previousCell = (x - 1) + container.Width * (y + container.Height * z);
      
      bool previousLeft = voxels[previousCell];

      if (currentLeft == true) {
        if (previousLeft == false)
          t += BuildLeftFace(x, y, z, vertices, triangles, uvs, t);

        if (x == container.Width - 1)
          t += BuildRightFace(x + 1, y, z, vertices, triangles, uvs, t);
      } else if (currentLeft == false) {
        if (previousLeft == true)
          t += BuildRightFace(x, y, z, vertices, triangles, uvs, t);
      }
    }

    return t;
  }

  private int BuildLeftFace(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t
  ) {
    vertices.Add(new Vector3(x, y,     z    ));
    vertices.Add(new Vector3(x, y,     z + 1));
    vertices.Add(new Vector3(x, y + 1, z + 1));
    vertices.Add(new Vector3(x, y + 1, z + 1));
    vertices.Add(new Vector3(x, y + 1, z    ));
    vertices.Add(new Vector3(x, y,     z    ));

    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);

    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(0, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 0));
    uvs.Add(new Vector2(0, 0));

    return 6;
  }

  private int BuildRightFace(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t
  ) {
    vertices.Add(new Vector3(x, y,     z    ));
    vertices.Add(new Vector3(x, y + 1, z    ));
    vertices.Add(new Vector3(x, y + 1, z + 1));
    vertices.Add(new Vector3(x, y + 1, z + 1));
    vertices.Add(new Vector3(x, y,     z + 1));
    vertices.Add(new Vector3(x, y,     z    ));

    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);

    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(1, 0));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(0, 1));
    uvs.Add(new Vector2(0, 0));

    return 6;
  }

  private int ProcessY(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t,
    VoxelRenderer container,
    bool currentBottom,
    bool[] voxels
  ) {
    if (y == 0) {
      if (currentBottom == true)
        t += BuildBottomFace(x, y, z, vertices, triangles, uvs, t);
    } else {
      int previousCell = x + container.Width * ((y - 1) + container.Height * z);
      
      bool previousBottom = voxels[previousCell];

      if (currentBottom == true) {
        if (previousBottom == false)
          t += BuildBottomFace(x, y, z, vertices, triangles, uvs, t);
        
        if (y == container.Height - 1)
          t += BuildTopFace(x, y + 1, z, vertices, triangles, uvs, t);
      } else if (currentBottom == false) {
        if (previousBottom == true)
          t += BuildTopFace(x, y, z, vertices, triangles, uvs, t);
      }
    }

    return t;
  }

  private int BuildBottomFace(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t
  ) {
    vertices.Add(new Vector3(x,     y, z    ));
    vertices.Add(new Vector3(x + 1, y, z    ));
    vertices.Add(new Vector3(x + 1, y, z + 1));
    vertices.Add(new Vector3(x + 1, y, z + 1));
    vertices.Add(new Vector3(x,     y, z + 1));
    vertices.Add(new Vector3(x,     y, z    ));

    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);

    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(1, 0));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(0, 1));
    uvs.Add(new Vector2(0, 0));

    return 6;
  }

  private int BuildTopFace(
    int x,
    int y,
    int z,
    List<Vector3> vertices,
    List<int> triangles,
    List<Vector2> uvs,
    int t
  ) {
    vertices.Add(new Vector3(x,     y, z    ));
    vertices.Add(new Vector3(x,     y, z + 1));
    vertices.Add(new Vector3(x + 1, y, z + 1));
    vertices.Add(new Vector3(x + 1, y, z + 1));
    vertices.Add(new Vector3(x + 1, y, z    ));
    vertices.Add(new Vector3(x,     y, z    ));

    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);
    triangles.Add(t++);

    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(0, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 0));
    uvs.Add(new Vector2(0, 0));

    return 6;
  }

}