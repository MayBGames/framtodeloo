using UnityEngine;

public delegate NoiseSample NoiseMethod (Vector3 point, float frequency);

public enum NoiseMethodType {
	Value,
	Perlin,
	SimplexValue,
	SimplexGradient
}

public static class Noise {

  public static int[] PermutationTable = {
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

  private const int hashMask = 255;

  private static float[] gradients1D = {
		1f, -1f
	};

	private const int gradientsMask1D = 1;

  private static Vector2[] gradients2D = {
		new Vector2( 1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2( 0f, 1f),
		new Vector2( 0f,-1f),
    new Vector2( 1f, 1f).normalized,
		new Vector2(-1f, 1f).normalized,
		new Vector2( 1f,-1f).normalized,
		new Vector2(-1f,-1f).normalized
	};
	
	private const int gradientsMask2D = 7;

  private static float sqr2 = Mathf.Sqrt(2f);

  private static Vector3[] gradients3D = {
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 1f,-1f, 0f),
		new Vector3(-1f,-1f, 0f),
		new Vector3( 1f, 0f, 1f),
		new Vector3(-1f, 0f, 1f),
		new Vector3( 1f, 0f,-1f),
		new Vector3(-1f, 0f,-1f),
		new Vector3( 0f, 1f, 1f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f, 1f,-1f),
		new Vector3( 0f,-1f,-1f),
		
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f,-1f,-1f)
	};
	
	private const int gradientsMask3D = 15;

	private static Vector3[] simplexGradients3D = {
		new Vector3( 1f, 1f, 0f).normalized,
		new Vector3(-1f, 1f, 0f).normalized,
		new Vector3( 1f,-1f, 0f).normalized,
		new Vector3(-1f,-1f, 0f).normalized,
		new Vector3( 1f, 0f, 1f).normalized,
		new Vector3(-1f, 0f, 1f).normalized,
		new Vector3( 1f, 0f,-1f).normalized,
		new Vector3(-1f, 0f,-1f).normalized,
		new Vector3( 0f, 1f, 1f).normalized,
		new Vector3( 0f,-1f, 1f).normalized,
		new Vector3( 0f, 1f,-1f).normalized,
		new Vector3( 0f,-1f,-1f).normalized,
		
		new Vector3( 1f, 1f, 0f).normalized,
		new Vector3(-1f, 1f, 0f).normalized,
		new Vector3( 1f,-1f, 0f).normalized,
		new Vector3(-1f,-1f, 0f).normalized,
		new Vector3( 1f, 0f, 1f).normalized,
		new Vector3(-1f, 0f, 1f).normalized,
		new Vector3( 1f, 0f,-1f).normalized,
		new Vector3(-1f, 0f,-1f).normalized,
		new Vector3( 0f, 1f, 1f).normalized,
		new Vector3( 0f,-1f, 1f).normalized,
		new Vector3( 0f, 1f,-1f).normalized,
		new Vector3( 0f,-1f,-1f).normalized,
		
		new Vector3( 1f, 1f, 1f).normalized,
		new Vector3(-1f, 1f, 1f).normalized,
		new Vector3( 1f,-1f, 1f).normalized,
		new Vector3(-1f,-1f, 1f).normalized,
		new Vector3( 1f, 1f,-1f).normalized,
		new Vector3(-1f, 1f,-1f).normalized,
		new Vector3( 1f,-1f,-1f).normalized,
		new Vector3(-1f,-1f,-1f).normalized
	};
	
	private const int simplexGradientsMask3D = 31;

  public static NoiseMethod[] perlinMethods = {
		Perlin1D,
		Perlin2D,
		Perlin3D
	};

  public static NoiseMethod[] valueMethods = {
		Value1D,
		Value2D,
		Value3D
	};

	public static NoiseMethod[] simplexValueMethods = {
		SimplexValue1D,
		SimplexValue2D,
		SimplexValue3D
	};

	public static NoiseMethod[] simplexGraientMethods = {
		SimplexGradient1D,
		SimplexGradient2D,
		SimplexGradient3D
	};

  public static NoiseMethod[][] noiseMethods = {
		valueMethods,
		perlinMethods,
		simplexValueMethods,
		simplexGraientMethods
	};

	private static float squaresToTriangles = (3f - Mathf.Sqrt(3f)) / 6f;
	private static float trianglesToSquares = (Mathf.Sqrt(3f) - 1f) / 2f;
	private static float simplexScale2D = 2916f * sqr2 / 125f;
	private static float simplexScale3D = 8192f * Mathf.Sqrt(3f) / 375f;

