using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public enum HexType {
	Water,
	Ice,
	Land,
	Mountain,
};

public class Hex : MonoBehaviour {
	public const float RADIUS_FACTOR = 0.8660254f; // if distance from center to center of hex edge is one, this is the radius to the corners cos(60 deg / 2)

	public float3 Center => transform.localPosition;
	public HexType Type;
	public float Height;

	void Start () {
		GetComponentInChildren<MeshFilter>().transform.localPosition = float3(0, Height, 0);
	}
	
	void Update () {

	}
}
