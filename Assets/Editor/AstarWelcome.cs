using UnityEngine;
using System.Collections;
using UnityEditor;

//Initialize AstarPath

public class AstarWelcome : EditorWindow {
    
    int step = 0;
    
    public Texture astarLogo;
    public GUISkin gskin;
    
    public bool checkForUpdates = true;
    
    // Add menu named "My Window" to the Window menu
    [MenuItem ("Window/A*/Welcome")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        AstarWelcome window = (AstarWelcome)EditorWindow.GetWindow (typeof (AstarWelcome),true,"Welcome");
     	window.position = new Rect (350,200,550,320);
     	//window.ShowUtility ();
    }
    
    public void OnEnable () {
    	checkForUpdates = EditorPrefs.GetBool ("CheckForUpdates",true);
    }
    
    void OnGUI () {
    	
    	if (astarLogo == null) {
    		astarLogo = EditorGUIUtility.Load ("AstarSkin/astar.png") as Texture;
    	}
    	if (gskin == null) {
    		gskin = EditorGUIUtility.Load ("AstarSkin/AstarSkin.GUISkin") as GUISkin;
    	}
    	GUI.skin = gskin;
    	
    	GUI.color = new Color (1,1,1,0.1F);
    	GUI.contentColor = Color.white;
    	GUI.DrawTexture (new Rect (position.width/2-100,position.height/2-100,200,200),astarLogo);
    	GUI.color = Color.white;
    	GUILayout.BeginHorizontal ();
    	GUILayout.FlexibleSpace ();
    	GUILayout.BeginVertical ();
    	GUILayout.Label ("Welcome To The A* Pathfinding Project","VeryLargeLabel");
    	GUILayout.EndVertical ();
    	GUILayout.FlexibleSpace ();
    	GUILayout.EndHorizontal ();
    	
    	switch (step) {
    		case 0:
    			GUILayout.BeginHorizontal ();
    			GUILayout.FlexibleSpace ();
    			GUILayout.BeginVertical ();
    			GUILayout.Space (10);
    			GUILayout.Label ("First we need to define some (one) settings to get going","LargeLabel");
    			GUILayout.Space (25);
    			
    			GUI.skin = null;
    			
    			GUILayout.BeginHorizontal ();
    			GUILayout.Label ("Check for updates automatically",EditorStyles.boldLabel);
    			checkForUpdates = GUILayout.Toolbar (checkForUpdates ? 1 : 0, new string[2] {"No","Yes"}) == 1;
    			GUILayout.EndHorizontal ();
    			GUILayout.Label ("The A* Editor will try to check for new updates (only in the editor)",EditorStyles.boldLabel);
    			
    			GUILayout.EndVertical ();
    			GUILayout.FlexibleSpace ();
    			GUILayout.EndHorizontal ();
    			
    			GUILayout.FlexibleSpace ();
    			GUILayout.BeginHorizontal ();
    			GUILayout.Label ("Made by Aron Granberg",EditorStyles.boldLabel);
    			GUILayout.FlexibleSpace ();
    			
    			if (GUILayout.Button ("Next",GUILayout.Width (100))) {
    				EditorPrefs.SetBool ("AstarInitialized",true);
    				EditorPrefs.SetBool ("CheckForUpdates",checkForUpdates);
    				step = 1;
    			}
    			
    			GUILayout.EndHorizontal ();
    			
    			break;
    		case 1:
    			GUILayout.BeginHorizontal ();
    			GUILayout.FlexibleSpace ();
    			GUILayout.BeginVertical ();
    			GUILayout.Space (10);
    			GUILayout.Label ("Great! You are now ready to begin setting up your pathfinding","LargeLabel");
    			GUILayout.Space (25);
    			
    			GUI.skin = null;
    			
    			if (GUILayout.Button ("A* Homepage")) {
    				Help.BrowseURL ("http://www.arongranberg.com/unity/a-pathfinding");
    			}
    			GUILayout.Space (10);
    			if (GUILayout.Button ("Documentation")) {
    				Help.BrowseURL ("http://www.arongranberg.com/unity/a-pathfinding/docs/");
    			}
    			GUILayout.Space (10);
    			if (GUILayout.Button ("You can find a get-started guide here")) {
    				Help.BrowseURL ("http://www.arongranberg.com/unity/a-pathfinding/docs/get-started");
    			}
    			
    			GUILayout.EndVertical ();
    			GUILayout.FlexibleSpace ();
    			GUILayout.EndHorizontal ();
    			
    			GUILayout.FlexibleSpace ();
    			GUILayout.BeginHorizontal ();
    			GUILayout.Label ("Made by Aron Granberg",EditorStyles.boldLabel);
    			GUILayout.FlexibleSpace ();
    			
    			if (GUILayout.Button ("Close",GUILayout.Width (100))) {
    				Close ();
    				step = 0;
    			}
    			
    			GUILayout.EndHorizontal ();
    			break;
    	}
    }
}