  public static NoiseSample Value1D (Vector3 point, float frequency) {
    point *= frequency;
		
    int i0 = Mathf.FloorToInt(point.x);
    
    float t = point.x - i0;

    i0 &= hashMask;

    int i1 = i0 + 1 & hashMask;

    int h0 = PermutationTable[i0];
    int h1 = PermutationTable[i1];

		float a = h0;
		float b = h1 - h0;

		float dt = SmoothDerivative(t);

    t = Smooth(t);
		
    NoiseSample sample;
		
		sample.value = a + b * t;
		
		sample.derivative.x = b * dt;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		
		sample.derivative *= frequency;
		
		return sample * (2f / hashMask) - 1f;
	}

  public static NoiseSample Value2D (Vector3 point, float frequency) {
    point *= frequency;
		
    int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);

    float tx = point.x - ix0;
		float ty = point.y - iy0;

    ix0 &= hashMask;
		iy0 &= hashMask;

    int ix1 = ix0 + 1 & hashMask;
		int iy1 = iy0 + 1 & hashMask;

    int h0 = PermutationTable[ix0];
    int h1 = PermutationTable[ix1];
    
    int h00 = PermutationTable[h0 + iy0 & hashMask];
    int h10 = PermutationTable[h1 + iy0 & hashMask];
		int h01 = PermutationTable[h0 + iy1 & hashMask];
    int h11 = PermutationTable[h1 + iy1 & hashMask];
		
    tx = Smooth(tx);
		ty = Smooth(ty);

		NoiseSample sample;
		
		sample.value = Mathf.Lerp(
			Mathf.Lerp(h00, h10, tx),
			Mathf.Lerp(h01, h11, tx),
			ty
    ) * (2f / hashMask) - 1f;
		
		sample.derivative.x = 0f;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		
		sample.derivative *= frequency;
		
