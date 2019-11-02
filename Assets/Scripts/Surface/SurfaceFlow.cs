using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SurfaceFlow : MonoBehaviour {

	public SurfaceCreator surface;

	private ParticleSystem system;
	private ParticleSystem.Particle[] particles;
}