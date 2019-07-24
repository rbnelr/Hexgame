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

	public float UnitChange;
	public float ContinentSize = 20f;
	public float LandSize = 20f;
	public float SeaLevel = 0f;

	World world;

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
	HexType GetType (float2 pos, out float height, out bool unit) {
		float continent = noise.cellular(float3(pos / ContinentSize, Seed)).y; // distance to voronoi cell edee i think
		
		// use 3d snoise z component for "seed"
		float land;
		land  = get_noise(pos, 1f / LandSize,		Seed + 0);
		land += get_noise(pos, 1f / LandSize * 2,	Seed + 1) / 2;
		land += get_noise(pos, 1f / LandSize * 4,	Seed + 2) / 8;
		
		float mountain;
		mountain  =  get_noise(pos, 1f / LandSize * 1.5f, Seed + 3) * 0.5f + 0.5f;
		mountain *= (get_noise(pos, 1f / LandSize * 8.5f, Seed + 4) * 0.5f + 0.5f) * 2;
		mountain = pow(mountain*2 + 0.7f, 1.4f) * 1;
		
		land *= mountain;
		
		height = land - continent*1.5f + 1f - SeaLevel/100;
		height /= 2;
		
		HexType type;

		if (height > 0) {
			height += 0.1f;

			type = HexType.Land;
		} else {
			type = HexType.Water;
		}

		unit = false;
		if (type == HexType.Land && UnityEngine.Random.value < UnitChange) {
			unit = true;
		}
		
		//if (abs(pos.y) > iceLine) {
		//	type = HexType.Ice;
		//}
		
		return type;
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

		for (int y=0; y<Rows; y++) {
			for (int x=0; x<Columns; x++) {
				
				float2 pos = world.GetHexPos(x,y);

				var type = GetType(pos, out float height, out bool unit);
				
				var script = Instantiate(PrefabsDict[type], float3(pos.x, 0, pos.y), Quaternion.identity, world.transform).GetComponent<Hex>();
				script.Type = type;
				script.Height = type == HexType.Water ? 0 : height;

				if (unit) {
					var u = Instantiate(UnitPrefab, script.Center, Quaternion.identity, world.transform).GetComponent<Unit>();
					script.Unit = u;
				}

				world.Hexes[y,x] = script;
			}
		}
	}
}
