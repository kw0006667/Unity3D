using UnityEngine;
using System.Collections;
using AstarClasses;

[System.Serializable]
public class AstarData : ScriptableObject {
	public int x = 0;
	
	[SerializeField]
	public Int3 test;
	
	[SerializeField]
	public SerializedNode[] staticNodes;
	public Grid grid;
	
} 

/*public class AstarDataImporter : AssetImporter{
	public void OnGUI () {
		GUILayout.Label ("Hi");
	}
	public void OnInspectorGUI () {
		GUILayout.Label ("Hi");
	}
}

public class AstarDataPostProcessor : AssetPostprocessor {
	public void OnPostprocessAllAssets () {
		if (assetPath.IndexOf("astarData") >= 0) {
			assetImporter = new AstarDataImporter ();
		}
		
	}
}*/