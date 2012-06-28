using UnityEngine;
using System.Collections;
using AstarClasses;
using AstarMath;
using AstarProcess;

[AddComponentMenu ("Pathfinding/Seeker")]
public class Seeker : MonoBehaviour {
	//The path the script is currently proccessing
	private AstarPath.Path path;
	
	//Should the path get debugged in the scene view during runtime
	public bool debugPath = false;
	public bool onlyDebugSmoothedPath = false;
	
	//This is the max slope the unit can travel on
	public float maxAngle = 90;
	
	//The cost of moving in a slope, use a higher cost to make it avoid slopes
	public float angleCost = 0;
	
	//If true, removes the first node on the path, reduces jittering
	public bool removeFirst = true;
	
	//Makes the path get calculated when searching at most one node per frame, good for debugging.
	public bool stepByStep = false;
	
	//The AstarScript will clamp the end point to the nearest node, should the seeker script replace the last node with the end point specified when calling the StartPath function
	public RealStart startPoint = RealStart.Snapped;
	public RealEnd endPoint = RealEnd.Snapped;
	//What should the script do when the script couldn't find a path to the target
	public OnError onError = OnError.ErrorMessage;
	
	//Should we only search on one of the grids or should it use all grids?
	public GridSelection gridSelection = GridSelection.Auto;
	
	//If Grid Selection is set to Fixed, this will indicate which grid will be used
	public int grid = 0;
	
	//This is still at beta stage, see below
	//private float turningRadius = 1.0F;
	
	//Warning this might crash the game on windows computers - Disabled due to a high possability of crashing
	//public bool multithread;
	
	public bool IsPathfindingDone () {
		return isDone;
	}
	
	private bool isDone = false;
	
	public PostProcessor postProcessor;
	public int subdivisions = 3;
	public bool bezierNormalizeTangents = true;
	public float bezierTangentLength = 0.5F;
	public int simpleSmoothIterations = 5;
	public float simpleSmoothStrength = 0.7F;
	
	public enum PostProcessor {
		None,
		CubicBezier,
		CatmullRom,
		SimpleSmooth
	}
	
	public enum GridSelection {
		Auto,
		Fixed
	}
	
	public enum OnError {
		None,
		ErrorMessage,
		EmptyArray
	}
	
	public enum RealStart {
		Snapped,
		Exact
	}
	
	public enum RealEnd {
		Snapped,
		Exact,
		AddExact
	}
	
	//The end/start point which were used when the StartPath function was called
	private Vector3 endpos;
	private Vector3 startpos;
	
	//The last path
	private Vector3 [] pathPoints;

	//Check if the path is finnished, see OnCompleteLater
	/*public IEnumerator MultithreadingCompleteCheck () {
		while (!isComplete) {
			yield return 0;
		}
		isDone = false;
		OnComplete (path);
	}
	
	//This is called instaed of OnComplete when using multithreading. Other thread can't call Unity specific stuff, so we have to set a flag that the path is finnished and then call OnComplete next time we check if it is complete (in Update).
	public void OnCompleteLater (AstarPath.Path p) {
		
		if (path != p) {
			return;
		}
		
		isComplete = true;
	}*/
	
