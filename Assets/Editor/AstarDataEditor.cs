//    © Copyright 2009 Aron Granberg
//    AstarPathEditor.cs script is licenced under a Creative Commons Attribution-Noncommercial 3.0 Unported License.
//    If you want to use the script in commercial projects, please contact me at aron.g@me.com

using UnityEngine;
using System.Collections;
using UnityEditor;
using AstarClasses;
using AstarMath;

[CustomEditor (typeof (AstarData))]
public class AstarDataEditor : Editor {
	
	//public override void OnInspectorGUI () {
	//	GUILayout.Label ("Astar Data");
	//}
	
	[MenuItem( "Assets/Create/AstarData" )]
	public static void CreateAstarData()
	{
		AstarData asset;
		string name = "AstarData";
		int nameIdx = 0;
		
		while( System.IO.File.Exists( Application.dataPath + "/" + name + nameIdx + ".asset" ) )
		{
			nameIdx++;
		}

		asset = new AstarData();
		//asset.Data = ( new CollectionAsset() ).GetData();
		AssetDatabase.CreateAsset( asset, "Assets/" + name + (nameIdx != 0 ? ""+nameIdx : "") + ".asset" );
		
		//EditorUtility.FocusProjectView();
		Selection.activeObject = asset;
	}
}
