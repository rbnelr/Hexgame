using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class CameraMouseDrag : MonoBehaviour {

	//public Plane BackgroundPlane = new Plane(new Vector3(0,1,0), 0);
	public float MaxGrabDist = 300;

	bool GetMousePos (out float2 pos) {
		Plane BackgroundPlane = new Plane(new Vector3(0,1,0), 0);

		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		bool hit = BackgroundPlane.Raycast(r, out float enter);
		pos = float3(r.origin + r.direction * min(enter, MaxGrabDist)).xz;
		return hit;
	}

	void Start () {

	}
	
	float2 grabPos;

	void Update () {
		if (GetMousePos(out float2 curPos)) {
			if (Input.GetMouseButtonDown(0)) {
				grabPos = curPos;
			}

			if (Input.GetMouseButton(0)) {
				float2 diff = grabPos - curPos;

				transform.Translate(float3(diff.x, 0, diff.y), Space.World);
			}

		}

	}
}
