using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class CameraMouseDrag : MonoBehaviour {

	//public Plane BackgroundPlane = new Plane(new Vector3(0,1,0), 0);
	public float MaxGrabDist = 300;

	public float MaxZoomOut = 100f;
	public float MaxZoomIn = 2f;

	public float ZoomspeedPow = 2f;
	public float ZoomspeedMultiplier = 0.01f;
	public float ZoomAnimSpeed = 0.2f;

	public float BaseFOV = 50f; // vertical
	public float FOVMultiplier = 1f;
	
	float2 grabPos;

	float zoomTarget = 0.6f; // [0,1] 0 is zoomed out
	float zoom = 0.6f;

	float zoomspeed => ZoomspeedMultiplier * (pow(1 - zoomTarget, ZoomspeedPow) + 0.2f);

	bool GetMousePos (out float2 pos) {
		Plane BackgroundPlane = new Plane(new Vector3(0,1,0), 0);

		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		bool hit = BackgroundPlane.Raycast(r, out float enter);
		pos = float3(r.origin + r.direction * min(enter, MaxGrabDist)).xz;
		return hit;
	}

	Camera cam;
	void Start () {
		cam = GetComponent<Camera>();
	}

	void mousegrab () {
		if (GetMousePos(out float2 curPos)) {
			if (Input.GetMouseButtonDown(0)) {
				grabPos = curPos;
			}

			if (Input.GetMouseButton(0)) {
				float2 diff = (grabPos - curPos) * 0.98f;

				transform.Translate(float3(diff.x, 0, diff.y), Space.World);
			}

		}
	}

	void mousezoom () {
		
		float delta = Input.mouseScrollDelta.y;

		zoomTarget += delta * zoomspeed;
		zoomTarget = saturate(zoomTarget);
		
		float animLinear = zoomTarget - zoom;
		animLinear = clamp((abs(animLinear) + 0.005f) * Time.deltaTime * ZoomAnimSpeed, 0, abs(animLinear)) * sign(animLinear);
		
		zoom += animLinear;
		
		float viewSize = lerp(MaxZoomOut, MaxZoomIn, zoom);

		float baseCamSize = tan(radians(BaseFOV) / 2) * 2;
		float camHeight = viewSize / baseCamSize;

		transform.localPosition = float3(transform.localPosition.x, camHeight, transform.localPosition.z);
		
		cam.fieldOfView = BaseFOV * FOVMultiplier;
	}

	void mouserotate () {

	}

	void Update () {
		mousezoom();
		mousegrab();
		mouserotate();
	}
}
