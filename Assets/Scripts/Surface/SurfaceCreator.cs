using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SurfaceCreator : MonoBehaviour {
  [Range(1, 200)] public int resolution = 10;
  [Range(256, 1024)] public int textureResolution = 512;
  [Range(1, 20)] public int frequency = 1;
  [Range(0f, 1f)] public float strength = 1f;
	
	[Range(1, 8)] public int octaves = 1;
	
	[Range(1f, 4f)] public float lacunarity = 2f;
	
	[Range(0f, 1f)] public float persistence = 0.5f;
	
	[Range(1, 3)] public int dimensions = 3;
	
	public NoiseMethodType type;
	
	public Gradient coloring;
  public Vector3 offset;
  public Vector3 rotation;
  public bool coloringForStrength;
  public bool damping;
  [Range(0.15f, 1f)] public float dampingIntensity = 0.5f;
  public bool showNormals;

  private Vector3[] vertices;
	private Vector3[] normals;
	private Mesh mesh;
  private Texture2D texture;

	private void OnEnable () {
		if (mesh == null) {
			mesh = new Mesh();
			
      mesh.name = "Surface Mesh";

      GetComponent<MeshFilter>().mesh = mesh;
		}

    Refresh();
	}

	public void Refresh () {
    CreateGrid();
    CreateTexture();

    Quaternion q    = Quaternion.Euler(rotation);
    Quaternion qInv = Quaternion.Inverse(q);

    Vector3 point00 = q * new Vector3(-0.5f,-0.5f) + offset;
		Vector3 point10 = q * new Vector3( 0.5f,-0.5f) + offset;
		Vector3 point01 = q * new Vector3(-0.5f, 0.5f) + offset;
		Vector3 point11 = q * new Vector3( 0.5f, 0.5f) + offset;
		
		NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
		
    float stepSize  = 1f / resolution;
    float amplitude = damping ? (strength / frequency) * (1f / dampingIntensity) : strength;
		
    for (int v = 0, y = 0; y <= resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, y * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, y * stepSize);
		
    	for (int x = 0; x <= resolution; x++, v++) {
				Vector3 point = Vector3.Lerp(point0, point1, x * stepSize);
		
    		NoiseSample sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);

        sample *= 0.5f;
        sample *= amplitude;
		
        vertices[v].y = sample.value;
			}
		}
    
    mesh.vertices = vertices;
    
    mesh.RecalculateNormals();

    float textureStepSize = 1f / textureResolution;

    for (int v = 0, y = 0; y <= textureResolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, y * textureStepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, y * textureStepSize);
		
    	for (int x = 0; x <= textureResolution; x++, v++) {
				Vector3 point = Vector3.Lerp(point0, point1, x * textureStepSize);
		
    		NoiseSample sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);

        sample = type == NoiseMethodType.Value ? (sample - 0.5f) : (sample * 0.5f);

        if (coloringForStrength)
					texture.SetPixel(x, y, coloring.Evaluate(sample.value + 0.5f));
        else
          texture.SetPixel(x, y, coloring.Evaluate((sample.value * amplitude) + 0.5f));
			}
		}

    texture.Apply();
	}

  private void CreateGrid() {
    vertices  = new Vector3[(resolution + 1) * (resolution + 1)];
    
    Vector2[] uv        = new Vector2[vertices.Length];
    int[]     triangles = new int[resolution * resolution * 6];
		
    float stepSize = 1f / resolution;
		
    for (int v = 0, z = 0; z <= resolution; z++)
			for (int x = 0; x <= resolution; x++, v++) {
				vertices[v] = new Vector3(x * stepSize - 0.5f, 0f, z * stepSize - 0.5f);
        uv[v]       = new Vector2(x * stepSize, z * stepSize);
      }

    for (int t = 0, v = 0, y = 0; y < resolution; y++, v++)
			for (int x = 0; x < resolution; x++, v++, t += 6) {
				triangles[t]     = v;
				triangles[t + 1] = v + resolution + 1;
				triangles[t + 2] = v + 1;
				triangles[t + 3] = v + 1;
				triangles[t + 4] = v + resolution + 1;
				triangles[t + 5] = v + resolution + 2;
			}

    mesh.vertices  = vertices;
    mesh.uv        = uv;
    mesh.triangles = triangles;
  }

  private void CreateTexture() {
    texture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGB24, true);

    texture.name       = "Procedural Texture";
    texture.wrapMode   = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Trilinear;
    texture.anisoLevel = 9;


    Material material = GetComponent<MeshRenderer>().sharedMaterial;

    /* This is the property name for the base texture in the HDRP/Lit shader */
    material.SetTexture("_BaseColorMap", texture);

    GetComponent<MeshRenderer>().sharedMaterial = material;
  }

  private float GetXDerivative (int x, int z) {
		int rowOffset = z * (resolution + 1);
		
    float left, right, scale;
		
    if (x > 0) {
			left = vertices[rowOffset + x - 1].y;
			
      if (x < resolution) {
				right = vertices[rowOffset + x + 1].y;
				scale = 0.5f * resolution;
			} else {
				right = vertices[rowOffset + x].y;
				scale = resolution;
			}
		}
		else {
			left  = vertices[rowOffset + x].y;
			right = vertices[rowOffset + x + 1].y;
			scale = resolution;
		}
		
    return (right - left) * scale;
	}

  private float GetZDerivative (int x, int z) {
		int rowLength = resolution + 1;
		
    float back, forward, scale;
		
    if (z > 0) {
			back = vertices[(z - 1) * rowLength + x].y;
		
    	if (z < resolution) {
				forward = vertices[(z + 1) * rowLength + x].y;
				scale   = 0.5f * resolution;
			} else {
				forward = vertices[z * rowLength + x].y;
				scale   = resolution;
			}
		} else {
			back    = vertices[z * rowLength + x].y;
			forward = vertices[(z + 1) * rowLength + x].y;
			scale   = resolution;
		}
		
    return (forward - back) * scale;
	}
}