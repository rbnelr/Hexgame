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

	void Start () {

	}

	void Update () {
		wrappingTest();
		mouseSelect();
	}

	void mouseSelect () {
		bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
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

	void wrappingTest () {
		if (prevWrap == Wrap)
			return;
		prevWrap = Wrap;
		
		int wrapped = wrap(Wrap, Columns, out int allOffest);

		for (int y=0; y<Rows; y++) {
			for (int x=0; x<Columns; x++) {
				float2 pos = GetHexPos(x,y);

				int offset = allOffest;
				if (x < wrapped)
					offset += 1;
				pos.x += offset * Width;

				Hexes[y,x].transform.localPosition = float3(pos.x, 0, pos.y);
			}
		}
	}
}