		return sample;
	}

  public static NoiseSample Value3D (Vector3 point, float frequency) {
    point *= frequency;
		
    int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
    int iz0 = Mathf.FloorToInt(point.z);

    float tx = point.x - ix0;
		float ty = point.y - iy0;
    float tz = point.z - iz0;

    ix0 &= hashMask;
		iy0 &= hashMask;
    iz0 &= hashMask;

    int ix1 = ix0 + 1 & hashMask;
		int iy1 = iy0 + 1 & hashMask;
    int iz1 = iz0 + 1 & hashMask;

    int h0 = PermutationTable[ix0];
    int h1 = PermutationTable[ix1];
    
    int h00 = PermutationTable[h0 + iy0 & hashMask];
    int h10 = PermutationTable[h1 + iy0 & hashMask];
		int h01 = PermutationTable[h0 + iy1 & hashMask];
    int h11 = PermutationTable[h1 + iy1 & hashMask];

    int h000 = PermutationTable[h00 + iz0 & hashMask];
		int h100 = PermutationTable[h10 + iz0 & hashMask];
		int h010 = PermutationTable[h01 + iz0 & hashMask];
		int h110 = PermutationTable[h11 + iz0 & hashMask];
		int h001 = PermutationTable[h00 + iz1 & hashMask];
		int h101 = PermutationTable[h10 + iz1 & hashMask];
		int h011 = PermutationTable[h01 + iz1 & hashMask];
		int h111 = PermutationTable[h11 + iz1 & hashMask];
		
    tx = Smooth(tx);
		ty = Smooth(ty);
    tz = Smooth(tz);

		NoiseSample sample;
		
		sample.value = Mathf.Lerp(
			Mathf.Lerp(Mathf.Lerp(h000, h100, tx), Mathf.Lerp(h010, h110, tx), ty),
			Mathf.Lerp(Mathf.Lerp(h001, h101, tx), Mathf.Lerp(h011, h111, tx), ty),
			tz
    ) * (2f / hashMask) - 1;
		
		sample.derivative.x = 0f;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		
		sample.derivative *= frequency;
		
		return sample;
	}

  public static NoiseSample Perlin1D (Vector3 point, float frequency) {
    point *= frequency;
		
    int i0 = Mathf.FloorToInt(point.x);
    
    float t0 = point.x - i0;
    float t1 = t0 - 1f;

    i0 &= hashMask;

    int i1 = i0 + 1 & hashMask;

    int h0 = PermutationTable[i0];
    int h1 = PermutationTable[i1];

    float g0 = gradients1D[h0 & gradientsMask1D];
		float g1 = gradients1D[h1 & gradientsMask1D];

    float v0 = g0 * t0;
		float v1 = g1 * t1;

    float t = Smooth(t0);

		NoiseSample sample;
		
		sample.value = Mathf.Lerp(v0, v1, t) * 2f;
		
		sample.derivative.x = 0f;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		
		sample.derivative *= frequency;
		
		return sample;
	}
	
	public static NoiseSample Perlin2D (Vector3 point, float frequency) {
    point *= frequency;
		
    int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);

    float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
    float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;

    ix0 &= hashMask;
		iy0 &= hashMask;

    int ix1 = ix0 + 1 & hashMask;
		int iy1 = iy0 + 1 & hashMask;

    int h0 = PermutationTable[ix0];
    int h1 = PermutationTable[ix1];
    
    Vector2 g00 = gradients2D[PermutationTable[h0 + iy0 & hashMask] & gradientsMask2D];
		Vector2 g10 = gradients2D[PermutationTable[h1 + iy0 & hashMask] & gradientsMask2D];
		Vector2 g01 = gradients2D[PermutationTable[h0 + iy1 & hashMask] & gradientsMask2D];
		Vector2 g11 = gradients2D[PermutationTable[h1 + iy1 & hashMask] & gradientsMask2D];
    
    float v00 = Dot(g00, tx0, ty0);
		float v10 = Dot(g10, tx1, ty0);
		float v01 = Dot(g01, tx0, ty1);
		float v11 = Dot(g11, tx1, ty1);
		
    float tx = Smooth(tx0);
		float ty = Smooth(ty0);

		NoiseSample sample;
		
		sample.value = Mathf.Lerp(
			Mathf.Lerp(v00, v10, tx),
			Mathf.Lerp(v01, v11, tx),
			ty
    ) * sqr2;
		
		sample.derivative.x = 0f;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		
		sample.derivative *= frequency;
		
		return sample;
	}
	
	public static NoiseSample Perlin3D (Vector3 point, float frequency) {
    point *= frequency;
		
    int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
    int iz0 = Mathf.FloorToInt(point.z);

    float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
    float tz0 = point.z - iz0;
    float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;
		float tz1 = tz0 - 1f;

    ix0 &= hashMask;
		iy0 &= hashMask;
    iz0 &= hashMask;

    int ix1 = ix0 + 1 & hashMask;
		int iy1 = iy0 + 1 & hashMask;
    int iz1 = iz0 + 1 & hashMask;

    int h0 = PermutationTable[ix0];
    int h1 = PermutationTable[ix1];
    
    int h00 = PermutationTable[h0 + iy0 & hashMask];
    int h10 = PermutationTable[h1 + iy0 & hashMask];
		int h01 = PermutationTable[h0 + iy1 & hashMask];
    int h11 = PermutationTable[h1 + iy1 & hashMask];

    Vector3 g000 = gradients3D[PermutationTable[h00 + iz0 & hashMask] & gradientsMask3D];
		Vector3 g100 = gradients3D[PermutationTable[h10 + iz0 & hashMask] & gradientsMask3D];
		Vector3 g010 = gradients3D[PermutationTable[h01 + iz0 & hashMask] & gradientsMask3D];
		Vector3 g110 = gradients3D[PermutationTable[h11 + iz0 & hashMask] & gradientsMask3D];
		Vector3 g001 = gradients3D[PermutationTable[h00 + iz1 & hashMask] & gradientsMask3D];
		Vector3 g101 = gradients3D[PermutationTable[h10 + iz1 & hashMask] & gradientsMask3D];
		Vector3 g011 = gradients3D[PermutationTable[h01 + iz1 & hashMask] & gradientsMask3D];
		Vector3 g111 = gradients3D[PermutationTable[h11 + iz1 & hashMask] & gradientsMask3D];

    float v000 = Dot(g000, tx0, ty0, tz0);
		float v100 = Dot(g100, tx1, ty0, tz0);
		float v010 = Dot(g010, tx0, ty1, tz0);
		float v110 = Dot(g110, tx1, ty1, tz0);
		float v001 = Dot(g001, tx0, ty0, tz1);
		float v101 = Dot(g101, tx1, ty0, tz1);
		float v011 = Dot(g011, tx0, ty1, tz1);
		float v111 = Dot(g111, tx1, ty1, tz1);
		
    float tx = Smooth(tx0);
		float ty = Smooth(ty0);
    float tz = Smooth(tz0);

		NoiseSample sample;
		
		sample.value = Mathf.Lerp(
			Mathf.Lerp(Mathf.Lerp(v000, v100, tx), Mathf.Lerp(v010, v110, tx), ty),
			Mathf.Lerp(Mathf.Lerp(v001, v101, tx), Mathf.Lerp(v011, v111, tx), ty),
			tz
    ) * sqr2;
		
		sample.derivative.x = 0f;
		sample.derivative.y = 0f;
		sample.derivative.z = 0f;
		
		sample.derivative *= frequency;
		
		return sample;
	}

	public static NoiseSample SimplexValue1D (Vector3 point, float frequency) {
		point *= frequency;
		
		int ix = Mathf.FloorToInt(point.x);
		
		NoiseSample sample = SimplexValue1DPart(point, ix);
		
		sample += SimplexValue1DPart(point, ix + 1);
		
		return sample * (2f / hashMask) - 1f;
	}

	public static NoiseSample SimplexValue2D (Vector3 point, float frequency) {
		point *= frequency;

		float skew = (point.x + point.y) * trianglesToSquares;
		float sx 	 = point.x + skew;
		float sy   = point.y + skew;
		
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		
		NoiseSample sample = SimplexValue2DPart(point, ix, iy);
		
		sample += SimplexValue2DPart(point, ix + 1, iy + 1);

		if (sx - ix >= sy - iy)
			sample += SimplexValue2DPart(point, ix + 1, iy);
		else
			sample += SimplexValue2DPart(point, ix, iy + 1);
		
		return sample * (8f * 2f / hashMask) - 1f;
	}

	public static NoiseSample SimplexValue3D (Vector3 point, float frequency) {
		point *= frequency;
		
		float skew = (point.x + point.y + point.z) * (1f / 3f);
		float sx 	 = point.x + skew;
		float sy   = point.y + skew;
		float sz   = point.z + skew;
		
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		int iz = Mathf.FloorToInt(sz);
		
		NoiseSample sample = SimplexValue3DPart(point, ix, iy, iz);
		
		sample += SimplexValue3DPart(point, ix + 1, iy + 1, iz + 1);

		float x = sx - ix;
		float y = sy - iy;
		float z = sz - iz;
		
		if (x >= y) {
			if (x >= z) {
				sample += SimplexValue3DPart(point, ix + 1, iy, iz);

				if (y >= z)
					sample += SimplexValue3DPart(point, ix + 1, iy + 1, iz);
				else
					sample += SimplexValue3DPart(point, ix + 1, iy, iz + 1);
			} else {
				sample += SimplexValue3DPart(point, ix, iy, iz + 1);
				sample += SimplexValue3DPart(point, ix + 1, iy, iz + 1);
			}
		} else {
			if (y >= z) {
				sample += SimplexValue3DPart(point, ix, iy + 1, iz);

				if (x >= z)
					sample += SimplexValue3DPart(point, ix + 1, iy + 1, iz);
				else
					sample += SimplexValue3DPart(point, ix, iy + 1, iz + 1);
			} else {
				sample += SimplexValue3DPart(point, ix, iy, iz + 1);

				sample += SimplexValue3DPart(point, ix, iy + 1, iz + 1);
			}
		}
		
		return sample * (8f * 2f / hashMask) - 1f;
	}

	public static NoiseSample SimplexGradient1D (Vector3 point, float frequency) {
		point *= frequency;
		
		int ix = Mathf.FloorToInt(point.x);
		
		NoiseSample sample = SimplexGradient1DPart(point, ix);
		
		sample += SimplexGradient1DPart(point, ix + 1);
		
		return sample * (64f / 27f);
	}

	public static NoiseSample SimplexGradient2D (Vector3 point, float frequency) {
		point *= frequency;

		float skew = (point.x + point.y) * trianglesToSquares;
		float sx 	 = point.x + skew;
		float sy   = point.y + skew;
		
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		
		NoiseSample sample = SimplexGradient2DPart(point, ix, iy);
		
		sample += SimplexGradient2DPart(point, ix + 1, iy + 1);

		if (sx - ix >= sy - iy)
			sample += SimplexGradient2DPart(point, ix + 1, iy);
		else
			sample += SimplexGradient2DPart(point, ix, iy + 1);
		
		return sample * simplexScale2D;
	}

	public static NoiseSample SimplexGradient3D (Vector3 point, float frequency) {
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

  private static float Smooth (float t) {
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

  private static float Dot (Vector2 g, float x, float y) {
		return g.x * x + g.y * y;
	}

  private static float Dot (Vector3 g, float x, float y, float z) {
		return g.x * x + g.y * y + g.z * z;
	}

  public static NoiseSample Sum (NoiseMethod method, Vector3 point, float frequency, int octaves, float lacunarity, float persistence) {
		NoiseSample sum = method(point, frequency);
		
		float amplitude = 1f;
		float range = 1f;
		
    for (int o = 1; o < octaves; o++) {
			frequency *= lacunarity;
			amplitude *= persistence;
			range += amplitude;
			sum += method(point, frequency) * amplitude;
		}
		
    return sum * (1f / range);
	}

	private static float SmoothDerivative (float t) {
		return 30f * t * t * (t * (t - 2f) + 1f);
	}

	private static NoiseSample SimplexValue1DPart (Vector3 point, int ix) {
		float x  = point.x - ix;
		float f  = 1f - x * x;
		float f2 = f * f;
		float f3 = f * f2;
		float h = PermutationTable[ix & hashMask];
		
		NoiseSample sample = new NoiseSample();
		
		sample.value = h * f3;
		
		return sample;
	}

	private static NoiseSample SimplexValue2DPart (Vector3 point, int ix, int iy) {
		float unskew = (ix + iy) * squaresToTriangles;
		float x  		 = point.x - ix + unskew;
		float y  		 = point.y - iy + unskew;
		float f  		 = 0.5f - x * x - y * y;

		NoiseSample sample = new NoiseSample();
		
		if (f > 0f) {
			float f2 = f * f;
			float f3 = f * f2;

			float h = PermutationTable[PermutationTable[ix & hashMask] + iy & hashMask];
			
			sample.value = h * f3;
		}
		
		
		return sample;
	}

	private static NoiseSample SimplexValue3DPart (Vector3 point, int ix, int iy, int iz) {
		float unskew = (ix + iy + iz) * (1f / 6f);
		float x 		 = point.x - ix + unskew;
		float y 		 = point.y - iy + unskew;
		float z 		 = point.z - iz + unskew;
		float f 		 = 0.5f - x * x - y * y - z * z;
		
		NoiseSample sample = new NoiseSample();
		
		if (f > 0f) {
			float f2 = f * f;
			float f3 = f * f2;
			float h  = PermutationTable[PermutationTable[PermutationTable[ix & hashMask] + iy & hashMask] + iz & hashMask];

			sample.value = h * f3;
		}
		
		return sample;
	}

	private static NoiseSample SimplexGradient1DPart (Vector3 point, int ix) {
		float x  = point.x - ix;
		float f  = 1f - x * x;
		float f2 = f * f;
		float f3 = f * f2;
		float g = gradients1D[PermutationTable[ix & hashMask] & gradientsMask1D];
		float v = g * x;
		
		NoiseSample sample = new NoiseSample();
		
		sample.value = v * f3;
		
		return sample;
	}

	private static NoiseSample SimplexGradient2DPart (Vector3 point, int ix, int iy) {
		float unskew = (ix + iy) * squaresToTriangles;
		float x  		 = point.x - ix + unskew;
		float y  		 = point.y - iy + unskew;
		float f  		 = 0.5f - x * x - y * y;

		NoiseSample sample = new NoiseSample();
		
		if (f > 0f) {
			float f2 = f * f;
			float f3 = f * f2;

			Vector2 g = gradients2D[PermutationTable[PermutationTable[ix & hashMask] + iy & hashMask]& gradientsMask2D];

			float v = Dot(g, x, y);
			
			sample.value = v * f3;
		}
		
		
		return sample;
	}

	private static NoiseSample SimplexGradient3DPart (Vector3 point, int ix, int iy, int iz) {
		float unskew = (ix + iy + iz) * (1f / 6f);
		float x 		 = point.x - ix + unskew;
		float y 		 = point.y - iy + unskew;
		float z 		 = point.z - iz + unskew;
		float f 		 = 0.5f - x * x - y * y - z * z;
		
		NoiseSample sample = new NoiseSample();
		
		if (f > 0f) {
			float f2 = f * f;
			float f3 = f * f2;
			
			Vector3 g = simplexGradients3D[PermutationTable[PermutationTable[PermutationTable[ix & hashMask] + iy & hashMask] + iz & hashMask] & simplexGradientsMask3D];

			float v = Dot(g, x, y, z);

			sample.value = v * f3;
		}
		
		return sample;
	}
}