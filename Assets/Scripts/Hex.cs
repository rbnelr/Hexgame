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
	public float Height => transform.localPosition.y;

	public Unit Unit = null;

	void Start () {
		
	}
	
	void Update () {

	}
}
