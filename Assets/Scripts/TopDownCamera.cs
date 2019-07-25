using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class TopDownCamera : MonoBehaviour {
	
	public int DragButton = 0;
	public int MouselookButton = 1;

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
	
	float3 grabPos;
	bool dragging = false;

	public void MoveInstantly (float3 newCamOrbitPos) { // The world controller wants to wrap the camera position.x, but when we are dragging this does not work by simply changing CamOrbitPos
		float3 offset = newCamOrbitPos - CamOrbitPos;
		CamOrbitPos = newCamOrbitPos;
		grabPos += offset;

		applyCamPos(); // reapply cam pos, to prevent script order dependenence (cam will jump on cam move if cam update was called before this)
	}

	void applyCamPos () {
		transform.position = CamOrbitPos - (float3)transform.forward * camHeight;
	}
	
	Ray mouseRay => Camera.main.ScreenPointToRay(Input.mousePosition);

	void GetGrabPos () {
		bool hit = Physics.Raycast(mouseRay.origin, mouseRay.direction, out RaycastHit info, MaxGrabDist);
		if (hit) {
			grabPos = info.point;
		} else {
			grabPos.y = 0;

			hit = RaycastMousePos(out grabPos);
		}

		dragging = hit;
	}
	bool RaycastMousePos (out float3 pos) {
		Plane BackgroundPlane = new Plane(new Vector3(0,1,0), new Vector3(0, grabPos.y, 0));
		bool hit = BackgroundPlane.Raycast(mouseRay, out float enter);
		
		pos = float3(mouseRay.origin + mouseRay.direction * min(enter, MaxGrabDist));
		pos.y = grabPos.y;
		return hit;
	}

	Camera cam;
	void Start () {
		cam = GetComponent<Camera>();
	}

	void mousegrab () {
		if (Input.GetMouseButtonDown(DragButton))
			GetGrabPos();
		
		if (dragging && RaycastMousePos(out float3 curPos)) {
			float3 diff = (grabPos - curPos);

			CamOrbitPos += float3(diff.x, 0, diff.z);
		}

		if (Input.GetMouseButtonUp(DragButton))
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
	
	float2 mouseDelta => float2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
	
	void mouserotate () {
		bool mouselook = Input.GetMouseButton(MouselookButton);
		
		Cursor.lockState = mouselook ? CursorLockMode.Confined : CursorLockMode.None;
		Cursor.visible = !mouselook;
		
		if (mouselook) {
			Mouserot += mouseDelta * MouselookSens;
			Mouserot.y = clamp(Mouserot.y, 5, 90);
			Mouserot.x = Mouserot.x % 360;
		}
		transform.eulerAngles = float3(90 -Mouserot.y, Mouserot.x, 0);
	}

	void Update () {
		mouserotate();
		mousezoom();
		applyCamPos(); // apply camera position based on rotate and zoom

		mousegrab(); // use new cam pos to raycast
		applyCamPos(); // apply camera position from dragging
	}
}
