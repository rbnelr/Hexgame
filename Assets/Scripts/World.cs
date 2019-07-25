using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class World : MonoBehaviour {
	public GameObject SelectionIndicator;

	public int Columns = 20;
	public int Rows = 10;

	public float Width => Columns;
	public float Height => Rows * Hex.RADIUS_FACTOR;

	public float2 GetHexPos (int x, int y) {
		return float2(x + (y % 2)/2f + 0.5f, y * Hex.RADIUS_FACTOR + 0.5f);
	}

	public Hex[,] Hexes;

	[Range(-100, 100)]
	public int Wrap = 0;
	int prevWrap = 0;
	
	Camera cam => Camera.main;
	TopDownCamera _tdcam;
	TopDownCamera tdcam { get {
		if (_tdcam == null)
			_tdcam = cam.GetComponent<TopDownCamera>();
		return _tdcam;
	} }

	void Start () {
		
	}

	void Update () {
		UpdateWrapping();
		mouseSelect();
	}

	void mouseSelect () {
		RaycastHit hitInfo = default;
		bool hit = Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo);
		if (hit) {
			var hex  = hitInfo.transform.gameObject.GetComponentInParent<Hex>();
			var unit = hitInfo.transform.gameObject.GetComponentInParent<Unit>();
			hit = hex || unit;

			if (hit) {
				SelectionIndicator.transform.position = (hex ?? unit.Hex).transform.position;
			}
		}
		SelectionIndicator.SetActive(hit);
	}

	public static int wrap (int x, int range, out int divided) { // assumes positive range
		divided = x / range;
		int remainder = x % range;
		if (remainder < 0) {
			remainder += range;
			divided -= 1;
		}
		return remainder;
	}
	
	public void UpdateWrapping () {
		tdcam.MoveInstantly(float3(tdcam.CamOrbitPos.x % Width, tdcam.CamOrbitPos.y, tdcam.CamOrbitPos.z));
		Wrap = (int)round(tdcam.CamOrbitPos.x);

		if (prevWrap == Wrap)
			return;
		prevWrap = Wrap;
		
		int wrapped = wrap(Wrap - Columns/2, Columns, out int allOffest);

		for (int y=0; y<Rows; y++) {
			for (int x=0; x<Columns; x++) {
				float2 pos = GetHexPos(x,y);

				int offset = allOffest;
				if (x < wrapped)
					offset += 1;
				pos.x += offset * Width;

				Hexes[y,x].transform.localPosition = float3(pos.x, Hexes[y,x].Height, pos.y);
			}
		}
	}
}
