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
	public float IceThickness = 0.05f;

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
	HexType GetType (float2 pos, out float height, out bool unit) {
		int seed = Seed;

		float continent = noise.cellular(float3(pos / ContinentSize, seed++)).y; // distance to voronoi cell edee i think
		
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
		bool mountain_spawned = mountainous && get_noise(pos, 1f / 1.5f, seed++) > 0.2f;
		
		land *= clamp(mountain, 0.5f, 2f);

		height = land - continent*1.5f + 1f - SeaLevel/100;
		height /= 2;
		
		float ice;
		ice  = get_noise(pos, 1f /   4f, seed++) * 0.5f;
		ice += get_noise(pos, 1f / 1.3f, seed++) * 1.2f - 0.3f;
		ice = (world.Height * IceThickness + 1f) - min(pos.y, world.Height - pos.y) + (ice * world.Height * IceThickness);

		HexType type = HexType.Water;

		if (height > 0) {
			height += 0.1f;
			
			type = mountain_spawned ? HexType.Mountain : HexType.Land;
		}
		
		if (ice > 0f) {
			type = HexType.Ice;
			height = max(height, 0.1f);
		}

		unit = false;
		if (type == HexType.Land && UnityEngine.Random.value < UnitChange) {
			unit = true;
		}
		
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

				var type = GetType(pos, out float height, out bool spawnUnit);
				
				if (type == HexType.Water)
					height = 0;

				var ori = Quaternion.AngleAxis(60 * UnityEngine.Random.Range(0, 6), Vector3.up);
				var hex = Instantiate(PrefabsDict[type], float3(pos.x, height, pos.y), ori, world.transform).GetComponent<Hex>();
				hex.Type = type;

				if (spawnUnit) {
					var unit = Instantiate(UnitPrefab, hex.transform, false).GetComponent<Unit>();
					hex.Unit = unit;
					unit.Hex = hex;
				}

				world.Hexes[y,x] = hex;
			}
		}
	}
}
