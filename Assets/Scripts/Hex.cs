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
	
	public float3 Position {
		get => transform.localPosition;
		set { transform.localPosition = value; }
	}
	public float2 Position2D {
		get => ((float3)transform.localPosition).xz;
		set { transform.localPosition = float3(value.x, Height, value.y); }
	}

	public HexType Type;
	public float Height {
		get => transform.localPosition.y;
		set { transform.localPosition = float3(transform.localPosition.x, value, transform.localPosition.z); }
	}

	public Unit Unit = null; // Unit standing on this tile

	void Start () {
		
	}
	
	void Update () {

	}
}
