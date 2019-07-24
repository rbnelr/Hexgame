using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor {
	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		WorldGenerator script = (WorldGenerator)target;
		if (GUILayout.Button("Regenerate")) {
			script.GenerateWorld();
		}
	}
}
