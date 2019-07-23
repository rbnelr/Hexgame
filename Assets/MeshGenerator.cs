using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static Unity.Mathematics.math;

public class MeshGenerator : MonoBehaviour {
	public Mesh HexMesh;
	public float HexRadius = 1f; // center to edge
	public float HexHeight = 1.5f;
	public float HexBevel = 0.05f;
	
	void Start () {
		CalcHex();
	}
	
	void Update () {
		//CalcHex();
	}

	void CalcHex () {
		
		HexMesh = new Mesh();
		HexMesh.name = "Hex";

		var verts = new List<Vector3>();
		var tris = new List<int>();
		
		for (int i = 0; i < 6; ++i) {
			float ang0 = (i + 0) / 6f * PI*2 + PI/2;
			float ang1 = (i + 1) / 6f * PI*2 + PI/2;

			float2 dir0 = float2(cos(ang0), sin(ang0));
			float2 dir1 = float2(cos(ang1), sin(ang1));
			
			float r = HexRadius / Hex.RADIUS_FACTOR;

			tris.Add(verts.Count);
			verts.Add(float3(0));
			tris.Add(verts.Count);
			verts.Add(float3(dir1 * (r - HexBevel), 0).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir0 * (r - HexBevel), 0).xzy);
			
			tris.Add(verts.Count);
			verts.Add(float3(dir0 * r, -HexBevel).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir0 * (r - HexBevel), 0).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir1 * r, -HexBevel).xzy);
			
			tris.Add(verts.Count);
			verts.Add(float3(dir1 * r, -HexBevel).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir0 * (r - HexBevel), 0).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir1 * (r - HexBevel), 0).xzy);
			
			tris.Add(verts.Count);
			verts.Add(float3(dir1 * r, -HexHeight).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir0 * r, -HexHeight).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir1 * r, -HexBevel).xzy);
			
			tris.Add(verts.Count);
			verts.Add(float3(dir1 * r, -HexBevel).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir0 * r, -HexHeight).xzy);
			tris.Add(verts.Count);
			verts.Add(float3(dir0 * r, -HexBevel).xzy);
		}

		HexMesh.SetVertices(verts);
		HexMesh.SetTriangles(tris, 0);
		HexMesh.RecalculateNormals();

		GetComponent<MeshFilter>().mesh = HexMesh;

		AssetDatabase.CreateAsset(HexMesh, "Assets/HexMesh.mesh");
		AssetDatabase.SaveAssets();
	}
}