	//This function will be called when the pathfinding is complete, it will be called when the pathfinding returned an error too.
	public void OnComplete (AstarPath.Path p) {
		//If the script was already calculating a path when a new path started calculating, then it would have canceled the first path, that will generate an OnComplete message from the first path, this makes sure we only get OnComplete messages from the path we started calculating latest.
		if (path != p) {
			return;
		}
		
		isDone = true;
		
		//What should we do if the path returned an error (there is no available path to the target).
		if (path.error) {
			switch (onError) {
				case OnError.None:
					return;
				case OnError.ErrorMessage:
					SendMessage("PathError",SendMessageOptions.DontRequireReceiver);
					break;
				case OnError.EmptyArray:
					SendMessage("PathComplete",new Vector3[0],SendMessageOptions.DontRequireReceiver);
					break;
			} 
			return;
		}
		
		if (path.path == null) {
			Debug.LogError ("The 'Path' array is not assigned - System Error - Please send a bug report - Include the following info:\nError = "+p.error+"\nStart = "+startpos+"\nEnd = "+endpos+"\nFound End = "+p.foundEnd+"\nError = "+p.error);
		}
		
		//Still at beta stage, can crash the game
		//if (postProcessEdges) {
			//Vector3[] points = AstarProcess.PostProcess.NavigationMesh (p.path,AstarPath.active.meshNodePosition == MeshNodePosition.Edge, startpos, endpos,turningRadius);
			//(FindObjectOfType (typeof(Clicker)) as Clicker).NavigationMesh (p.path,AstarPath.active.meshNodePosition == MeshNodePosition.Edge, startpos, endpos,turningRadius);
		//}
		
		if(path.path.Length > 1) {
			//Convert the Node array to a Vector3 array, subract one from the array if Remove First is true and add one to the array if Use Real End is set to Add
			Vector3[] a = new Vector3[path.path.Length - (removeFirst ? 1 : 0) + (endPoint == RealEnd.AddExact && !path.forceEndSnap ? 1 : 0)];
			
			for (int i=0;i<path.path.Length;i++) {
				//Ignore the first node if Remove First is set to True
				if (removeFirst && i==0) {
					continue;
				}
				
				a[i - (removeFirst ? 1 : 0)] = path.path[i].vectorPos;
			}
			
			if (startPoint == RealStart.Exact && !path.forceStartSnap) {
				a[0] = startpos;
			}
			
			//Assign the endpoint
			if ((endPoint == RealEnd.AddExact || endPoint == RealEnd.Exact) && !path.forceEndSnap) {
				a[a.Length-1] = endpos;
			}
			
			//Store the path in a variable so it can be drawn in the scene view for debugging
			pathPoints = a;
			
			//Post-process the path using splines if the user has choosen to do that (default is None)
			switch (postProcessor) {
				case PostProcessor.CubicBezier:
					pathPoints = PostProcessSplines.CubicSmooth (pathPoints,subdivisions,bezierNormalizeTangents,bezierTangentLength);
					break;
				case PostProcessor.CatmullRom:
					pathPoints = PostProcessSplines.CatmullRom (pathPoints,subdivisions);
					break;
				case PostProcessor.SimpleSmooth:
					pathPoints = PostProcessSplines.SimpleSmooth (pathPoints,simpleSmoothIterations,simpleSmoothStrength,subdivisions);
					break;
			}
			
			//Send the Vector3 array to a movement script attached to this gameObject
			SendMessage("PathComplete",pathPoints,SendMessageOptions.DontRequireReceiver);
		} else {
			Vector3[] a2 = new Vector3[1] {((endPoint == RealEnd.AddExact || endPoint == RealEnd.Exact) && !path.forceEndSnap ? endpos : startpos)};
			pathPoints = a2;
			SendMessage("PathComplete",a2,SendMessageOptions.DontRequireReceiver);
		}
		
	}
	
	//Returns a node from a clamped I integer (makes sure no Out-Of-Range-Exceptions will get called)
	public static Vector3 GetNode (int i,Vector3[] nodes) {
		i = i > nodes.Length-1 ? nodes.Length-1 : i;
		i = i < 0 ? 0 : i;
		return nodes[i];
	}
	
	
	public void OnDrawGizmos () {
		//Draw the smoothed path (however if no smoothing is enabled, this path will not be smoothed)
		if (debugPath && pathPoints != null) {
			Gizmos.color = new Color (1,0.565F,0);
			
			for (int i=0;i<pathPoints.Length-1;i++) {
				Gizmos.DrawLine (pathPoints[i],pathPoints[i+1]);
			}
		}
		
		//Draw the basic, unsmoothed path
		if (postProcessor != PostProcessor.None && !onlyDebugSmoothedPath) {
			Gizmos.color = new Color (1,0,0,0.7F);
			if (debugPath && path != null && path.path != null) {
				for (int i=0;i<path.path.Length-1;i++) {
					Gizmos.DrawLine (path.path[i].vectorPos,path.path[i+1].vectorPos);
				}
			}
			
		}
	}
	
	//Call this function to start calculating a path
	public AstarPath.Path StartPath (Vector3 start, Vector3 end) {
		
		//Cancel the previous path
		if (path != null) {
			path.error = true;
		}
		
		endpos = end;
		startpos = start;
		
		//Shall we search all grids or only the one specified in the "grid" variable
		if (gridSelection == GridSelection.Auto) {
			path = new AstarPath.Path (this,start,end,maxAngle,angleCost,stepByStep);//Create a new Path instance
		} else {
			path = new AstarPath.Path (this,start,end,maxAngle,angleCost,stepByStep,grid);//Create a new Path instance
		}
		
		isDone = false;
		
		/*if (multithread) {
			AstarPath.StartPathPreThread(path,this);
			StartCoroutine (MultithreadingCompleteCheck ());
		} else {*/
			AstarPath.StartPath(path);//Send a call to the pathfinding system to calculate the path
		//}
		
		return path;
	}
}
