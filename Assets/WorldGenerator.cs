using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;
using System;
using System.Linq;

public class WorldGenerator : MonoBehaviour {
	
	public int Width = 20;
	public int Height = 10;

	public bool RandomSeed = true;
	public int Seed;

	public float MountainChance;
	public float ContinentSize = 20f;
	public float LandSize = 20f;
	public float SeaLevel = 0f;

	GameObject worldPrefab;
	World world;

	[Serializable]
	public struct HexTypePrefab {
		public HexType type;
		public GameObject prefab;
	}
	public HexTypePrefab[] Prefabs;
	public Dictionary<HexType, GameObject> PrefabsDict;

	void Start () {
		worldPrefab = new GameObject("World");
		worldPrefab.AddComponent<World>();
		
		PrefabsDict = Prefabs.ToDictionary(x => x.type, x => x.prefab);
		
		GenerateWorld();
	}
	
	void Update () {

	}

	HexType GetType (int x, int y, out float height) {
		float2 pos = float2(x,y) - float2(Width, Height)/2;
		
		float continent = noise.cellular(float3(pos / ContinentSize, Seed)).y; // distance to voronoi cell edee i think

		// use 3d snoise z component for "seed"
		float land = noise.cnoise(float3(pos / LandSize, Seed + 0));
		land += noise.cnoise(float3(pos / LandSize * 2, Seed + 1)) / 2;
		land += noise.cnoise(float3(pos / LandSize * 4, Seed + 2)) / 8;

		float mountain = noise.cnoise(float3(pos / LandSize * 1.5f, Seed + 3)) * 0.5f + 0.5f;
		mountain *= (noise.cnoise(float3(pos / LandSize * 8.5f, Seed + 4)) * 0.5f + 0.5f) * 2;
		mountain = pow(mountain*2 + 0.7f, 1.4f) * 1;

		land *= mountain;

		height = land - continent*1.5f + 1f - SeaLevel/100;
		
		HexType type;

		if (height > 0) {
			height += 0.1f;

			type = HexType.Land;
		} else {
			type = HexType.Water;
		}

		if (type == HexType.Land && UnityEngine.Random.value < MountainChance) {
			type = HexType.Mountain;
		}
		
		//if (abs(pos.y) > iceLine) {
		//	type = HexType.Ice;
		//}
		
		return type;
	}

	public void GenerateWorld () {
		if (world != null)
			Destroy(world.gameObject);

		world = Instantiate(worldPrefab, null).GetComponent<World>();
		world.Width = Width;
		world.Height = Height;

		if (RandomSeed)
			Seed = UnityEngine.Random.Range(0, 2 << 15);

		for (int y=0; y<Height; y++) {
			for (int x=0; x<Width; x++) {
				
				var type = GetType(x,y, out float height);
				
				float3 pos = float3(x*2 + y % 2, 0, y*2 * Hex.RADIUS_FACTOR);

				var script = Instantiate(PrefabsDict[type], pos, Quaternion.identity, world.transform).GetComponent<Hex>();
				script.Type = type;
				script.Height = type == HexType.Water ? 0 : height;
			}
		}
	}
}
