using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;
using System;
using System.Linq;

public class WorldGenerator : MonoBehaviour {
	
	public int Columns = 20;
	public int Rows = 10;

	public bool RandomSeed = true;
	public int Seed;

	public float UnitChance;
	public float ContinentSize = 20f;
	public float LandSize = 20f;
	public float IceThickness = 0.05f;
	public float LandPercentage = 45f;

	World world;

	public float _Columns { set { Columns = (int)value; } }
	public float _Rows { set { Rows = (int)value; } }

	[Serializable]
	public struct HexTypePrefab {
		public HexType type;
		public GameObject prefab;
	}
	public HexTypePrefab[] Prefabs;
	public Dictionary<HexType, GameObject> PrefabsDict;
	
	public GameObject WorldPrefab;
	public GameObject UnitPrefab;

	void Start () {
		PrefabsDict = Prefabs.ToDictionary(x => x.type, x => x.prefab);
		
		GenerateWorld();
	}
	
	void Update () {

	}

	// cyclic_noise().x repeats after xperiod (cyclic_noise((0,n)).x == cyclic_noise((xperiod,n)).x)
	// does not repeat in y
	// frequency is the frequency of the noise features, this is unrelated to the repeating
	float cyclic_noise (float2 pos, float freq, float xperiod, int seed) {
		pos.y *= freq;
		float ang = pos.x / xperiod * PI*2; // one revolution for one xperiod
		float radius = xperiod * freq / (PI*2);
		float2 circ = float2(cos(ang), sin(ang)) * radius;
		float4 inp = float4(circ, pos.y, seed);
		return noise.cnoise(inp);
	}
	float get_noise (float2 pos, float freq, int seed) {
		return cyclic_noise(pos, freq, world.Width, seed);
	}

	struct GeneratedHex {
		public float2 pos;
		public HexType type;
		public float height;
		public bool unit;
	};
	GeneratedHex GenerateHex (float2 pos) {
		var hex = new GeneratedHex { pos = pos };

		int seed = Seed;

		//float continent = noise.cellular(float3(pos / ContinentSize, seed++)).y; // distance to voronoi cell edee i think
		float continent = get_noise(pos, 1f / ContinentSize, seed++) - 1f;
		
		// use 3d snoise z component for "seed"
		float land;
		land  = get_noise(pos, 1f / LandSize,		seed++);
		land += get_noise(pos, 1f / LandSize * 2,	seed++) / 2;
		land += get_noise(pos, 1f / LandSize * 4,	seed++) / 8;
		
		float mountain;
		mountain  =  get_noise(pos, 1f / LandSize * 1.5f, seed++) * 0.5f + 0.5f;
		mountain *= (get_noise(pos, 1f / LandSize * 8.5f, seed++) * 0.5f + 0.5f) * 2;
		mountain = pow(mountain*2 + 0.7f, 1.3f);
		
		bool mountainous = mountain > 2.5f;
		
		hex.type = HexType.Land;

		if (mountainous && get_noise(pos, 1f / 1.5f, seed++) > 0.2f)
			hex.type = HexType.Mountain;

		land *= clamp(mountain, 0.5f, 2f);

		hex.height = land + continent;
		hex.height /= 2;
		
		float ice;
		ice  = get_noise(pos, 1f /   4f, seed++) * 0.5f;
		ice += get_noise(pos, 1f / 1.3f, seed++) * 1.2f - 0.3f;
		ice = (world.Height * IceThickness + 1f) - min(pos.y, world.Height - pos.y) + (ice * world.Height * IceThickness);
		
		if (ice > 0f) {
			hex.type = HexType.Ice;
		}

		return hex;
	}
	void SpawnHex (int x, int y, GeneratedHex h) {
		
		if (h.type == HexType.Ice)
			h.height = max(h.height, 0.05f);

		//if (h.height >= 0) {
		//	h.height += 0.1f;
		//} else {
		//	h.type = HexType.Water;
		//	h.height = 0;
		//}

		h.unit = false;
		if (h.type == HexType.Land && UnityEngine.Random.value < UnitChance) {
			h.unit = true;
		}

		var ori = Quaternion.AngleAxis(60 * UnityEngine.Random.Range(0, 6), Vector3.up);
		var hex = Instantiate(PrefabsDict[h.type], float3(h.pos.x, h.height, h.pos.y), ori, world.transform).GetComponent<Hex>();
		hex.Type = h.type;

		if (h.unit) {
			var unit = Instantiate(UnitPrefab, hex.transform, false).GetComponent<Unit>();
			hex.Unit = unit;
			unit.Hex = hex;
		}

		world.Hexes[y,x] = hex;
	}

	public void GenerateWorld () {
		if (world != null)
			Destroy(world.gameObject);

		world = Instantiate(WorldPrefab, null).GetComponent<World>();
		world.Columns = Columns;
		world.Rows = Rows;
		world.Hexes = new Hex[Rows, Columns];

		if (RandomSeed)
			Seed = UnityEngine.Random.Range(0, 2 << 15);
		
		var hexes = new GeneratedHex[Rows,Columns];
		var sortedHexes = new List<int2>(Rows * Columns);
		
		for (int y=0; y<Rows; y++) {
			for (int x=0; x<Columns; x++) {
				hexes[y,x] = GenerateHex( world.GetHexPos(x,y) );
				sortedHexes.Add(int2(x,y));
			}
		}

		{ // place water, to match LandPercentage
			float land = LandPercentage / 100f;

			sortedHexes.Sort((l, r) => {
				var a = hexes[l.y, l.x];
				var b = hexes[r.y, r.x];
				return a.height.CompareTo(b.height);
			});

			int lowestLandHex = (int)floor(sortedHexes.Count * (1 - land));

			int2 p = sortedHexes[lowestLandHex];
			float seaLevel = hexes[p.y, p.x].height;
			seaLevel -= 0.1f;

			for (int i=0; i<lowestLandHex; ++i) {
				p = sortedHexes[i];
				if (hexes[p.y,p.x].type != HexType.Ice) {
					hexes[p.y,p.x].type = HexType.Water;
					hexes[p.y,p.x].height = 0.0f;
				}
			}
			for (int i=lowestLandHex; i<sortedHexes.Count; ++i) {
				p = sortedHexes[i];
				hexes[p.y,p.x].height -= seaLevel;
			}
		}

		for (int y=0; y<Rows; y++) {
			for (int x=0; x<Columns; x++) {
				SpawnHex(x,y, hexes[y,x]);
			}
		}

		world.UpdateWrapping();
	}
}
