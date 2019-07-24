using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class CameraMouseDrag : MonoBehaviour {
	
	public float3 CamOrbitPos = float3(50, 0, 50);

	public float MaxGrabDist = 300;

	public float MaxZoomOut = 100f;
	public float MaxZoomIn = 2f;

	public float ZoomspeedPow = 2f;
	public float ZoomspeedMultiplier = 0.01f;
	public float ZoomAnimSpeed = 0.2f;

	public float BaseFOV = 50f; // vertical
	public float FOVMultiplier = 1f;
	
	public float2 Mouserot = float2(0, 30);
	public float2 MouselookSens = 1f; // can be used to invert

	public float ZoomTarget = 0.6f; // [0,1] 0 is zoomed out
	public float Zoom = 0.6f;

	float zoomspeed => ZoomspeedMultiplier * (pow(1 - ZoomTarget, ZoomspeedPow) + 0.2f);
	
	float grabHeight;
	float2 grabPos;
	bool dragging = false;
	
	Ray mouseRay => Camera.main.ScreenPointToRay(Input.mousePosition);

	void GetGrabPos () {
		bool hit = Physics.Raycast(mouseRay.origin, mouseRay.direction, out RaycastHit info, MaxGrabDist);
		if (hit) {
			grabPos.x = info.point.x;
			grabHeight = info.point.y;
			grabPos.y = info.point.z;
		} else {
			grabHeight = 0;

			hit = RaycastMousePos(out grabPos);
		}

		dragging = hit;
	}
	bool RaycastMousePos (out float2 pos) {
		Plane BackgroundPlane = new Plane(new Vector3(0,1,0), new Vector3(0, grabHeight, 0));
		bool hit = BackgroundPlane.Raycast(mouseRay, out float enter);
		
		pos = float3(mouseRay.origin + mouseRay.direction * min(enter, MaxGrabDist)).xz;
		return hit;
	}

	Camera cam;
	void Start () {
		cam = GetComponent<Camera>();
	}

	void mousegrab () {
		if (Input.GetMouseButtonDown(0))
			GetGrabPos();
		
		if (dragging && RaycastMousePos(out float2 curPos)) {
			float2 diff = (grabPos - curPos);

			CamOrbitPos += float3(diff.x, 0, diff.y);
		}

		if (Input.GetMouseButtonUp(0))
			dragging = false;
	}

	float camHeight;
	void mousezoom () {
		
		float delta = Input.mouseScrollDelta.y;

		ZoomTarget += delta * zoomspeed;
		ZoomTarget = saturate(ZoomTarget);
		
		float animLinear = ZoomTarget - Zoom;
		animLinear = clamp((abs(animLinear) + 0.005f) * Time.deltaTime * ZoomAnimSpeed, 0, abs(animLinear)) * sign(animLinear);
		
		Zoom += animLinear;
		
		float viewSize = lerp(MaxZoomOut, MaxZoomIn, Zoom);
		
		float baseCamSize = tan(radians(BaseFOV) / 2) * 2;
		camHeight = viewSize / baseCamSize;

		cam.fieldOfView = BaseFOV * FOVMultiplier;
	}

	float2 mouseDelta => Input.GetMouseButton(1) ? float2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) : 0;
	
	void mouserotate () {
		Mouserot += mouseDelta * MouselookSens;
		Mouserot.y = clamp(Mouserot.y, 5, 90);
		Mouserot.x = Mouserot.x % 360;

		transform.eulerAngles = float3(90 -Mouserot.y, Mouserot.x, 0);
	}

	void Update () {
		mouserotate();
		mousezoom();
		transform.position = CamOrbitPos - (float3)transform.forward * camHeight; // apply camera position based on rotate and zoom

		mousegrab(); // use new cam pos to raycast
		transform.position = CamOrbitPos - (float3)transform.forward * camHeight; // apply camera position from dragging
	}
}
