//    � Copyright 2010 Aron Granberg
//    AstarPath.cs script is licenced under a Creative Commons Attribution-Noncommercial 3.0 Unported License.
//    If you want to use the script in commercial projects, please contact me at aron.g@me.com

//For documentation see http://www.arongranberg.com/unity/a-pathfinding/docs/

using UnityEngine;
using System.Collections;
using AstarClasses;
using AstarMath;
using AstarProcess;

using System.Threading;

[AddComponentMenu ("Pathfinding/A* Pathfinding")]
public class AstarPath : MonoBehaviour {
	
	//MAIN
	
	//There nodes is the nodes all paths will use
	public static int totalNodeAmount = 0;
	[System.NonSerialized]
	public Node[][,] staticNodes;
	public Grid[] grids = new Grid[1] {new Grid (20)};
	public Grid textureGrid = new Grid (20);
	public Grid meshGrid = new Grid (20);
	public NodeLink[] links = new NodeLink [0];
	
	public GridGenerator gridGenerator = GridGenerator.Grid;
	
	[System.NonSerialized]
	public BinaryHeap binaryHeap;
	
	public int levelCost = 40;
	
	public bool calculateOnStartup = true;
	
	//The last calculated path
	public static Path prevPath = null;
	
	public static Path[] cache;
	public bool cachePaths = false;
	public int cacheLimit = 3;
	public float cacheTimeLimit = 2.0F;
	

	public bool showGrid = false;
	
	public bool showGridBounds = true;
	
	public bool showLinks = true;
	
	//This script - Singleton structure
	public static new AstarPath active;
	
	//A predefined cost array for grid nodes
	public static int[] costs = new int[9] {14,10,14,10,10,14,10,14,20};
	
	[System.NonSerialized]
	public int area = -1;
	
	[System.NonSerialized]
	public Color[] areaColors;
	
	[System.NonSerialized]
	public static Color[] presetAreaColors = new Color[8] {
		Color.green,
		Color.yellow,
		Color.blue,
		Color.red,
		new Color (1,0.5F,0), //Orange
		Color.cyan,
		Color.white,
		new Color (0.5F,0,1) //Purple
	};
	
	//See docs http://www.arongranberg.com/unity/a-pathfinding/docs/
	//For explanations about what the variables mean
	
	//The max time the script can use per frame
	public float maxFrameTime = 0.01F;
	public int maxPathsPerFrame = 3;
	public int pathExecutionDelay = 1;
	[System.NonSerialized]
	public int pathsThisFrame = 3;
	[System.NonSerialized]
	public int lastPathFrame = -9999;
	
	public Simplify simplify = Simplify.None;
	
	public float staticMaxAngle = 45F;
	public bool useNormal = true;
	
	public float heapSize = 1.0F;
	
	//Path Queue
	
	//Is there any paths calculating currently?
	public static bool activePath = false;
	
	//The path which is currently being calculated, index in the pathQueue array
	private static int currentCalculatingPath = 0;
	private static int lastAddedPath = 0;
	//The queue of paths to be calculated
	private static Path[] pathQueue;
	public static int maxPathQueueSize = 1000;
	
	//Save & Load
	
	public AstarData astarData;
	
	//Debuging
	public DebugMode debugMode = DebugMode.Areas;
	public float debugModeRoof = 300F;
	public bool showParent = false;
	public bool showUnwalkable = true;
	public bool onlyShowLastPath = false;
	public bool showNeighbours = true;
	public Path lastPath = null;
	
	//Non interactive debuging
	[System.NonSerialized]
	public bool anyPhysicsChanged = false;
	[System.NonSerialized]
	public int physicsChangedGrid = -1;
	
	// Height
	
	
	public Height heightMode = Height.Flat;
	public LayerMask groundLayer;
	
	// 
	
	public bool dontCutCorners = false;
	public bool testStraightLine = false;
	public float lineAccuracy = 0.5F;
	
	public Formula formula = Formula.HG;
	public IsNeighbour isNeighbours = IsNeighbour.Eight;
	public float heightRaycast = 100;
	
	public bool useWorldPositions = false;
	
	
	public float boundsMargin = 0;
	
	// ----- Addons -----
	
	public bool quadtreePostProcess = false;
	public int quadtreeMinDepth = 3;
	public bool quadtreeDebug = true;
	
	// ----- Texture Scanning ----
	
	public Texture2D navTex;
	public float threshold = 0.1F;
	public int penaltyMultiplier = 20;
	
	// ------ Bounds Scanning ----
	
	public string boundsTag = "";
	public float neighbourDistanceLimit = Mathf.Infinity;
	public float boundMargin = 1;
	public LayerMask boundsRayHitMask = 1;
	
	//This is used for list scanning too
	public float yLimit = 10;
	
	// -------- Mesh Scanning ------
	
	public Vector3 navmeshRotation = Vector3.zero;
	public Mesh navmesh;
	public CoordinateSystem meshCoordinateSystem = CoordinateSystem.RightHanded;
	//public bool removeDownFacingPolys = true;
	[System.NonSerialized]
	public Matrix rotationMatrix = new Matrix ();
	public MeshNodePosition meshNodePosition = MeshNodePosition.Edge;
	
	//This little function generates a rotation matrix for the Mesh mode, it takes rotation, scale and offset into account
	public void GenerateRotationMatrix (Grid grid) {
		rotationMatrix = 
		(Matrix.RotateX(navmeshRotation.x) * 
		Matrix.RotateY(navmeshRotation.y) * 
		Matrix.RotateZ (navmeshRotation.z) * 
		Matrix.Scale (grid.nodeSize*(meshCoordinateSystem == CoordinateSystem.LeftHanded ? -1.0F : 1.0F)))
		.translate (grid.offset.x,grid.offset.y,grid.offset.z);
	}
	
	// --------- List Scanning -------
	
	public Transform listRootNode;
	
	public static string[] log;
	
	public static void AddLogEntry (string message) {
		if (log == null) {
			log = new string[20];
		}
		
		for (int i=19;i>0;i--) {
			log[i] = log[i-1];
		}
		log[0] = message;
		
		/*lastLogEntry ++;
		if (lastLogEntry >= 20) {
			lastLogEntry = 0;
		}
		log[lastLogEntry] = message;*/
	}
	
	public void OnDrawGizmos () {
		//Draw the grid
		active = this;
		
		for (int y=0;y<grids.Length;y++) {//Height
			Grid grid = grids[y];
			
			if (grid == null) {
				return;
			}
			
			if (showGridBounds) {
				float w = (grid.globalWidth)*grid.nodeSize;
				float d = (grid.globalDepth)*grid.nodeSize;
				Gizmos.color = Color.white;
				
				//This part draws a 3d wire box around the grid
				/*Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y,grid.realOffset.z),new Vector3(grid.realOffset.x+w,grid.realOffset.y,grid.realOffset.z));
				Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y,grid.realOffset.z),new Vector3(grid.realOffset.x,grid.realOffset.y,grid.realOffset.z+d));
				
				Gizmos.DrawLine (new Vector3(grid.realOffset.x+w,grid.realOffset.y,grid.realOffset.z),new Vector3(grid.realOffset.x+w,grid.realOffset.y,grid.realOffset.z+d));
				Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y,grid.realOffset.z+d),new Vector3(grid.realOffset.x+w,grid.realOffset.y,grid.realOffset.z+d));
				
				
				Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y+grid.globalHeight,grid.realOffset.z),new Vector3(grid.realOffset.x+w,grid.realOffset.y+grid.globalHeight,grid.realOffset.z));
				Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y+grid.globalHeight,grid.realOffset.z),new Vector3(grid.realOffset.x,grid.realOffset.y+grid.globalHeight,grid.realOffset.z+d));
				Gizmos.DrawLine (new Vector3(grid.realOffset.x+w,grid.realOffset.y+grid.globalHeight,grid.realOffset.z),new Vector3(grid.realOffset.x+w,grid.realOffset.y+grid.globalHeight,grid.realOffset.z+d));
				Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y+grid.globalHeight,grid.realOffset.z+d),new Vector3(grid.realOffset.x+w,grid.realOffset.y+grid.globalHeight,grid.realOffset.z+d));
				
				Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y,grid.realOffset.z),new Vector3(grid.realOffset.x,grid.realOffset.y+grid.globalHeight,grid.realOffset.z));
				
				Gizmos.DrawLine (new Vector3(grid.realOffset.x+w,grid.realOffset.y,grid.realOffset.z),new Vector3(grid.realOffset.x+w,grid.realOffset.y+grid.globalHeight,grid.realOffset.z));
				
				Gizmos.DrawLine (new Vector3(grid.realOffset.x,grid.realOffset.y,grid.realOffset.z+d),new Vector3(grid.realOffset.x,grid.realOffset.y+grid.globalHeight,grid.realOffset.z+d));
				
				Gizmos.DrawLine (new Vector3(grid.realOffset.x+w,grid.realOffset.y,grid.realOffset.z+d),new Vector3(grid.realOffset.x+w,grid.realOffset.y+grid.globalHeight,grid.realOffset.z+d));*/
				
				Gizmos.DrawWireCube (
				new Vector3(grid.realOffset.x+(w/2.0F),grid.realOffset.y+(grid.globalHeight/2.0F),
				grid.realOffset.z+(d/2.0F)),new Vector3(w,grid.globalHeight,d)
				);
				//End box
			}
			
			
			
			if (staticNodes == null || (staticNodes.Length > 0 && staticNodes[0] == null) || !showGrid || !grid.debug) {
				continue;
			}
			
			bool[,] viewed = quadtreePostProcess ? new bool[staticNodes[y].GetLength(0),staticNodes[y].GetLength(1)] : null;
			
			//Loop through all nodes in this grid
			foreach (Node node in staticNodes[y]) {
				if (node == null || (!showUnwalkable && !node.walkable)) {
					continue;
				}
				
				if (onlyShowLastPath && node.script != lastPath) {
					continue;
				}
				
				if (quadtreePostProcess) {
					if (viewed[node.pos.x,node.pos.z] == true) {
						continue;
					} else {
						viewed[node.pos.x,node.pos.z] = true;
					}
				}
				
				Color c;
				
				if (!node.walkable) {
					c = Color.red;
					c.a = 0.5F;
					Gizmos.color = c;
					
					Gizmos.DrawCube (node.vectorPos,Vector3.one * grid.nodeSize*0.3F);
					continue;
				} else if (quadtreePostProcess && quadtreeDebug) {
					c = Color.Lerp (Color.red,Color.blue,node.depth/debugModeRoof);
					c.a = 0.6F;
					Gizmos.color = c;
					
					Gizmos.DrawCube (node.vectorPos,new Vector3 (node.dia,grid.nodeSize,node.dia));
					//continue;
				}
				
				switch (debugMode) {
					case DebugMode.Areas:
						c = Color.white;
					
						c = (Color)areaColors[node.area];
					
						c.a = 0.5F;
						Gizmos.color = c;
						break;
					case DebugMode.Angles:
					
						float max = 0;
						for (int i=0;i<node.connections.Length;i++) {
							max = node.connections[i].angle > max ? node.connections[i].angle : max;
						}
					
						c = Color.Lerp (Color.green,Color.red,max/90.0F);
						Gizmos.color = c;
						break;
					case DebugMode.H:
						c = Color.Lerp (Color.green,Color.red,node.h/debugModeRoof);
						Gizmos.color = c;
						break;
					case DebugMode.G:
						c = Color.Lerp (Color.green,Color.red,node.g/debugModeRoof);
						Gizmos.color = c;
						break;
					case DebugMode.F:
						c = Color.Lerp (Color.green,Color.red,node.f/debugModeRoof);
						Gizmos.color = c;
						break;
				}
				
				if (node.connections == null) {
					continue;
				}
				
				if (node.parent != null && showParent) {
					Gizmos.DrawLine (node.vectorPos,node.parent.vectorPos);
				} else if (showNeighbours) {
					for (int i=0;i<node.enabledConnections.Length;i++) {
						Gizmos.DrawLine (node.vectorPos,node.enabledConnections[i].endNode.vectorPos);
						//Gizmos.DrawRay (node.vectorPos,(node.neighbours[i].vectorPos-node.vectorPos)*0.5F);
					}
				}
				
				//Commented code for showing a bunch of spheres to indicate how big the Physic Check spheres/capsules are.
				/*if (anyPhysicsChanged && grid == grids[physicsChangedGrid]) {
					Gizmos.color = Color.red;
				
					Vector3 scPos = Camera.current.WorldToScreenPoint (node.vectorPos);
					if (Mathf.Abs (scPos.x-Screen.width/2) < 30 && Mathf.Abs (scPos.y-Screen.height/2) < 30) {
						Gizmos.DrawWireSphere (node.vectorPos,grid.nodeSize*grid.physicsRadius);
					}
				}*/
			}
			
			if (gridGenerator == GridGenerator.Mesh && navmesh != null) {
				Color c = Color.grey;
				c.a = 0.7F;
				Gizmos.color = c;
				Vector3[] verts = navmesh.vertices;
				int[] tris = navmesh.triangles;
				for (int i=0;i<tris.Length/3;i++) {
					
					Vector3 p1 = rotationMatrix.TransformVector (verts[tris[i*3]]);
					Vector3 p2 = rotationMatrix.TransformVector (verts[tris[i*3+1]]);
					Vector3 p3 = rotationMatrix.TransformVector (verts[tris[i*3+2]]);
					
					Gizmos.DrawLine (p1,p2);
					
					Gizmos.DrawLine (p1,p3);
					
					Gizmos.DrawLine (p2,p3);
					
					/*Gizmos.DrawLine (
					RotatePoint (verts[tris[i*3]]*meshGrid.nodeSize)+meshGrid.offset,
					RotatePoint (verts[tris[i*3+1]]*meshGrid.nodeSize)+meshGrid.offset);
					
					Gizmos.DrawLine (
					RotatePoint (verts[tris[i*3]]*meshGrid.nodeSize)+meshGrid.offset,
					RotatePoint (verts[tris[i*3+2]]*meshGrid.nodeSize)+meshGrid.offset);
					
					Gizmos.DrawLine (
					RotatePoint (verts[tris[i*3+1]]*meshGrid.nodeSize)+meshGrid.offset,
					RotatePoint (verts[tris[i*3+2]]*meshGrid.nodeSize)+meshGrid.offset);*/
				}
				
				
			}
			
			
		}
		
		if (staticNodes == null || !showLinks) {
			return;
		}
		
		//Draw all the links
		for (int i=0;i<links.Length && grids.Length > 0;i++) {
			if (staticNodes.Length == 0 || staticNodes[0] == null) {
				continue;
			}
			NodeLink link = links[i];
			Int3 from = ToLocal (link.fromVector);
			Node fromNode = null;
			
			Int3 to = ToLocal (link.toVector);
			//Vector3 toPos = link.toVector;
			Node toNode = null;
			Gizmos.color = Color.green;
			if (from != new Int3 (-1,-1,-1) && !grids[from.y].changed) {
				fromNode = GetNode (from);//.vectorPos;
			} else {
				Gizmos.color = Color.red;
			}
			
			if (to != new Int3 (-1,-1,-1) && !grids[to.y].changed) {
				//Debug.Log (to + " "+grids.Length +" "+grids[to.y].width+grids[to.y].width);
				toNode = GetNode (to);//.vectorPos;
				
			} else {
				Gizmos.color = Color.red;
			}
			
			switch (link.linkType) {
				case LinkType.Link:
					Gizmos.DrawLine (fromNode == null ? link.fromVector : fromNode.vectorPos,toNode == null ? link.toVector : toNode.vectorPos);
					break;
				case LinkType.NodeDisabler:
					if (fromNode != null) {
						Gizmos.color = new Color (1,0,0,0.6F);
						Gizmos.DrawSphere (fromNode.vectorPos,grids[from.y].nodeSize*0.4F);
					} else {
						Gizmos.color = new Color (1,0.5F,0.5F,0.6F);
						Gizmos.DrawSphere (link.fromVector,grids[0].nodeSize * 0.4F);
					}
					break;
				case LinkType.NodeEnabler:
					if (fromNode != null) {
						Gizmos.color = new Color (0,1,0,0.6F);
						Gizmos.DrawSphere (fromNode.vectorPos,grids[from.y].nodeSize*0.4F);
					} else {
						Gizmos.color = new Color (1,0.5F,0.5F,0.6F);
						Gizmos.DrawSphere (link.fromVector,grids[0].nodeSize * 0.4F);
					}
					break;
			}
		}
		
	}
	
	public int GetUnwalkableNodeAmount () {
		int c = 0;
		for (int i=0;i<grids.Length;i++) {
			if (staticNodes[i] == null) {
				continue;
			}
			foreach (Node node in staticNodes[i]) {
				if (node != null && !node.walkable) {
					c++;
				}
			}
		}
		return c;
	}
	
	public int GetWalkableNodeAmount () {
		int c = 0;
		for (int i=0;i<grids.Length;i++) {
			if (staticNodes[i] == null) {
				continue;
			}
			foreach (Node node in staticNodes[i]) {
				if (node != null && node.walkable) {
					c++;
				}
			}
		}
		return c;
	}
	
	
	public static void StartPath (Path p) {
		int nextPath = lastAddedPath+1;
		if (nextPath >= maxPathQueueSize) {
			nextPath = 0;
		}
		
		if (nextPath == currentCalculatingPath) {
			Debug.LogError ("To Many Paths In Queue, please increase queue size or call StartPath less often");
			return;
		}
		
		pathQueue[nextPath] = p;
		
		if (!activePath) {
			lastAddedPath = nextPath;
			active.StartCoroutine (CalculatePaths ());
		} else {
			lastAddedPath = nextPath;
		}	
	}
	
	//This function is run to calculate paths, it will end after a few frames with no paths to calculate
	public static IEnumerator CalculatePaths () {
		activePath = true;
		
		float ptf = 0F;
		int framesWithNoPaths = 0;
		
		while (framesWithNoPaths < 5) {
			
			if (lastAddedPath == currentCalculatingPath) {
				framesWithNoPaths++;
				yield return 0;
			}
			
			while (lastAddedPath != currentCalculatingPath) {
				framesWithNoPaths = 0;
					
				currentCalculatingPath++;
				if (currentCalculatingPath >= maxPathQueueSize) {
					currentCalculatingPath = 0;
				}
				
				
				Path p = pathQueue[currentCalculatingPath]; 
				
				//Just to not cause lag when the user replaces a very large number of paths
				ptf += 0.01F;
				
				if (!p.error) {
					
					ptf += 0.99F;
					
					//For debug uses, we set the last computed path to [p], so we can view debug info on it in the editor (scene view).
					active.lastPath = p;
					
					if (!p.error) {
						p.Init ();
						
					}
					
					//The error can turn up in the Init function
					if (!p.error && !p.foundEnd) {
						
						p.Calc (false);//Comment this line out if you want a bit higher framerate, works a bit like a hard coded value for "only one path per frame" @Performance
						
						while (!p.foundEnd && !p.error) {
							
							for (int i=0;i < (active.pathExecutionDelay < 1 ? 1 : active.pathExecutionDelay);i++) {
								yield return 0;
							}
							
							//Reset the counter for this frame since we have called yield at least once and has now only computed 1 path this frame
							ptf = 1F;
							p.Calc (false);
						}
						
					}
					
					//Add stuff to the log
					AddLogEntry ("Computation Time: "+(p.t*1000).ToString ("0.0") +"ms	  Return Time: "+((Time.realtimeSinceStartup-p.realStartTime)*1000).ToString ("0.0")+"ms");
				}
				
				//Send the computed path to the seeker
				if (p.seeker != null) {
					p.seeker.OnComplete (p);
				}
				
				//Wait for a bit if we have computed a lot of paths
				if (ptf >= active.maxPathsPerFrame) {
					for (int i=0;i<(active.pathExecutionDelay < 1 ? 1 : active.pathExecutionDelay);i++) {
						yield return 0;
					}
					ptf = 0F;
				}
			
			}
		}
		
		activePath = false;
	}
	
	//Scan the map at startup
	public void Awake () {
		active = this;
		
		//In case we are reloading a level, we should make sure that the queue values are being reset
		currentCalculatingPath = 0;
		lastAddedPath = 0;
		activePath = false;
		
		pathQueue = new Path[maxPathQueueSize];
		//Create the Cache array and make sure the length is at least 1
		if (cachePaths) {
			cache = new Path[cacheLimit >= 1 ? cacheLimit : 1];
		} else {
			cache = new Path[1];
		}
		
		if (calculateOnStartup) {
			float startTime = Time.realtimeSinceStartup;
			Scan (true,0);
			float elapsed = Time.realtimeSinceStartup-startTime;
			Debug.Log ("Grid calculated. Generation took "+elapsed+" seconds to complete");
			//Debug.Log ("calc");
		} else {
			if (astarData != null) {
				LoadAstarData ();
			}
		}
	}
	
	
	//From global position to local position, i.e index in the node array
	//ForceNodesUnder means that the nodes must be under the player/npc/whatever if it should be choosen
	public static Int3 ToLocalTest (Vector3 pos,bool forceNodesUnder) {
		Node nearest = null;
		float shortest = Mathf.Infinity;
		
		for (int y=0;y<active.grids.Length;y++) {
			Grid grid = active.grids[y];
			if (grid.Contains (pos)) {
				for (int z=0;z<grid.depth;z++) {
					for (int x=0;x<grid.width;x++) {
						Node node = GetNode (x,y,z);
						
						//The nodes must be under the player/npc/whatever
						if (forceNodesUnder && node.vectorPos.y > pos.y) {
							continue;
						}
						
						float dist = (pos-node.vectorPos).sqrMagnitude;
						if (dist < shortest) {
							nearest = node;
							shortest = dist;
						}
					}
				}
				
			}
		}
		if (nearest == null) {
			return new Int3 (-1,-1,-1);
		}
		return nearest.pos;
		
	}
	
	public static Int3 ToLocalTest2 (Vector3 pos,bool forceNodesUnder) {
		float minYDist = Mathf.Infinity;
		Node minNode = null;
		Vector2 pos2D = new Vector2(pos.x,pos.z);
		
		for (int y=0;y<active.grids.Length;y++) {
			Grid grid = active.grids[y];
			if (grid.Contains (pos)) {
				for (int z=0;z<grid.depth;z++) {
					for (int x=0;x<grid.width;x++) {
						Node node = GetNode (x,y,z);
						float dist = Mathf.Abs (node.vectorPos.y-pos.y);
						if (dist < minYDist && (forceNodesUnder ? node.vectorPos.y <= pos.y + 0.0001F : true)) {
							Vector2[] area = node.GetArea ();
							
							if (area != null && Polygon.ContainsPoint (area,pos2D)) {
								minNode = node;
								minYDist = dist;
							}
						}
					}
				}
			}
		}
		
		if (minNode == null) {
			return new Int3 (-1,-1,-1);
		}
		
		if (!minNode.walkable) {
			float minDist = Mathf.Infinity;
			Node minNode2 = null;
			for (int i=0;i<minNode.connections.Length;i++) {
				Node node = minNode.connections[i];
				float dist = (node.vectorPos-minNode.vectorPos).sqrMagnitude;
				
				if (dist < minDist) {
					minDist = dist;
					minNode2 = node;
				}
			}
			
			if (minNode2 == null) {
				return new Int3 (-1,-1,-1);
			}
			minNode = minNode2;
		}
		return minNode.pos;
	}
	
	public static Int3 ToLocal (Vector3 Vpos,int gridIndex) {
		return ToLocal (Vpos,gridIndex,false);
	}
	
	public static Int3 ToLocal (Vector3 Vpos,int gridIndex,bool forceNodesUnder) {
		if (active.gridGenerator == GridGenerator.Bounds || active.gridGenerator == GridGenerator.List || active.gridGenerator == GridGenerator.Procedural) {
			return ToLocalTest (Vpos,forceNodesUnder);
		}
		
		if (active.gridGenerator == GridGenerator.Mesh) {
			return ToLocalTest2 (Vpos,forceNodesUnder);
		}
		
		Grid grid = active.grids[gridIndex];
		if (grid.Contains (Vpos)) {
			Vpos -= new Vector3(grid.offset.x,0,grid.offset.z);
			Int3 pos = Vpos;
			pos.x = Mathf.RoundToInt (Vpos.x/grid.nodeSize-0.5F);
			pos.z = Mathf.RoundToInt (Vpos.z/grid.nodeSize-0.5F);
			pos.y = gridIndex;
			return pos;
		}
		
		return new Int3 (-1,-1,-1);
	}
	
	public static Int3 ToLocal (Vector3 Vpos) {
		return ToLocal (Vpos,false);
	}
	
	public static Int3 ToLocal (Vector3 Vpos,bool forceNodesUnder) {
		if (active.gridGenerator == GridGenerator.Bounds || active.gridGenerator == GridGenerator.List || active.gridGenerator == GridGenerator.Procedural) {
			return ToLocalTest (Vpos,forceNodesUnder);
		}
		
		if (active.gridGenerator == GridGenerator.Mesh) {
			return ToLocalTest2 (Vpos,forceNodesUnder);
		}
		
		for (int i=0;i<active.grids.Length;i++) {
			Grid grid = active.grids[i];
			if (grid.Contains (Vpos)) {
				Vpos -= new Vector3(grid.offset.x,0,grid.offset.z);
				Int3 pos = Vpos;
				pos.x = Mathf.RoundToInt (Vpos.x/(float)grid.nodeSize-0.5F);
				pos.z = Mathf.RoundToInt (Vpos.z/(float)grid.nodeSize-0.5F);
				pos.y = i;
				return pos;
			}
		}
		return new Int3 (-1,-1,-1);
	}
	
	public static float ToLocalX (float pos,int level) {
		pos -= active.grids[level].offset.x;
		pos = pos/active.grids[level].nodeSize-0.5F;
		return pos;
	}
	
	public static float ToLocalZ (float pos,int level) {
		pos -= active.grids[level].offset.z;
		pos = pos/active.grids[level].nodeSize-0.5F;
		return pos;
	}
	
	public static Int3 ToLocalUnclamped (Vector3 vPos) {
		Int3[] nearest = new Int3[active.grids.Length];
		
		for (int i=0;i<active.grids.Length;i++) {
			Grid grid = active.grids[i];
			Vector3 vPos2 = vPos;
			vPos2 -= new Vector3(grid.offset.x,0,grid.offset.z);
			Int3 pos = new Int3(0,0,0);
			pos.x = Mathf.Clamp (Mathf.RoundToInt (vPos2.x/(float)grid.nodeSize-0.5F) ,0, grid.width-1);
			pos.z = Mathf.Clamp (Mathf.RoundToInt (vPos2.z/(float)grid.nodeSize-0.5F) ,0, grid.depth-1);
			pos.y = i;
			nearest[i] = pos;
		}
		
		Int3 nearestPos = new Int3(0,0,0);
		float minDist = Mathf.Infinity;
		for (int i=0;i<nearest.Length;i++) {
			float dist = (GetNode(nearest[i]).vectorPos-vPos).sqrMagnitude;
			Debug.DrawRay (GetNode(nearest[i]).vectorPos,Vector3.up*2,Color.red);
			if (dist < minDist) {
				minDist = dist;
				nearestPos = nearest[i];
			}
		}
		return nearestPos;
	}
	
	//Short parameter functions. This will skip the 'limit' and use the default value, 100.
	public static Node GetNearest (Node startNode, bool walkable) {
		return GetNearest (startNode,walkable,-1,100);
	}
	
	//This call skips the 'area' variable, which means that any area will do
	public static Node GetNearest (Node startNode, bool walkable, int limit) {
		return GetNearest (startNode,walkable,-1,limit);
	}
	
	//For Grid use only! Returns the nearest node satisfying the 'walkable' and 'area' conditions in the scene (however it stops searching after [limit] nodes)
	public static Node GetNearest (Node startNode, bool walkable, int area, int limit) {
		if (startNode.walkable == walkable && (area == -1 || startNode.area == area)) {
			return startNode;
		}
		
		//ArrayList open = new ArrayList ();
		//ArrayList open2 = new ArrayList ();
		
		int openCounter = 0;
		Node[] open = new Node[limit];
		int open2Counter = 0;
		Node[] open2 = new Node[limit];
		
		Node current = startNode;
		int c = 0;
		int counter = 0;
		while (current.walkable != walkable || (area != -1 && current.area != area)) {
			
			counter++;
			if (counter >= limit) {
				return null;
			}
			
			foreach (Connection conn in current.connections) {
				if (conn.endNode != null && !Arrays.Contains (open2,conn.endNode) && open2Counter < open2.Length) {
					open2[open2Counter] = conn.endNode;
					open2Counter++;
				}
			}
			
			if (c >= openCounter) {
				open = open2;
				openCounter = open2Counter;
				
				open2 = new Node[limit];
				current = open[c];
				open2Counter = 0;
				c = 1;
			} else {
				current = open[c];
				c++;
			}
			//Debug.DrawRay (current.vectorPos,Vector3.up*5,Color.yellow);
		}
		
		//Debug.DrawRay (current.vectorPos,Vector3.up*10,Color.red);
		
		Node minNode = current;
		float minDist = Mathf.Infinity;
		
		bool succeded = false;
		
		for (int i=0;i<openCounter;i++) {
			Node node = open[i] as Node;
			if (node.walkable == walkable && (area == -1 || node.area == area)) {
				float dist = (node.vectorPos-startNode.vectorPos).sqrMagnitude;
				if (dist < minDist) {
					minNode = node;
					minDist = dist;
					succeded = true;
				}
			}
		}
		
		return succeded ? minNode : null;
	}
	 
	public class Path {
		public Seeker seeker;
		
		
		public float pathStartTime = 0;
		//Start/End node
		private Node start;
		public Node end;
		
		//The open list
		private BinaryHeap open;
		
		//The node we are currently processing
		private Node current;
		//Have we found the end?
		public bool foundEnd = false;
		private float maxFrameTime = 0.002F;
		private float maxAngle = 20;
		private float angleCost = 2F;
		private bool stepByStep = true;
		
		//The time this instance was created
		public float realStartTime;
		
		//private float unitRadius = 0.5F;//BETA, this variable don't do anything
		public Node[] path;
		public bool error = false;//Has there occured an error while calculating?
		
		//Debug--
		public float t = 0;//The time the script has calculated
		private int frames = 1;//The number of frames the calculation has taken so far
		private int closedNodes = 0;//The number of searched nodes
		
		public bool forceStartSnap = false;
		public bool forceEndSnap = false;
		
		public Path (Seeker s,Vector3 newstart,Vector3 newend,float NmaxAngle,float NangleCost,bool NstepByStep) {
			if (active == null || active.staticNodes == null) {
				Debug.LogError ("The navmesh is not calculated yet - Check the 'Scan Map On Startup' toggle to make sure the map is scanned at start - Don't run any pathfinding calls in Awake");
				error = true;
				return;
			}
			
			seeker = s;
			float startTime = Time.realtimeSinceStartup;
			pathStartTime = startTime;
			maxFrameTime = AstarPath.active.maxFrameTime;
			maxAngle = NmaxAngle;
			angleCost = NangleCost;
			stepByStep = NstepByStep;
			//unitRadius = 0;//BETA, Not used
			Int3 startPos = ToLocal (newstart,true);
			Int3 endPos = ToLocal (newend,false);
			PostNew (startPos,endPos);
		}
		
		public Path (Seeker s,Vector3 newstart,Vector3 newend,float NmaxAngle,float NangleCost,bool NstepByStep,int grid) {
			if (active == null || active.staticNodes == null) {
				Debug.LogError ("The navmesh is not calculated yet - Check the 'Scan Map On Startup' toggle to make sure the map is scanned at start - Don't run any pathfinding calls in Awake");
				error = true;
				return;
			}
			
			seeker = s;
			float startTime = Time.realtimeSinceStartup;
			pathStartTime = startTime;
			maxFrameTime = AstarPath.active.maxFrameTime;
			maxAngle = NmaxAngle;
			angleCost = NangleCost;
			stepByStep = NstepByStep;
			//unitRadius = 0;//BETA, Not used
			Int3 startPos = ToLocal (newstart,grid,true);
			Int3 endPos = ToLocal (newend,grid,false);
			t += Time.realtimeSinceStartup-startTime;
			PostNew (startPos,endPos);
		}
		
		public void PostNew (Int3 startPos,Int3 endPos) {
			float startTime = Time.realtimeSinceStartup;
			realStartTime = startTime;
			
			if (startPos == new Int3 (-1,-1,-1)) {
				Debug.LogWarning ("Start Point : No nearby Nodes Were found (Position not inside any grids or no nodes were close enough");
				error = true;
				return;
			}
			
			if (endPos == new Int3 (-1,-1,-1)) {
				Debug.LogWarning ("Target Point : No nearby Nodes Were found (Position not inside any grids or no nodes were close enough");
				error = true;
				return;
			}
			
			
			start = GetNode (startPos);
			end = GetNode (endPos);
			if (!start.walkable) {
				start = GetNearest (start,true,120);
				
				if (start == null) {
					Debug.LogWarning ("Cannot find any valid start nodes (the start is on unwalkable ground)");
				}
				
				forceStartSnap = true;
				
				/*if (start.enabledConnections.Length > 0) {
					
					start = start.enabledConnections[0];
					Debug.LogWarning ("Start point is not walkable, setting a node close to start as start");
				} else {
					Debug.LogWarning ("Starting from non walkable node");
					error= true;
					return;
				}*/
			}
			current = start;
			
			if (!end.walkable || end.area != start.area) {
				end = GetNearest (end,true,start.area,400);
				
				if (end == null) {
					Debug.LogWarning ("Cannot find any valid target nodes");
					error = true;
					return;
				}
				
				if (end.area != start.area) {
					Debug.LogError ("WUT!!?!?");
				}
				
				forceEndSnap = true;
				/*if (end.enabledConnections.Length > 0) {
					end = end.enabledConnections[0];
					Debug.LogWarning ("Target point is not walkable, setting a node close to target as target");
				} else {
					Debug.LogWarning ("Target point is not walkable");
					error= true;
					return;
				}*/
			}
			
			if (end.area != start.area) {
				Debug.LogWarning ("We can't walk from start to end, differend areas");
				error= true;
				return;
			}
			
			t += Time.realtimeSinceStartup-startTime;
		}
		
		public void Init () {
			//Make a new binary heap (like an array but faster)
			//open = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount));
			open = active.binaryHeap;
			open.numberOfItems = 1;
			
			
			float startTime = Time.realtimeSinceStartup;
			start.script = this;
			start.parent = null;
			start.UpdateH (end);
			
			if (active.cachePaths) {
				for (int i= 0;i<cache.Length;i++) {
					Path p = cache[i];
					if (p != null && p.path != null) {
						if ( Time.realtimeSinceStartup - p.pathStartTime < active.cacheTimeLimit) {
							if (p.start == start && p.end == end) {
								path = p.path;
								foundEnd = true;
								Debug.Log ("A* Pathfinding Completed Succesfully, a cached path was used"); 
								return;
							}
						} else {
							cache[i] = null;
						}
					}
				}
			}
			
			if (active.testStraightLine && CheckLine (start,end,maxAngle)) {
				
				foundEnd = true;
				path = new Node[2] {start,end};
				Debug.Log ("A* Pathfinding Completed Succesfully, a straight path was used");
				return;
			}
			t += Time.realtimeSinceStartup-startTime;
		}
		
		//Calculate the path until we find the end or an error occured or we have searched all nodes/the maxFrameTime was exceded
		public void Calc (bool multithreaded) {
			float startTime = Time.realtimeSinceStartup;
			
			start.script = this;
			start.parent = null;
			
			int counter = 0;
			//Continue to search while there hasn't ocurred an error and the end hasn't been found
			while (!foundEnd && !error) {
				counter++;
				//Debug.Log ("C0 "+counter);
				//Close the current node, if the current node is the target node then the path is finnished
				if (current==end) {
					foundEnd = true;
					break;
				}
				
				//Debug.Log ("C1 "+counter);
				
				if (current == null) {
					Debug.LogWarning ("Current is Null");
					return;
				}
				
				//@Performance Just for debug info
				closedNodes++;
				//Loop through all walkable neighbours of the node
				
				//Debug.Log ("C2 "+counter);
				//Debug.Log (current.neighbours.Length);
				
				current.Open (open, this, start, end, angleCost);
				/*for (int i=0;i<current.enabledConnections.Length;i++) {
					//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.red); //Uncomment for debug
					//We shouldn't test the start node
					
					if (current.enabledConnections[i].endNode != start) {
						Open (i);
					}
				}*/
				
				//Debug.Log ("C3 "+counter);
				
				//No nodes left to search?
				if (open.numberOfItems <= 1) {
					Debug.LogWarning ("No open points, whole area searched");
					error = true;
					return;
				}
				
				//Debug.Log ("C4 "+counter);
				
				//Select the node with the smallest F score and remove it from the open array
				current = open.Remove ();
				
				//Debug.Log ("C5 "+counter);
				
				//Have we exceded the maxFrameTime, if so we should wait one frame before continuing the search since we don't want the game to lag
				if (!multithreaded && (stepByStep || Time.realtimeSinceStartup-startTime>=maxFrameTime)) {//@Performance remove that step By Step thing in the IF, if you don't use in the seeker component
				
					t += Time.realtimeSinceStartup-startTime;
					frames++;
					
					//A class can't hold a coroutine, so a separate function need to handle the yield (StartPathYield)
					return;
				}
			
			}
			
			if (!error) {
				
				if (end == start) {
					path = new Node[2] {end,start};
				
				
					t += Time.realtimeSinceStartup-startTime;
					
					//@Performance Debug calls cost performance
					Debug.Log ("A* Pathfinding Completed Succesfully : End code 1\nTime: "+t+" Seconds\nFrames "+frames+"\nAverage Seconds/Frame "+(t/frames)+"\nPoints:"+path.Length+"\nSearched Nodes"+closedNodes+"\nPath Length (G score) Was "+end.g);
					
				} else if (AstarPath.active.simplify == Simplify.Simple) {
					if (active.gridGenerator != GridGenerator.Grid && active.gridGenerator != GridGenerator.Texture) {
						Debug.LogError ("Simplification can not be used with grid generators other than 'Texture' and 'Grid', excpect weird results");
					}
					Debug.LogWarning ("The Simple Simplification is broken");
					/*Node c = end;
					int p = 0;
					//Follow the parents of all nodes to the start point, but only add nodes if there is a change in direction
					int preDir = c.invParentDirection;
					ArrayList a = new ArrayList ();
					a.Add (c);
					while (c.parent != null) {
						
						if (c.parent.invParentDirection != preDir) {
							a.Add (c.parent);
							preDir = c.parent.invParentDirection;
						}
						
						c = c.parent;
						p++;
						
						if (c == start) {
							
							break;
						}
						
						if (p > 300) {
							Debug.LogError ("Preventing possible infinity loop");
							break;
						}
					}
					
					//Then reverse it so the start node gets the first place in the array
					a.Reverse ();
					
					path = a.ToArray (typeof (Node)) as Node[];
				
					t += Time.realtimeSinceStartup-startTime;
					
					//@Performance Debug calls cost performance
					Debug.Log ("A* Pathfinding Completed Succesfully : End code 2\nTime: "+t+" Seconds\nFrames "+frames+"\nAverage Seconds/Frame "+(t/frames)+"\nPoints:"+path.Length+" (simplified)"+"\nSearched Nodes"+closedNodes+"\nPath Length (G score) Was "+end.g);*/
					
				} else if (AstarPath.active.simplify == Simplify.Full) {
					if (active.gridGenerator != GridGenerator.Grid && active.gridGenerator != GridGenerator.Texture) {
						Debug.LogError ("Simplification can not be used with grid generators other than 'Texture' and 'Grid' excpect weird results");
					}
					
					Node c = end;
					ArrayList a = new ArrayList ();
					a.Add (c);
					int p = 0;
					
					//Follow the parents of all nodes to the start point
					while (c.parent != null) {
						a.Add (c.parent);
						
						c = c.parent;
						p++;
						if (c == start) {
							
							break;
						}
						
						//@Performance this IF is almost completely unnecessary
						//if (p > 300) {
						//	Debug.LogError ("Preventing possible infinity loop, remove this code if you have very long paths");
						//	break;
						//}
					}
					
					for (int i=2;i<a.Count;i++) {
						if (i >= a.Count) {
							break;
						}
						
						if (CheckLine ((Node)a[i],(Node)a[i-2],maxAngle)) {
							a.RemoveAt (i-1);
							i=2;
						}
					}
					//Then reverse it so the start node gets the first place in the array
					a.Reverse ();
					
					path = a.ToArray (typeof (Node)) as Node[];
				
				
					t += Time.realtimeSinceStartup-startTime;
					
					//@Performance Debug calls cost performance
					Debug.Log ("A* Pathfinding Completed Succesfully : End code 3\nTime: "+t+" Seconds\nFrames "+frames+"\nAverage Seconds/Frame "+(t/frames)+"\nPoints:"+path.Length+" (simplified)"+"\nSearched Nodes"+closedNodes+"\nPath Length (G score) Was "+end.g);
					
					//We have now found the end and filled the "path" array
					//The next update the Seeker script will find that this is done and send a message with the data
				}
				 else {
					Node c = end;
					ArrayList a = new ArrayList ();
					a.Add (c);
					int p = 0;
					
					//Follow the parents of all nodes to the start point
					while (c.parent != null) {
						a.Add (c.parent);
						
						c = c.parent;
						p++;
						if (c == start) {
							
							break;
						}
						
						//@Performance this IF is almost completely unnecessary
						if (p > 700) {
							Debug.LogError ("Preventing possible infinity loop, remove this code if you have very long paths (i.e more than 300 nodes)");
							break;
						}
					}
					//Then reverse it so the start node gets the first place in the array
					a.Reverse ();
					
					path = a.ToArray (typeof (Node)) as Node[];
				
				
					t += Time.realtimeSinceStartup-startTime;
					
					//@Performance Debug calls cost performance
					Debug.Log ("A* Pathfinding Completed Succesfully : End code 4\nTime: "+t+" Seconds\nFrames "+frames+"\nAverage Seconds/Frame "+(t/frames)+"\nPoints:"+path.Length+"\nSearched Nodes"+closedNodes+"\nPath Length (G score) Was "+end.g);
					
					//We have now found the end and filled the "path" array
					//The next frame the Seeker script will find that this is done and send a message with the path data
					
				}
			}
			
			//@Performance Remove the next lines if you don't use caching
			//These lines pushes the latest path on the cache array (the latest is always first) while keaping a constant length of the array
			for (int i=cache.Length-1;i > 0;i--) {
				cache[i] = cache[i-1];
			}
			cache[0] = this;
			
			//t += Time.realtimeSinceStartup-startTime;
			
			//(FindObjectOfType (typeof(Clicker)) as Clicker).NavigationMesh (path);
			//path = AstarProcess.PostProcess.NavigationMesh (path,active.meshNodePosition == MeshNodePosition.Edge);
		
		}
		
		public void Open (int i) {
			Connection connection = current.enabledConnections[i];
			Node node = connection.endNode;
			//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.red); //Uncomment for debug
			//If the nodes script variable isn't refering to this path class the node counts as "not used yet" and can then be used
			if (node.script != this) {
				//Test if the angle from the current node to this one has exceded the angle limit
				//if (angle >= maxAngle) {
				//	return;
				//}
				node.script = this;
				node.parent = current;
				
				node.basicCost = connection.cost;//(current.costs == null || costs.Length == 0 ? costs[node.invParentDirection] : current.costs[node.invParentDirection]);
				//Calculate the extra cost of moving in a slope
				//@NEED TO BE USEDnode.extraCost =  Mathf.RoundToInt (node.basicCost*connection.angle*angleCost);
				//Add the node to the open array
				//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.green); //Uncomment for @Debug
				
				node.UpdateH (end);
				node.UpdateG ();
				
				open.Add (node);
				
			} else {
				
				//If not we can test if the path from the current node to this one is a better one then the one already used
				int cost = connection.cost;//(current.costs == null || current.costs.Length == 0 ? costs[current.neighboursKeys[i]] : current.costs[current.neighboursKeys[i]]);
				
				int extraCost = Mathf.RoundToInt (node.basicCost*connection.angle*angleCost);
				
				if (current.g+cost+extraCost+node.penalty < node.g) {
					node.basicCost = cost;
					//node.extraCost = extraCost;
					node.parent = current;
					
					node.UpdateAllG ();
					
					open.Add (node);//@Quality, uncomment for better quality (I think).
					
					//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.cyan); //Uncomment for @Debug
				}
				
				 else if (node.g+cost+extraCost+current.penalty < current.g) {//Or if the path from this node ("node") to the current ("current") is better
					bool contains = false;
					
					//Make sure we don't travel along the wrong direction of a one way link now, make sure the Current node can be accesed from the Node.
					for (int y=0;y<node.connections.Length;y++) {
						if (node.connections[y].endNode == current) {
							contains = true;
							break;
						}
					}
					
					if (!contains) {
						return;
					}
					
					current.parent = node;
					current.basicCost = cost;
					//current.extraCost = extraCost;
					
					node.UpdateAllG ();
					
					//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.blue); //Uncomment for @Debug
					open.Add (current);
				}
			}
		}
	}
	
	public static Node GetNode (Int3 pos) {
		return AstarPath.active.staticNodes[pos.y][pos.x,pos.z];//Height, Width, Depth
	}
	
	public static Node GetNode (int x,int y,int z) {
		return AstarPath.active.staticNodes[y][x,z];
	}
	
	public IEnumerator SetNodes (bool walkable, Bounds bounds, bool fullPhysicsCheck, float t) {
		yield return new WaitForSeconds (t);
		SetNodes (walkable,bounds,fullPhysicsCheck,false);
	}
	
	public IEnumerator SetNodes (bool walkable, Bounds bounds, bool fullPhysicsCheck, bool allGrids, float t) {
		yield return new WaitForSeconds (t);
		SetNodes (walkable,bounds,fullPhysicsCheck,allGrids);
	}
	
	public void SetNodes (bool walkable, Bounds bounds, bool fullPhysicsCheck) {
		SetNodes (walkable,bounds,fullPhysicsCheck,false);
	}
	
	//public void SetNodes (bool walkable, Bounds bounds, bool fullPhysicsCheck, bool allGrids) {
	//	SetNodes (walkable,bounds,fullPhysicsCheck,allGrids);
	//}
	
	public void SetNodes (bool walkable, Vector3 point, int gridIndex,bool allGrids) {
		if (gridGenerator != GridGenerator.Grid && gridGenerator != GridGenerator.Texture) {
			Debug.LogError ("The SetNodes function can not be used with grid generators other than 'Texture' and 'Grid'");
			return;
		}
		
		
		if (allGrids) {
			for (int i=0;i<grids.Length;i++) {
				Grid grid = grids[i];
				Int3 p = ToLocal (point,i);
				if (p != new Int3 (-1,-1,-1)) {
					Node node = GetNode(p);
					node.walkable = walkable;
					RecalcNeighbours (node);
					for (int z=Mathf.Clamp(p.z-1,0,grid.depth);z<=p.z+1;z++) {
						for (int x=Mathf.Clamp(p.x-1,0,grid.width);x<=p.x+1;x++) {
							if (x>=grid.width) {
								continue;
							}
							if (z>=grid.depth) {
								continue;
							}
							Node nodeNeighbour = GetNode (new Int3(x,i,z));
							RecalcNeighbours (nodeNeighbour);
							
							
						}
					}
				}
			}
		} else {
			Int3 p = ToLocal (point,gridIndex);
			if (p != new Int3 (-1,-1,-1)) {
				Grid grid = grids[p.y];
				Node node = GetNode(p);
				node.walkable = walkable;
				RecalcNeighbours (node);
				
				for (int z=Mathf.Clamp(p.z-1,0,grid.depth);z<=p.z+1;z++) {
					for (int x=Mathf.Clamp(p.x-1,0,grid.width);x<=p.x+1;x++) {
						if (x>=grid.width) {
							continue;
						}
						if (z>=grid.depth) {
							continue;
						}
						Node nodeNeighbour = GetNode (new Int3(x,gridIndex,z));
						RecalcNeighbours (nodeNeighbour);
						
							
					}
				}
						
			}
			
			
		}
		
	}
	
	public void SetNodes (bool walkable, Bounds bounds, bool fullPhysicsCheck, bool allGrids) {
		
		if (gridGenerator != GridGenerator.Grid && gridGenerator != GridGenerator.Texture) {
			Debug.LogError ("The SetNodes function can not be used with grid generators other than 'Texture' and 'Grid'");
			return;
		}
		
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Rect rect = Rect.MinMaxRect (min.x,min.z,max.x,max.z);
		Debug.Log ("Changing the Grid...");
		
		//Check if the bounds center is inside any grids, and if so, call SetNodesWorld
		if (allGrids) {
			bool any = false;
			
			for (int i=0;i<grids.Length;i++) {
				
				Int3 p = ToLocal (bounds.center,i);
				
				if (p != new Int3 (-1,-1,-1)) {
					SetNodesWorld (walkable,rect,p.y, fullPhysicsCheck);
					any = true;
				}
				
			}
			
			if (!any) {
				Debug.LogWarning ("Can't set nodes, area center is not inside any grid");
			}
			
		} else {
			Int3 p = ToLocal (bounds.center);
			
			if (p != new Int3 (-1,-1,-1)) {
				SetNodesWorld (walkable,rect,p.y, fullPhysicsCheck);
			} else {
				Debug.LogWarning ("Can't set nodes, area center is not inside any grid");
			}
		}
	}
	
	//Rect is defined in world coordinates (x,z)
	public void SetNodesWorld (bool walkable, Rect rect,int gridIndex, bool fullPhysicsCheck) {
		
		if (gridGenerator != GridGenerator.Grid && gridGenerator != GridGenerator.Texture) {
			Debug.LogError ("The SetNodes function can not be used with grid generators other than 'Texture' and 'Grid'");
			return;
		}
		
		/*rect.x = Mathf.Floor (ToLocalX (rect.x,level));
		rect.y = Mathf.Floor (ToLocalZ (rect.y,level));
		rect.width = Mathf.Ceil (rect.width/grids[level].nodeSize);
		rect.height = Mathf.Ceil (rect.height/grids[level].nodeSize);*/
						
						
		Grid grid = grids[gridIndex];
		
		rect.x -= grid.offset.x;
		rect.y -= grid.offset.z;
		//Rect defined in local Coordinates
		int[] localRect = new int[4] {
			Mathf.FloorToInt ((rect.xMin/grid.nodeSize)-0.5F),
			Mathf.FloorToInt ((rect.yMin/grid.nodeSize)-0.5F),
			Mathf.CeilToInt ((rect.xMax/grid.nodeSize)-0.5F),
			Mathf.CeilToInt ((rect.yMax/grid.nodeSize)-0.5F)
		};
		
		//Debug.LogError (localRect[0]+" "+localRect[1]+" "+localRect[2]+" "+localRect[3]);
		
		if (fullPhysicsCheck) {
			RecalculateArea (localRect,gridIndex);
		} else {
			SetNodesLocal (walkable,localRect,gridIndex);
		}
	}
	
	
	
	//Rect is defined in local coordinates, i.e array index
	public void SetNodesLocal (bool walkable, int[] localRect,int gridIndex) {
		Grid grid = grids[gridIndex];
		/*rect = new Rect (
		Mathf.Clamp(rect.x+1,0,grid.width-1),
		Mathf.Clamp(rect.y+1,0,grid.depth-1),
		Mathf.Clamp(rect.width,0,grid.width-1),
		Mathf.Clamp(rect.height,0,grid.depth-1));*/
		
		/*int rx = (int)rect.x;
		int ry = (int)rect.y;
		int xMax = (int)rect.xMax;
		int yMax = (int)rect.yMax;*/
		
		/*
		0 = left x (xMin)
		1 = top z (zMin)
		2 = right x (xMax)
		3 = bottom z (zMax)
		*/
		
		int xMin = localRect[0] < 0 ? 0 : (localRect[0] >= grid.width ? grid.width-1 : localRect[0]);
		int xMax = localRect[2] < 0 ? 0 : (localRect[2] >= grid.width ? grid.width-1 : localRect[2]);
		int zMin = localRect[1] < 0 ? 0 : (localRect[1] >= grid.depth ? grid.depth-1 : localRect[1]);
		int zMax = localRect[3] < 0 ? 0 : (localRect[3] >= grid.depth ? grid.depth-1 : localRect[3]);
		
		
		int xMin2 = xMin-1 < 0 ? 0 : (xMin-1 >= grid.width ? grid.width-1 : xMin-1);
		int xMax2 = xMax+1 < 0 ? 0 : (xMax+1 >= grid.width ? grid.width-1 : xMax+1);
		int zMin2 = zMin-1 < 0 ? 0 : (zMin-1 >= grid.depth ? grid.depth-1 : zMin-1);
		int zMax2 = zMax+1 < 0 ? 0 : (zMax+1 >= grid.depth ? grid.depth-1 : zMax+1);
		
		for (int z=zMin;z<=zMax;z++) {
			for (int x=xMin;x<=xMax;x++) {
				GetNode(x,gridIndex,z).walkable = walkable;
			}
		}
		
		bool anyUnWalkable = false;
		
		//Recalculate the nodes around the changed are too, they need to be updated too
		//if (walkable) {
			for (int z=zMin2;z<=zMax2;z++) {
				for (int x=xMin2;x<=xMax2;x++) {
					Node node = GetNode(x,gridIndex,z);
					if (node.walkable) {
						RecalcNeighbours (node);
					} else {
						anyUnWalkable = true;
					}
				}
			}
		/*} else {
			for (int z=zMin;z<=zMax;z++) {
				for (int x=xMin;x<=xMax;x++) {
					Node node = GetNode(x,gridIndex,z);
					if (node.walkable) {
						RecalcNeighbours (node);
					} else {
						anyUnWalkable = true;
					}
				}
			}
		}*/
		
		//This is a random number specifying when a node was flood filled
		int areaTimeStamp = Mathf.RoundToInt (Random.Range (0,10000));
		
		//If the whole area outside the area is walkable, don't recalculate flood fill, do it only if we are going to change them to walkable
		if (anyUnWalkable && !walkable) {
			
			for (int z=zMin2;z<=zMax2;z++) {
				for (int x=xMin2;x<=xMax2;x++) {
					Node node = GetNode(x,gridIndex,z);
					if (node.walkable) {
						FloodFill (node,areaTimeStamp);
					}
				}
			}
		} else {
			FloodFill (GetNode(xMin,gridIndex,zMin),areaTimeStamp);
		}
	}
	
	public void RecalculateArea (int[] localRect,int gridIndex) {
		Grid grid = grids[gridIndex];
		
		/*rect = new Rect (
		Mathf.Clamp(rect.x+1,0,grid.width-1),
		Mathf.Clamp(rect.y+1,0,grid.depth-1),
		Mathf.Clamp(rect.width,0,grid.width-1),
		Mathf.Clamp(rect.height,0,grid.depth-1));
		
		int rx = (int)rect.x;
		int ry = (int)rect.y;
		int xMax = (int)rect.xMax;
		int yMax = (int)rect.yMax;*/
		
		//We need to recompute the nodes around the area within the physics radius
		int physRad = Mathf.CeilToInt (grid.physicsRadius);
		
		int xMin = localRect[0]-physRad;
		int xMax = localRect[2]+physRad;
		int zMin = localRect[1]-physRad;
		int zMax = localRect[3]+physRad;
		
		//Clamp the values to the grid bounds
		xMin = xMin < 0 ? 0 : (xMin >= grid.width ? grid.width-1 : xMin);
		xMax = xMax < 0 ? 0 : (xMax >= grid.width ? grid.width-1 : xMax);
		zMin = zMin < 0 ? 0 : (zMin >= grid.depth ? grid.depth-1 : zMin);
		zMax = zMax < 0 ? 0 : (zMax >= grid.depth ? grid.depth-1 : zMax);
		
		for (int z=zMin;z<=zMax;z++) {
			for (int x=xMin;x<=xMax;x++) {
				Node node = GetNode(x,gridIndex,z);
				FullPhysicsCheck (node,grids[gridIndex]);
				
				if (gridGenerator == GridGenerator.Texture) {
					node.walkable = node.walkable && TextureEvaluateNode (x,z);
				}
			}
		}
		
		for (int z=zMin;z<=zMax;z++) {
			for (int x=xMin;x<=xMax;x++) {
				Node node = GetNode(x,gridIndex,z);
				//if (node.walkable) {
					RecalcNeighbours (node);
				//}
			}
		}
		//This is a random number specifying when a node was flood filled
		int areaTimeStamp = Mathf.RoundToInt (Random.Range (0,10000));
		
		//If the whole area outside the area is walkable, don't recalculate flood fill, do it only if we are going to change them to walkable
		for (int z=zMin;z<=zMax;z++) {
			for (int x=xMin;x<=xMax;x++) {
				Node node = GetNode(x,gridIndex,z);
				if (node.walkable) {
					FloodFill (node,areaTimeStamp);
				}
			}
		}
	}
	
	//This is to be used only with Grid-like navmeshes!
	public void RecalcNeighbours (Node node) {
		
		int x = node.pos.x;
		int y = node.pos.y;
		int z = node.pos.z;
		
		Grid grid = grids[y];
		
		int topLeftCut = 0;
		int topRightCut = 0;
		int bottomLeftCut = 0;
		int bottomRightCut = 0;
		
		
		//Add the node left of this node to it's neighbourlist if it is walkable
		if (x != 0 && (isNeighbours==IsNeighbour.Eight || isNeighbours==IsNeighbour.Four)) {
			node.connections[3].endNode = GetNode(x-1,y,z);
			node.connections[3].enabled = GetNode(x-1,y,z).walkable && node.connections[3].angle < staticMaxAngle;
			if (node.connections[3].enabled) {
				topLeftCut++;
				bottomLeftCut++;
			}
		}
		
		if (x != grid.width-1 && (isNeighbours==IsNeighbour.Eight || isNeighbours==IsNeighbour.Four)) {
			node.connections[4].endNode = GetNode(x+1,y,z);
			node.connections[4].enabled = GetNode(x+1,y,z).walkable && node.connections[4].angle < staticMaxAngle;
			if (node.connections[4].enabled) {
				topRightCut ++;
				bottomRightCut ++;
			}
		}
		
		if (z != 0) {
			if ((isNeighbours==IsNeighbour.Eight || isNeighbours==IsNeighbour.Four)) {
				node.connections[6].endNode = GetNode(x,y,z-1);
				node.connections[6].enabled = GetNode(x,y,z-1).walkable && node.connections[6].angle < staticMaxAngle;
				if (node.connections[6].enabled) {
					bottomLeftCut ++;
					bottomRightCut ++;
				}
			}
			
			if (x != 0 && isNeighbours==IsNeighbour.Eight) {
				node.connections[5].endNode = GetNode(x-1,y,z-1);
				node.connections[5].enabled = GetNode(x-1,y,z-1).walkable && node.connections[5].angle < staticMaxAngle && (bottomLeftCut==2 || !dontCutCorners);
			}
			
			if (x != grid.width-1 && isNeighbours==IsNeighbour.Eight) {
				node.connections[7].endNode = GetNode(x+1,y,z-1);
				node.connections[7].enabled = GetNode(x+1,y,z-1).walkable && node.connections[7].angle < staticMaxAngle && (bottomRightCut==2 || !dontCutCorners);
			}
		}
		
		if (z != grid.depth-1) {
			
			if ((isNeighbours==IsNeighbour.Eight || isNeighbours==IsNeighbour.Four)) {
				node.connections[1].endNode = GetNode(x,y,z+1);
				node.connections[1].enabled = GetNode(x,y,z+1).walkable && node.connections[1].angle < staticMaxAngle;
				if (node.connections[1].enabled) {
					topLeftCut++;
					topRightCut++;
				}
			}
			
			if (x != 0 && isNeighbours==IsNeighbour.Eight) {
				node.connections[0].endNode = GetNode(x-1,y,z+1);
				node.connections[0].enabled = GetNode(x-1,y,z+1).walkable && node.connections[0].angle < staticMaxAngle && (topLeftCut==2 || !dontCutCorners);
			}
			
			if (x != grid.width-1 && isNeighbours==IsNeighbour.Eight) {
				node.connections[2].endNode = GetNode(x+1,y,z+1);
				node.connections[2].enabled = GetNode(x+1,y,z+1).walkable && node.connections[2].angle < staticMaxAngle && (topRightCut==2 || !dontCutCorners);
			}
		}
		
		node.GenerateEnabledConnections ();
	}
	
	public void FullPhysicsCheck (Node node, Grid grid) {
		FullPhysicsCheck (node,grid,0);
	}
	
	public void FullPhysicsCheck (Node node, Grid grid, LayerMask extraMask) {
		FullPhysicsCheck (node,grid,extraMask,true);
	}
	
	public void FullPhysicsCheck (Node node, Grid grid, LayerMask extraMask, bool doYTest) {
		
		bool noPhysTest = false;
		if (doYTest) {
			switch (heightMode) {
				case Height.Flat:
					node.vectorPos.y = grid.offset.y;
					break;
				case Height.Terrain:
					if(Terrain.activeTerrain) {
						node.vectorPos.y = Terrain.activeTerrain.SampleHeight(node.vectorPos);
					}
					break;
				case Height.Raycast:
					RaycastHit hit;
					node.vectorPos.y = grid.offset.y;
					if (Physics.Raycast (node.vectorPos+new Vector3(0,grid.height,0),-Vector3.up,out hit,grid.height+0.001F,groundLayer)) {
						node.vectorPos.y = hit.point.y;
						
						if (useNormal) {
							Vector3 normal = hit.normal;
							if (Vector3.Angle (Vector3.up,normal) > staticMaxAngle) {
								node.walkable = false;
								noPhysTest = true;
							}
						}
					} else {
						node.walkable = false;
						noPhysTest = true;
					}
					break;
			}
		}
		
		if (noPhysTest) {
			return;
		}
		//Test if the node is walkable
		switch (grid.physicsType) {
			case PhysicsType.OverlapSphere:
			
				Collider[] collisions = Physics.OverlapSphere(node.vectorPos, 0.5F*grid.nodeSize*grid.physicsRadius);
				if (collisions.Length > 0) {
					for (int i=0;i<collisions.Length;i++) {
						if (collisions[i].gameObject.layer != grid.ignoreLayer) {
							node.walkable = false;
						}
					}
				} else {
					node.walkable = true;
				}
				break;
			case PhysicsType.TouchSphere:
				if (Physics.CheckSphere(node.vectorPos, 0.5F*grid.nodeSize*grid.physicsRadius,grid.physicsMask | extraMask)) {
					node.walkable = false;
				} else {
					node.walkable = true;
				}
				break;
			case PhysicsType.TouchCapsule:
				if (Physics.CheckCapsule(node.vectorPos,node.vectorPos+Vector3.up*grid.capsuleHeight, 0.5F*grid.nodeSize*grid.physicsRadius,grid.physicsMask)) {
					node.walkable = false;
				} else {
					node.walkable = true;
				}
				break;
			case PhysicsType.Raycast:
				Ray ray = new Ray ();
				float l = 0;
				if (grid.raycastUpDown==UpDown.Up) {
					ray = new Ray (node.vectorPos,Vector3.up);
					l = grid.raycastLength;
				} else {
					ray = new Ray (node.vectorPos + Vector3.up*grid.raycastLength,Vector3.down);
					l = grid.raycastLength-0.001F;
				}
				//RaycastHit hit;
				if (Physics.Raycast (ray,l,grid.physicsMask)) {
					node.walkable = false;
				} else {
					node.walkable = true;
				}
				break;
		}
	}
	
	public static bool CheckLine (Vector3 from,Vector3 to, float maxAngle) {
		Int3 startPos = ToLocal (from);
		
		if (startPos == new Int3(-1,-1,-1)) {
			Debug.LogError ("'From' is outside the grid bounds");
			return false;
		}
		
		Int3 endPos = ToLocal (to);
		
		if (endPos == new Int3(-1,-1,-1)) {
			Debug.LogError ("'To' is outside the grid bounds");
			return false;
		}
		
		Node start = GetNode (startPos);
		Node end = GetNode (endPos);
		
		if (!start.walkable) {
			return false;
		}
		
		return CheckLineDetailed (start,end,maxAngle,false) < 0;
	}
	
	public static bool CheckLine (Node from,Node to, float maxAngle) {
		return CheckLineDetailed (from,to,maxAngle,false) < 0;
	}
	
	//Return value -1 equals 'No Obstacles'
	//value >= 0 equals 'Obstacle found'
	//0 --> Infinity equals obstacle [value] units away
	
	public static float CheckLineDetailed (Vector3 from,Vector3 to, float maxAngle,bool returnWorldUnits) {
		Int3 startPos = ToLocal (from);
		
		if (startPos == new Int3(-1,-1,-1)) {
			Debug.LogError ("'From' is outside the grid bounds");
			return 0;
		}
		
		Int3 endPos = ToLocal (to);
		
		if (endPos == new Int3(-1,-1,-1)) {
			Debug.LogError ("'To' is outside the grid bounds");
			return 0;
		}
		
		Node start = GetNode (startPos);
		Node end = GetNode (endPos);
		
		if (!start.walkable) {
			return 0;
		}
		
		return CheckLineDetailed (start,end,maxAngle,returnWorldUnits);
	}
	
	public static float CheckLineDetailed (Node from,Node to, float maxAngle,bool returnWorldUnits) {
		if (from.pos.y != to.pos.y) {
			return 0.0F;
		}
		
		if (!from.walkable) {
			return 0.0F;
		}
		
		Vector3 dir = (Vector3)(to.pos - from.pos);
		Vector3 dir2 = dir.normalized;
		
		Vector3 prePos = -Vector3.one;
		
		float lineAcc = Mathf.Max (active.lineAccuracy,0.01F);
		
		for (float i= 0;i<dir.magnitude;i += lineAcc) {
			Int3 pos = from.pos + (Int3)(dir2 * i);
			Node node = GetNode (pos);
			
			if (i > 0 && prePos != node.vectorPos) {
				Vector3 adir = node.vectorPos - prePos;
				Vector3 adir2 = adir;
				adir2.y = 0;
				if (maxAngle != -1 && Vector3.Angle (adir,adir2) > maxAngle) {
					if (returnWorldUnits) {
						return i*lineAcc*active.grids[from.pos.y].nodeSize;
					} else {
						return i;
					}
				}
			}
			
			prePos = node.vectorPos;
			
			if (node.walkable == false) {
				//Debug.DrawRay (node.vectorPos,Vector3.up,Color.red);
				//Debug.DrawLine (from.vectorPos,to.vectorPos,Color.red);
				if (returnWorldUnits) {
					return i*lineAcc*active.grids[from.pos.y].nodeSize;
				} else {
					return i;
				}
			}
			//Debug.DrawRay (node.vectorPos,Vector3.up,Color.green);
		}
		//Debug.DrawLine (from.vectorPos,to.vectorPos,Color.green);
		return -1;
	}
	
	public void FloodFill (Node node,int areaTimeStamp) {
		
		if (node.areaTimeStamp == areaTimeStamp || !node.walkable) {
			return;
		}
		
		
		area++;
		ArrayList areaColorsArr = new ArrayList (areaColors);
		areaColorsArr.Add (area <= presetAreaColors.Length ? presetAreaColors[area-1] : new Color (Random.value,Random.value,Random.value));
		areaColors = areaColorsArr.ToArray (typeof(Color)) as Color[];
		int searched = 0;
		ArrayList open = new ArrayList ();
		Node current = null;
		
		open.Add (node);
		while (open.Count > 0) {
			searched++;
			if (searched > totalNodeAmount) {
				Debug.Log ("Infinity Loop");
			}
			current = open[0] as Node;
			current.area = area;
			open.Remove (current);
			
			for (int i=0;i<current.enabledConnections.Length;i++) {
				if (current.enabledConnections[i].endNode.areaTimeStamp != areaTimeStamp) {
					current.enabledConnections[i].endNode.areaTimeStamp = areaTimeStamp;
					open.Add (current.enabledConnections[i].endNode);
					
				}
			}
		}
		Debug.Log ("Flood Filled "+searched+ " Nodes, The Grid now contains "+area +" Areas");
	}
	
	public void FloodFillAll () {
		
		area = 0;
		int areaTimeStamp = Mathf.RoundToInt (Random.Range (0,10000));
		int searched = 0;
		ArrayList open = new ArrayList ();
		Node current = null;
		ArrayList areaColorsArr = new ArrayList ();
		areaColorsArr.Add (new Color (Random.value,Random.value,Random.value));
		
		int totalWalkableNodeAmount = 0;//The amount of nodes which are walkable
		
		for (int y=0;y<grids.Length;y++) {//Height
			Grid grid = grids[y];
			for (int z=0;z<grid.depth;z++) {//Depth
				for (int x=0;x<grid.width;x++) {//Depth
					Node node = GetNode (x,y,z);
					if (node.walkable) {
						totalWalkableNodeAmount++;
					}
				}
			}
		}
			
		while (searched < totalWalkableNodeAmount) {
			area++;
			
			areaColorsArr.Add (area <= presetAreaColors.Length ? presetAreaColors[area-1] : new Color (Random.value,Random.value,Random.value));
			if (area > 400) {
				Debug.Log ("Preventing possible Infinity Loop (Searched " + searched+" nodes in the flood fill pass)");
				break;
			}
			for (int y=0;y<grids.Length;y++) {//Height
				Grid grid = grids[y];
				for (int z=0;z<grid.depth;z++) {//Depth
					for (int x=0;x<grid.width;x++) {//Depth
						Node node = GetNode (x,y,z);
						if (node.walkable && node.areaTimeStamp != areaTimeStamp && node.enabledConnections.Length>0) {
							node.areaTimeStamp = areaTimeStamp;
							//searched++;
							open.Add (node);
							z = grid.depth;
							x = grid.width;
							y = grids.Length;
						}
					}
				}
			}
			
			if (open.Count==0) {
				searched=totalWalkableNodeAmount;
				area--;
				break;
			}
			
			while (open.Count > 0) {
				searched++;
				
				
				if (searched > totalWalkableNodeAmount) {
					Debug.LogError ("Infinity Loop, can't flood fill more than the total node amount (System Failure)");
					break;
				}
				current = open[0] as Node;
				current.area = area;
				current.areaTimeStamp = areaTimeStamp;
				open.Remove (current);
				
				for (int i=0;i<current.enabledConnections.Length;i++) {
					if (current.enabledConnections[i].endNode.areaTimeStamp != areaTimeStamp) {
						current.enabledConnections[i].endNode.areaTimeStamp = areaTimeStamp;
						open.Add (current.enabledConnections[i].endNode);
						
					}
				}
			}
			open.Clear ();
		}
		
		areaColors = areaColorsArr.ToArray (typeof(Color)) as Color[];
		
		Debug.Log ("Grid contains "+(area)+" Area(s)");
	}
	
	[ContextMenu ("Scan Map")]
	public void Scan () {
		Scan (true,-1);
	}
	
	// Update is called once per frame
	public void Scan (bool calcAll,int pass) {
		active = this;
		
		//float startTime1 = Time.realtimeSinceStartup;
		
		for (int i=0;i<grids.Length;i++) {//Depth
			grids[i].Reset ();
		}
		
		if (gridGenerator == GridGenerator.Texture) {
			if (calcAll) {
				ScanTexture ();
			} else {
				Debug.LogWarning ("The Texture Mode don't use passes, calculate everything once instead");
			}
			binaryHeap = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount*heapSize));
			return;
		}
		
		if (gridGenerator == GridGenerator.Bounds) {
			if (calcAll) {
				ScanBounds ();
			} else {
				Debug.LogWarning ("The Bounds Mode don't use passes, calculate everything once instead");
			}
			binaryHeap = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount*heapSize));
			return;
			
		}
		
		if (gridGenerator == GridGenerator.List) {
			if (calcAll) {
				ScanList ();
			} else {
				Debug.LogWarning ("The List Mode don't use passes, calculate everything once instead");
			}
			binaryHeap = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount*heapSize));
			return;
			
		}
		
		if (gridGenerator == GridGenerator.Mesh) {
			if (calcAll) {
				ScanNavmesh ();
			} else {
				Debug.LogWarning ("The Mesh Mode don't use passes, calculate everything once instead");
			}
			binaryHeap = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount*heapSize));
			return;
			
		}
		
		if (gridGenerator == GridGenerator.Procedural) {
			return;
		}
			
		//float startTime2 = Time.realtimeSinceStartup;
		
		if (pass==1 || calcAll) {
			staticNodes = new Node[grids.Length][,];
			totalNodeAmount = 0;
			for (int i=0;i<grids.Length;i++) {//Depth
				staticNodes[i] = new Node [grids[i].width,grids[i].depth];
				totalNodeAmount += grids[i].width*grids[i].depth;
				grids[i].changed = false;
			}
		}
		
		//float startTime3 = Time.realtimeSinceStartup;
			
		//Debug.Log ("Pass 1");
		bool anyWalkable = false;
		for (int y=0;y<grids.Length && (pass==1 || calcAll) ;y++) {//Height
			Grid grid = grids[y];
			for (int z=0;z<grid.depth;z++) {//Depth
				for (int x=0;x<grid.width;x++) {//Width
					//Calculate the position of the node
					Node node = staticNodes[y][x,z] = new Node ();
					node.pos = new Int3 (x,y,z);
					node.vectorPos = new Vector3 (((float)x+0.5F)*grid.nodeSize+grid.offset.x,grid.offset.y,((float)z+0.5F)*grid.nodeSize+grid.offset.z);
					
					FullPhysicsCheck (node,grid,0);
					if (node.walkable) {
						anyWalkable = true;
					}
					
					node.connections = new Connection[8];
					
					for (int i=0;i<8;i++) {
						node.connections[i] = new Connection ();
						node.connections[i].cost = costs[i];
					}
				}
			}
		}
		
		//Stop here if we are using the quadtree post processor
		if (quadtreePostProcess && (pass > 1 || calcAll)) {
			ScanQuadtree ();
			return;
		}
		
		//float startTime4 = Time.realtimeSinceStartup;
		
		//Pass 2
		//Debug.Log ("Pass 2");
		for (int y=0;y<grids.Length  && (pass==2 || calcAll);y++) {//Height
			Grid grid = grids[y];
			for (int z=0;z<grid.depth;z++) {//Depth
				for (int x=0;x<grid.width;x++) {//Width
					Node node = GetNode(x,y,z);
					/*Keys:
					0:Top Left
					1:Top
					2:Top Right
					3:Left
					4:Right
					5:Bottom Left
					6:Bottom
					7:Bottom Right
					*/
					
					//The if's look a bit strange because I wanted to minimize the amount of if's in the code
					Vector3 vector;
					float angle;
					if (x != 0) {
						
						//The direction to another node
						vector = GetNode(x-1,y,z).vectorPos-node.vectorPos;
						//We can set the other nodes vector angle to -this angle because the direction is the opposite
						//GetNode(x-1,y,z).vectorAngles[4] = -node.vectorAngles[3];
						angle = Vector3.Angle (vector,-Vector3.right);
						node.connections[3].angle = angle;
						GetNode(x-1,y,z).connections[4].angle = angle;
						
					}
					
					if (z != 0) {
						
						
						vector = GetNode(x,y,z-1).vectorPos-node.vectorPos;
						//GetNode(x,y,z-1).vectorAngles[1] = -node.vectorAngles[6];
						
						angle = Vector3.Angle (vector,-Vector3.forward);
						node.connections[6].angle = angle;
						GetNode(x,y,z-1).connections[1].angle = angle;
						
						if (x != 0) {
							
							vector = GetNode(x-1,y,z-1).vectorPos-node.vectorPos;
							//GetNode(x-1,y,z-1).vectorAngles[2] = -node.vectorAngles[5];
							
							angle = Vector3.Angle (vector,-Vector3.right-Vector3.forward);
							node.connections[5].angle = angle;
							GetNode(x-1,y,z-1).connections[2].angle = angle;
						}
						
						if (x != grid.width-1) {
							
							vector = GetNode(x+1,y,z-1).vectorPos-node.vectorPos;
							
							angle = Vector3.Angle (vector,Vector3.right-Vector3.forward);
							node.connections[7].angle = angle;
							GetNode(x+1,y,z-1).connections[0].angle = angle;
						}
					}
				}
			}
		}
		
		//float startTime5 = Time.realtimeSinceStartup;
		
		if (pass == 3 || calcAll) {
			ApplyEnablerLinks ();
		}
		
		//float startTime6 = Time.realtimeSinceStartup;
		
		//Debug.Log ("Pass 3");
		for (int y=0;y<grids.Length && (pass==3 || calcAll);y++) {//Height
			Grid grid = grids[y];
			for (int z=0;z<grid.depth;z++) {//Depth
				for (int x=0;x<grid.width;x++) {//Width
					Node node = GetNode(x,y,z);
					
					RecalcNeighbours (node);
				}
			}
		}
		
		//float startTime7 = Time.realtimeSinceStartup;
		
		if (pass == 3 || calcAll) {
			ApplyLinks ();
		}
		
		//float startTime8 = Time.realtimeSinceStartup;
		
		
		if ((pass == 4 || calcAll)) {
			
			//Debug.Log ("Pass 4");
			//Pass 4
			FloodFillAll ();
		}
		
		//float startTime9 = Time.realtimeSinceStartup;
		
		if ((pass==1 || calcAll) && !anyWalkable) {
			Debug.LogError ("No nodes are walkable (maybe you should change the layer mask)");
		}
		
		binaryHeap = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount*heapSize));
		
		//float startTime10 = Time.realtimeSinceStartup;
		
		//Debug.Log ("Times are:\n1"+(startTime2-startTime1)+"\n2"+(startTime3-startTime2)+"\n3"+(startTime4-startTime3)+"\n4"+(startTime5-startTime4)+"\n5"+(startTime6-startTime5)+"\n6"+(startTime7-startTime6)+"\n7"+(startTime8-startTime7)+"\n8"+(startTime9-startTime8)+"\n9"+(startTime10-startTime9)+"\n");
		//Debug.Log ("Length is "+nodes.Length);
	}
		
	public void ScanTexture () {
		if (navTex == null) {
			return;
		}
		
		Color[] pixels = navTex.GetPixels (0);
		
		Grid grid = textureGrid;
		grids = new Grid[1] {grid};
		grid.width = navTex.width;
		grid.depth = navTex.height;
			
		staticNodes = new Node[1][,];
		staticNodes[0] = new Node [grid.width,grid.depth];
		totalNodeAmount = grid.depth * grid.width;
		
		for (int z=0;z<grid.depth;z++) {//Depth/Height
			for (int x=0;x<grid.width;x++) {//Width
				Node node = new Node ();
				node.pos = new Int3 (x,0,z);
				
				node.vectorPos = new Vector3 ((x+0.5F)*grid.nodeSize,0,(z+0.5F)*grid.nodeSize) + grid.offset;
				
				//if (pixels[z*grid.width+x].grayscale <= threshold) {
				//	node.walkable = false;
				//}
				node.walkable = TextureEvaluateNode (x,z);
				
				node.penalty = (int)(pixels[z*grid.width+x].r*penaltyMultiplier);
				
				staticNodes[0][x,z] = node;
				
				node.connections = new Connection[8];
					
				for (int i=0;i<8;i++) {
					node.connections[i] = new Connection ();
					node.connections[i].cost = costs[i];
				}
			}
		}
		
		//Stop here if we are using the quadtree post processor
		if (quadtreePostProcess) {
			ScanQuadtree ();
			return;
		}
		
		ApplyEnablerLinks ();
		
		for (int z=0;z<grid.depth;z++) {//Depth
			for (int x=0;x<grid.width;x++) {//Width
				Node node = GetNode (x,0,z);
				RecalcNeighbours (node);
			}
		}
		
		ApplyLinks ();
		
		FloodFillAll ();
	}
	
	public bool TextureEvaluateNode (int x,int z) {
		
		if (navTex.GetPixel(x,z).grayscale <= threshold) {
			return false;
		}
		return true;
	}
	
	public void ScanTilemap (bool[] array,int width,int depth) {
		
		if (width * depth != array.Length) {
			Debug.LogError ("The array length and width*depth values must match");
			return;
		}
		if (gridGenerator != GridGenerator.Texture) {
			Debug.LogError ("Only use this grid generator with the Texture mode");
			return;
		}
		
		if (!calculateOnStartup) {
			Debug.LogWarning ("To prevent that other grids gets generated at startup just to be replaced by this grid you should switch Calculate Grid On Startup to FALSE");
		}
		
		Grid grid = textureGrid;
		grids = new Grid[1] {grid};
		grid.width = width;
		grid.depth = depth;
		
		//The first node is placed at position 0, so the actual width of the grid will be one less than the number of nodes in one direction
		grid.globalWidth = grid.width-1;
		grid.globalDepth = grid.depth-1;
			
		//grid.nodeSize = 1;
		staticNodes = new Node[1][,];
		staticNodes[0] = new Node [grid.width,grid.depth];
		totalNodeAmount = grid.depth * grid.width;
		
		for (int z=0;z<grid.depth;z++) {//Depth
			for (int x=0;x<grid.width;x++) {//Width
				Node node = new Node ();
				node.pos = new Int3 (x,0,z);
				
				node.vectorPos = new Vector3 (x*grid.nodeSize,0,z*grid.nodeSize) + grid.offset;
				//new Vector3 (x,0,z) + grid.offset;
				if (!array[z*grid.depth+x]) {
					node.walkable = false;
				}
				//node.penalty = (int)(pixels[z*grid.depth+x].r*10);
				staticNodes[0][x,z] = node;
			}
		}
		
		ApplyEnablerLinks ();
		
		for (int z=0;z<grid.depth;z++) {//Depth
			for (int x=0;x<grid.width;x++) {//Width
				Node node = GetNode (x,0,z);
				RecalcNeighbours (node);
			}
		}
		
		ApplyLinks ();
		
		FloodFillAll ();
	}
	
	public void ScanBounds () {
		
		Collider[] all = FindObjectsOfType (typeof(Collider)) as Collider[];
		ArrayList allArr = new ArrayList ();
		for (int i=0;i<all.Length;i++) {
			if (!all[i].isTrigger && all[i].gameObject.tag == boundsTag) {
				allArr.Add (all[i]);
			}
		}
		all = allArr.ToArray (typeof(Collider)) as Collider[];
		
		Vector3[] points = new Vector3[all.Length * 4];
		for (int i=0;i<all.Length;i++) {
			Collider obj = all[i];
			
			Bounds b = obj.bounds;
			points[i*4 + 0] = new Vector3 (b.extents.x+boundMargin,0,b.extents.z+boundMargin) + b.center;
		
			points[i*4 + 1] = new Vector3 (-b.extents.x-boundMargin,0,-b.extents.z-boundMargin) + b.center;
		
			points[i*4 + 2] = new Vector3 (b.extents.x+boundMargin,0,-b.extents.z-boundMargin) + b.center;
			points[i*4 + 3] = new Vector3 (-b.extents.x-boundMargin,0,b.extents.z+boundMargin) + b.center;
			
			
		}
		
		ArrayList pointArr = new ArrayList ();
		for (int i=0;i<points.Length;i++) {
			bool similarity = false;
			for (int y=0;y<points.Length;y++) {
				if (points[i] == points[y] && i != y) {
					similarity = true;
				}
			}
			if (!similarity) {
				pointArr.Add (points[i]);
			}
		}
		
		points = pointArr.ToArray (typeof (Vector3)) as Vector3[];
		
		GenerateNavmesh (points);
	}
	
	public void ScanNavmesh () {
		if (meshNodePosition == MeshNodePosition.Edge) {
			ScanNavmeshEdge ();
		} else {
			ScanNavmeshCenter ();
		}
	}
	
	public void ScanNavmeshEdge () {
		
		Vector3[] meshPoints = navmesh.vertices;
		int[] tris = navmesh.triangles;
		Grid grid = meshGrid;
		
		GenerateRotationMatrix (grid);
		
		if (meshPoints.Length <= 3) {
			Debug.LogError ("Mesh Scanner : Make sure the mesh does contains at least three vertices");
			return;
		}
		
		for (int i=0;i<meshPoints.Length;i++) {
			//Apply rotation, scale and offset to the points
			meshPoints[i] = rotationMatrix.TransformVector (meshPoints[i]);//f
			//meshPoints[i] = RotatePoint (meshPoints[i])* meshGrid.nodeSize;
		}
		
		//Calculate all edge points
		Edge[] points = new Edge[tris.Length];
		Vector3[] trisCenters = new Vector3[tris.Length/3];
		for (int i=0;i<tris.Length/3;i++) {
			//Vector3 point = ((meshPoints[tris[i*3]] + meshPoints[tris[i*3+1]] + meshPoints[tris[i*3+2]]) / 3);
			Vector3 p1 = meshPoints[tris[i*3]];
			Vector3 p2 = meshPoints[tris[i*3+1]];
			Vector3 p3 = meshPoints[tris[i*3+2]];
			trisCenters[i] = (p1+p2+p3)/3.0F;
			points[i*3] = new Edge (p1,p2);
			points[i*3+1] = new Edge (p1,p3);
			points[i*3+2] = new Edge (p2,p3);
		}
		
		bool[] isDuplicate = new bool[points.Length];
		bool[] isWalkable = new bool[points.Length];
		//Loop through all points and remove all doubles, store which triangles the node exists in, at most it will be two triangles
		for (int i=0;i<points.Length;i++) {
			
			if (isDuplicate[i]) {
				continue;
			}
			
			points[i].triIndex1 = Mathf.FloorToInt (i/3.0F);
			points[i].triIndex2 = -i;
			points[i].offset = i-points[i].triIndex1*3;
			//Check for duplicates
			for (int x=i+1;x<points.Length;x++) {
				if (points[x] == points[i]) {
					//Don't use this node anymore since we have got two copies of it
					points[i].triIndex2 = Mathf.FloorToInt (x/3.0F);
					isDuplicate[x] = true;
					isWalkable[x] = true;
					isWalkable[i] = true;
					points[i].Merge (points[x]);
					points[x] = points[i];
					
					break;
				}
			}
			
			//Remove the node if it does only exist in one triangle (i.e the next node will write over this node in the points2 array since the counter (c) isn't incremented
			
		}
		
		for (int i=0;i<points.Length;i++) {
		
			if (!isWalkable[i] && !isDuplicate[i]) {
				Edge e1 = points[i];
				
				e1.DebugError ();
				//The index number of the edge (0,1 or 2)
				//The first edge of the triangle this node exists in
				int tri = e1.triIndex1*3;
				int tri2 = e1.triIndex1;
				int offset = i-tri;
				
				
				switch (offset) {
					case 0:
						//Debug.DrawRay (points[triConn1[c]*3+1].endPointA,Vector3.up,Color.red);
						//Debug.DrawRay (points[triConn1[c]*3+2].endPointA,Vector3.up,Color.red);
						points[tri+1].SetBorderA (true,tri2,e1.endPointB);
						points[tri+2].SetBorderA (true,tri2,e1.endPointA);
						break;
					case 1:
						//Debug.DrawRay (points[triConn1[c]*3].endPointA,Vector3.up,Color.red);
						//Debug.DrawRay (points[triConn1[c]*3+2].endPointB,Vector3.up,Color.red);
						points[tri].SetBorderA (true,tri2,e1.endPointB);
						points[tri+2].SetBorderB (true,tri2,e1.endPointA);
						break;
					case 2:
						Debug.DrawRay (points[tri].endPointA,Vector3.up*2,Color.red);
						Debug.DrawRay (points[tri].endPointB,Vector3.up,Color.black);
						points[tri].SetBorderB (true,tri2,e1.endPointB);//@Error
						points[tri+1].SetBorderB (true,tri2,e1.endPointA);
						
						/*if (tri2 == 23 || tri2 == 24) {
							Debug.DrawLine (e1.endPointA,e1.endPointB,Color.red);
							Debug.DrawRay (e1.endPointA,Vector3.up,Color.blue);
							Debug.DrawRay (e1.endPointB,Vector3.up,Color.yellow);
							//Debug.DrawRay (points[tri+1].center,Vector3.up*2,Color.cyan);
							Debug.Log ("Position is "+points[tri+1].center);
							Debug.DrawRay (points[tri].center,Vector3.up,Color.cyan*10);
						}*/
				
						break;
					default:
						Debug.LogError ("Triangles with more than 3 vertices?? Eeeh...");
						break;
				}
				/*for (int x=0;x<points.Length;x++) {
					Edge e2 = points[x];
					if (e1.endPointA == e2.endPointA || e1.endPointB == e2.endPointA) {
						e2.borderA = true;
						//Debug.DrawRay (e2.endPointA,Vector3.up,Color.red);
						//Debug.DrawLine (e2.endPointA,e2.endPointB-e2.edgeVector3D*0.6F,Color.red);
					}
					if (e1.endPointA == e2.endPointB || e1.endPointB == e2.endPointB) {
						e2.borderB = true;
						//Debug.DrawRay (e2.endPointB,Vector3.up,Color.red);
						//Debug.DrawLine (e2.endPointA+e2.edgeVector3D*0.6F,e2.endPointB,Color.red);
					}
				}*/
			}
			
			//The last unwalkable node hasn't been written over so we have to prevent it from becoming a walkable node
			
			/* else {//Does only exist in one triangle
				Debug.DrawLine (points[i].endPointA,points[i].endPointB,Color.red);
				int offset = i-triConn1[c]*3;
				switch (offset) {
					case 0:
						Debug.DrawRay (points[triConn1[c]*3+1].endPointA,Vector3.up,Color.red);
						Debug.DrawRay (points[triConn1[c]*3+2].endPointA,Vector3.up,Color.red);
						points[triConn1[c]*3+1].borderA = true;
						points[triConn1[c]*3+2].borderA = true;
						break;
					case 1:
						Debug.DrawRay (points[triConn1[c]*3].endPointA,Vector3.up,Color.red);
						Debug.DrawRay (points[triConn1[c]*3+2].endPointB,Vector3.up,Color.red);
						points[triConn1[c]*3].borderA = true;
						points[triConn1[c]*3+2].borderB = true;
						break;
					case 2:
						Debug.DrawRay (points[triConn1[c]*3].endPointB,Vector3.up,Color.red);
						Debug.DrawRay (points[triConn1[c]*3+1].endPointB,Vector3.up,Color.red);
						points[triConn1[c]*3].borderB = true;
						points[triConn1[c]*3+1].borderB = true;
						break;
				}
			}*/
		}
		
		
		//The counter of how many nodes have been added, some nodes are ignored (see below)
		int c = 0;
		Edge[] points2 = new Edge[points.Length];
		bool[] isWalkable2 = new bool[points.Length];
		bool lastOneWasUnwalkable = false;
		for (int i=0;i<points.Length;i++) {
			if (!isDuplicate[i]) {
				points2[c] = points[i];
				isWalkable2[c] = isWalkable[i];
				c++;
				lastOneWasUnwalkable = false;
			} else if (!isWalkable[i]) {
				lastOneWasUnwalkable = true;
			}
		}
				
		if (lastOneWasUnwalkable) {
			//c--;
		}
			
		//Make sure there is at least one connection
		if (c <= 0) {
			Debug.LogError ("Mesh Scanner : Make sure there is at least one connection");
			return;
		}
		
		//Trim the array to only include the nodes which we should use
		Edge[] points3 = new Edge[c];
		bool[] isWalkable3 = new bool[c];
		for (int i=0;i<c;i++) {
			points3[i] = points2[i];
			isWalkable3[i] = isWalkable2[i];
		}
		
		points = points3;
		isWalkable = isWalkable3;
		
		//Create the bounds of the grid
		Bounds b = new Bounds (points[0].center,Vector3.zero);
		for (int i=0;i<points.Length;i++) {
			b.Encapsulate (points[i].center);
		}
		
		b.extents += Vector3.one * boundsMargin;
		//Debug.Log (""+b.ToString ());
		grids = new Grid[1] {grid};
		grid.width = points.Length;
		grid.depth = 1;
		grid.globalWidth = b.size.x/grid.nodeSize;
		
		grid.globalDepth = b.size.z/grid.nodeSize;
		grid.globalHeight = b.size.y;
		
		grid.globalOffset = (b.center-b.extents)-grid.offset;
		//grid.nodeSize = 1;
		staticNodes = new Node[1][,];
		staticNodes[0] = new Node [grid.width,grid.depth];
		totalNodeAmount = grid.width*grid.depth;
		
		//Initialize all nodes
		for (int x=0;x<grid.width;x++) {//Width
			Node node = new MeshEdgeNode ();//MeshEdgeNode is used to gain extra functionality such as 2D area storage
			node.pos = new Int3 (x,0,0);
			node.edge = points[x];
			node.vectorPos = points[x].center;
			node.walkable = isWalkable[x];
			staticNodes[0][x,0] = node;
		}
		
		ApplyEnablerLinks ();
		
		//Create the connections between the nodes
		for (int x=0;x<grid.width;x++) {
			MeshEdgeNode node = (MeshEdgeNode)staticNodes[0][x,0];
			
			int triConnection1 = node.edge.triIndex1;
			int triConnection2 = node.edge.triIndex2;
			
			//if (node.edge.borderA || node.edge.borderB) {
				
				/*ArrayList area = new ArrayList ();
				bool drawDebug = x == 30;
				
				if (drawDebug) {
					Debug.Log ("Offset is "+node.edge.offset);
				}
				
				//area.Add (trisCenters[triConnection1]);
				area.Add (node.edge.endPointA);
				if (node.edge.borderA1 && node.edge.borderB1) {
					area.Add (node.edge.borderA1E);
					if (drawDebug) { Debug.DrawRay ((Vector3)area[area.Count-1],Vector3.up,Color.red); }
				} else if (node.edge.borderA1) {
					
					area.Add ((node.edge.borderA1E+node.edge.endPointA)/2.0F);
					if (drawDebug) { Debug.DrawRay ((Vector3)area[area.Count-1],Vector3.up,Color.green); }
				} else if (node.edge.borderB1) {
					
					area.Add ((node.edge.borderB1E+node.edge.endPointB)/2.0F);
					if (drawDebug) { Debug.DrawRay ((Vector3)area[area.Count-1],Vector3.up,Color.blue); }
				} else {
					area.Add (trisCenters[triConnection1]);
					if (drawDebug) { Debug.DrawRay ((Vector3)area[area.Count-1],Vector3.up,Color.yellow); }
				}
				
				area.Add (node.edge.endPointB);
				
				if (node.edge.borderA2 && node.edge.borderB2) {
					area.Add (node.edge.borderA2E);//the same as borderB2E
				} else if (node.edge.borderA2) {
					
					area.Add ((node.edge.borderA2E+node.edge.endPointA)/2.0F);
					if (drawDebug) { Debug.DrawRay ((Vector3)area[area.Count-1],Vector3.up,Color.magenta); }
				} else if (node.edge.borderB2) {
					
					area.Add ((node.edge.borderB2E+node.edge.endPointB)/2.0F);
					if (drawDebug) { Debug.DrawRay ((Vector3)area[area.Count-1],Vector3.up,Color.cyan); }
				} else {
					area.Add (trisCenters[triConnection2]);
					if (drawDebug) { Debug.DrawRay ((Vector3)area[area.Count-1],Vector3.up,Color.gray); }
				}
				
				Vector3[] area3D = area.ToArray (typeof(Vector3)) as Vector3[];
				node.area2D = new Vector2[area3D.Length];
				
				Vector3 debugCenter = Vector3.zero;
				for (int i=0;i<area3D.Length;i++) {
					debugCenter += area3D[i];
					node.area2D[i] = new Vector2 (area3D[i].x,area3D[i].z);
				}
				debugCenter /= area3D.Length;
				for (int i=0;i<area3D.Length;i++) {
					if (i == area3D.Length-1) {
						Debug.DrawLine (debugCenter+ (area3D[i]-debugCenter)*0.9F,debugCenter+ (area3D[0]-debugCenter)*0.9F,drawDebug ? Color.cyan : Color.magenta);
					} else {
						Debug.DrawLine (debugCenter+ (area3D[i]-debugCenter)*0.9F,debugCenter+ (area3D[i+1]-debugCenter)*0.9F,drawDebug ? Color.cyan : Color.magenta);
					}
				}
				Debug.Log (triConnection1+ "  "+triConnection2+"  "+trisCenters.Length);
			} else {*/
				if (triConnection2 >= 0) {
					Vector3 p1 = trisCenters[triConnection1];
					Vector3 p2 = node.edge.endPointA;
					Vector3 p3 = trisCenters[triConnection2];
					Vector3 p4 = node.edge.endPointB;
					node.area2D = new Vector2[4] { new Vector2(p1.x,p1.z),new Vector2(p2.x,p2.z),new Vector2(p3.x,p3.z),new Vector2(p4.x,p4.z) };
				} else {
					Vector3 p1 = trisCenters[triConnection1];
					Vector3 p2 = node.edge.endPointA;
					Vector3 p3 = node.edge.endPointB;
					node.area2D = new Vector2[3] { new Vector2(p1.x,p1.z),new Vector2(p2.x,p2.z),new Vector2(p3.x,p3.z)};
				}
			//}
			
			if (!node.walkable) {
				//continue;
			}
			
			ArrayList edges = new ArrayList ();
			
			ArrayList connections = new ArrayList ();
			
			for (int y=0;y<grid.width;y++) {
				if (y == x) {
					continue;
				}
				Node otherNode = staticNodes[0][y,0];
				
				int triConnection12 = otherNode.edge.triIndex1;
				int triConnection22 = otherNode.edge.triIndex2;
				
				//If another node exists in the same triangle as this one, they should make a connection to each other
				if (triConnection12 == triConnection1 || triConnection12 == triConnection2 || triConnection22 == triConnection1 || triConnection22 == triConnection2) {
					
					
					if (!otherNode.walkable) {
						continue;
					}
			
					Vector3 dir = (otherNode.vectorPos - node.vectorPos);
					Vector3 dir2 = dir;
					dir2.y = 0;
					float a = Vector3.Angle (dir.normalized,dir2.normalized);
					
					float dist = (otherNode.vectorPos-node.vectorPos).sqrMagnitude;
					int cost = Mathf.RoundToInt (Mathf.Sqrt(dist)*100);
					
					connections.Add (new Connection(otherNode,cost,a,true));
					
					edges.Add (otherNode.edge);
				}
			}
			
			node.edges = edges.ToArray (typeof (Edge)) as Edge[];
			node.connections = connections.ToArray (typeof (Connection)) as Connection[];
			node.GenerateEnabledConnections ();
		}
		
		ApplyLinks ();
		FloodFillAll ();
	}
	
	
	public void ScanNavmeshCenter () {
		
		
		Vector3[] meshPoints = navmesh.vertices;
		int[] tris = navmesh.triangles;
		Grid grid = meshGrid;
		
		GenerateRotationMatrix (grid);
		
		if (meshPoints.Length <= 2) {
			Debug.LogError ("Make sure the mash does contains at least two vertices");
			return;
		}
		
		for (int i=0;i<meshPoints.Length;i++) {
			meshPoints[i] = rotationMatrix.TransformVector (meshPoints[i]);
		}
		
		Vector3[] points = new Vector3[(int)(tris.Length/3)];
		for (int i=0;i<tris.Length/3;i++) {
			points[i] = ((meshPoints[tris[i*3]] + meshPoints[tris[i*3+1]] + meshPoints[tris[i*3+2]]) / 3);
		}
		
		Bounds b = new Bounds (points[0],Vector3.zero);
		for (int i=0;i<points.Length;i++) {
			b.Encapsulate (points[i]);
		}
		
		b.extents += Vector3.one * boundsMargin;
		//Debug.Log (""+b.ToString ());
		grids = new Grid[1] {grid};
		grid.width = points.Length;
		grid.depth = 1;
		grid.globalWidth = b.size.x/grid.nodeSize;
		
		grid.globalDepth = b.size.z/grid.nodeSize;
		grid.globalHeight = b.size.y;
		
		grid.globalOffset = (b.center-b.extents)-grid.offset;
		//grid.nodeSize = 1;
		staticNodes = new Node[1][,];
		staticNodes[0] = new Node [grid.width,grid.depth];
		totalNodeAmount = grid.width*grid.depth;
		
		
		for (int x=0;x<grid.width;x++) {//Width
			MeshCenterNode node = new MeshCenterNode ();
			node.pos = new Int3 (x,0,0);
			node.vectorPos = points[x];
			
			/*node.edges = new Edge[3] {
				new Edge (meshPoints[tris[x*3]],meshPoints[tris[x*3+1]],node),
				new Edge (meshPoints[tris[x*3]],meshPoints[tris[x*3+2]],node),
				new Edge (meshPoints[tris[x*3+1]],meshPoints[tris[x*3+2]],node)
			};*/
			
			staticNodes[0][x,0] = node;
			
			//Set the area of the node (triangle vertices)	
			Vector3[] verts = new Vector3[3] {
				meshPoints[tris[x*3]],
				meshPoints[tris[x*3+1]],
				meshPoints[tris[x*3+2]]
			};
			
			node.area2D = new Vector2[3] {new Vector2 (verts[0].x,verts[0].z),new Vector2 (verts[1].x,verts[1].z),new Vector2 (verts[2].x,verts[2].z)};
		}
		
		ApplyEnablerLinks ();
		
		for (int x=0;x<grid.width;x++) {
			MeshCenterNode node = (MeshCenterNode)staticNodes[0][x,0];
			
			Vector3[] verts = new Vector3[3] {
				meshPoints[tris[x*3]],
				meshPoints[tris[x*3+1]],
				meshPoints[tris[x*3+2]]
			};
			
			if (!node.walkable) {
				continue;
			}
		
			ArrayList edges = new ArrayList ();
			
			ArrayList connections = new ArrayList ();
			
			for (int y=0;y<grid.width;y++) {
				if (y == x) {
					continue;
				}
				
				int similarities = 0;
				//int[] verts2 = new int[3] {tris[y*3],tris[y*3+1],tris[y*3+2]};
				
				Vector3[] verts2 = new Vector3[3] {
					meshPoints[tris[y*3]],
					meshPoints[tris[y*3+1]],
					meshPoints[tris[y*3+2]]
				};
			
				Vector3 match1 = Vector3.zero;
				Vector3 match2 = Vector3.zero;
				
				foreach (Vector3 vert in verts) {
					foreach (Vector3 vert2 in verts2) {
						if (vert == vert2) {
							if (similarities == 0) {
								match1 = vert;
							} else {
								match2 = vert;
							}
							similarities++;
						}
					}
				}
				
				if (similarities >= 2) {
					
					Node otherNode = staticNodes[0][y,0];
					edges.Add (new Edge(match1,match2));
					if (!otherNode.walkable) {
						continue;
					}
					
					if (connections.Count >= 3) {
						Debug.DrawLine (verts2[0],verts2[1],Color.red);
						Debug.DrawLine (verts2[1],verts2[2],Color.red);
						Debug.DrawLine (verts2[2],verts2[0],Color.red);
						Debug.LogError ("The system does only support polygons with at most three connections");
						continue;
					}
			
					float dist = (otherNode.vectorPos-node.vectorPos).sqrMagnitude;
					
					Vector3 dir = (otherNode.vectorPos - node.vectorPos);
					Vector3 dir2 = dir;
					dir2.y = 0;
					float a = Vector3.Angle (dir.normalized,dir2.normalized);
					
					connections.Add (new Connection (otherNode,Mathf.RoundToInt (Mathf.Sqrt(dist)*100),a,true));
					/*for (int e=0;e<3;e++) {
						for (int e2=0;e2<;e2++) {
							if (node.edges[e] == otherNode.edges[e2]) {
								//Make the nodes share the edge class to save memory
								otherNode.edges[e2] = node.edges[e];
								//node.edges[e].border = false;
							}
						}
					}*/
					
				}
			}
			
			node.edges = edges.ToArray (typeof(Edge)) as Edge[];
			node.connections = connections.ToArray (typeof (Connection)) as Connection[];
			node.GenerateEnabledConnections ();
		}
		
		ApplyLinks ();
		FloodFillAll ();
	}
	
	public void ScanList () {
		if (listRootNode == null) {
			Debug.LogError ("No Root Node Was Assigned");
			return;
		}
		
		Transform[] listNodes = GetChildren (listRootNode);//listRootNode.GetComponentsInChildren(typeof(Transform)) as Transform[];
		Vector3[] points = new Vector3[listNodes.Length];
		for (int i=0;i<listNodes.Length;i++) {
			points[i] = listNodes[i].position;
		}
		GenerateNavmesh (points);
	}
	
	//-------- QUADTREE ------------>
	
	public void ScanQuadtree () {
		
		for (int y=0;y<grids.Length;y++) {//Height
			Grid grid = grids[y];
			
			ArrayList que = new ArrayList ();
			
			int log = (int)Mathf.Log (grid.width,2);
				
			
			Node start = new Node ();
			start.pos = new Int3(0,y,0);
			start.vectorPos = new Vector3 ((grid.width/2.0F)*grid.nodeSize,grid.offset.y,(grid.depth/2.0F)*grid.nodeSize);
			start.dia = grid.width*grid.nodeSize;
			start.depth = 0;
			int c = 0;
			que.Add (start);
			while (que.Count > 0) {
				c++;
				Node node = que[0] as Node;
				que.RemoveAt (0);
				
				int w = (int)Mathf.Pow (2,log-node.depth);
				int mode = IsSingleArea (node,w,true);
				if ((mode == 0 || mode == 1) && (node.depth >= quadtreeMinDepth || node.depth >= log)) {
					
					
					node.walkable = mode == 1;
					
					float r = Mathf.RoundToInt (Mathf.Pow (2,log-node.depth-1));
					FillWithNode (node,node.pos,w);
					
					if (node.pos.x+r == 196 && node.pos.z+r == 192) {
						Debug.Log ("Node 196*192\nR : "+r+"\nDepth: "+node.depth+"\nPos : "+node.pos);
					}
					
					node.vectorPos = new Vector3(
					(node.pos.x+r+ (node.depth >= log ? 0.5F : 0) )*grid.nodeSize+grid.offset.x,
					node.vectorPos.y,
					(node.pos.z+r+ (node.depth >= log ? 0.5F : 0) )*grid.nodeSize+grid.offset.z
					);
					
					continue;
				} else {
				
					int width = (int)Mathf.Pow (2,log-node.depth-1);
					for (int z=0;z<2;z++) {//Depth
						for (int x=0;x<2;x++) {//Width
							//Debug.Log (x+"  "+z+"  "+width+"  "+node.depth+"  "+node.dia);
							int x2 = x*width+node.pos.x;
							int z2 = z*width+node.pos.z;
							//Calculate the position of the node
							Node node2 /*= staticNodes[y][x2,z2]*/ = new Node ();
							node2.pos = new Int3 (x2,y,z2);
							
							//node2.vectorPos = new Vector3 (
							//node.vectorPos.x+(x*width*1.0F*grid.nodeSize+grid.offset.x),
							//grid.offset.y,
							//node.vectorPos.z+(z*width*1.0F*grid.nodeSize+grid.offset.z));
							
							node2.dia = node.dia/2.0F;
							node2.depth = node.depth+1;
							que.Add (node2);
							node.walkable = false;
						}
					}
					
				}
			}
		}
		
		ApplyEnablerLinks ();
		
		for (int y=0;y<grids.Length;y++) {//Height
			Grid grid = grids[y];
			int log = (int)Mathf.Log (grid.width,2);
			for (int z=0;z<grid.depth;z++) {//Depth
				for (int x=0;x<grid.width;x++) {//Width
					Node node = staticNodes[y][x,z];
					
					//Don't waste time calculating the neighbours multiple times
					if (node.connections != null) {
						continue;
					}
					
					//The neighbours array is filled with the nodes surounding this node, it contains no duplicates but may contain unwalkable nodes
					Node[] neighbours = GetSurroundingNodes (node,log);
					
					//Create the connections, the angle between the nodes is always 0 (zero)
					node.connections = new Connection[neighbours.Length];
					for (int i=0;i<neighbours.Length;i++) {
						int cost = Mathf.RoundToInt ((Vector3.Distance (node.vectorPos,neighbours[i].vectorPos)/grid.nodeSize)*10.0F);
						node.connections[i] = new Connection (neighbours[i],cost,0,neighbours[i].walkable);
					}
					
					node.GenerateEnabledConnections ();
					
					int r = Mathf.RoundToInt ((int)Mathf.Pow (2,log-node.depth-1));
					node.pos = node.pos+new Int3 (r,0,r);
				}
			}
		}
		
		ApplyLinks ();
		
		FloodFillAll ();
		binaryHeap = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount*heapSize));
	}
	
	public Node[] GetSurroundingNodes (Node node, int log) {
		Grid grid = grids[node.pos.y];
		
		//This is the width of the node, when using a Quadtree, nodes can have differnt widths, so we must take that into account
		int w = (int)Mathf.Pow (2,log-node.depth);
		
		ArrayList nodes = new ArrayList ();
		
		//Loop trough the nodes sourounding (and containing) the node
		for (int x=node.pos.x-1;x<node.pos.x+w+1;x++) {
			for (int z=node.pos.z-1;z<node.pos.z+w+1;z++) {
			
				//Ignore positions outside the grid bounds and inside this node's bounds
				if ((x >= node.pos.x && x < node.pos.x+w && z >= node.pos.z && z < node.pos.z+w) || x<0 || z<0 || x>=grid.width || z>=grid.depth) {
					continue;
				}
				
				//Add the node to the array -- //staticNodes[node.pos.y][x,z].walkable -- We don't check for it to be walkable anymore, this is done in the ScanQuadtree function
				if (!nodes.Contains (staticNodes[node.pos.y][x,z])) {
					nodes.Add (staticNodes[node.pos.y][x,z]);
				}
			}
		}
		return nodes.ToArray (typeof (Node)) as Node[];
	}
	
	public int IsSingleArea (Node node,int w, bool adjustY) {
		Int3 pos = node.pos;
		bool allWalkable = true;
		bool allUnwalkable = true;
		float avg = 0;
		int c = 0;
		for (int x=pos.x;x<pos.x+w;x++) {
			for (int z=pos.z;z<pos.z+w;z++) {
				if (staticNodes[pos.y][x,z].walkable) {
					allUnwalkable = false;
				} else {
					allWalkable = false;
				}
				avg += staticNodes[pos.y][x,z].vectorPos.y;
				c++;
				if (!allWalkable && !allUnwalkable && !adjustY) {
					return 2;
				}
			}
		}
		
		if (adjustY) {
			node.vectorPos.y = avg/(float)c;
		}
		
		if (!allWalkable && !allUnwalkable) {
			return 2;
		}
		 
		if (allWalkable) {
			return 1;
		}
		return 0;
	}
	
	public void FillWithNode (Node node, Int3 pos,int w) {
		Grid grid = grids[pos.y];
		for (int x=pos.x;x<pos.x+w;x++) {
			for (int z=pos.z;z<pos.z+w;z++) {
				if (x >= grid.width || z >= grid.depth) {
					Debug.LogError ("Out of range!! X: "+x+" Z: "+z+" Pos: "+pos+" Width: "+w);
				}
				staticNodes[pos.y][x,z] = node;
			}
		}
	}
	
	//-------- END QUADTREE ---------->
	
	Transform[] GetChildren (Transform parent) {
		Transform[] childs = new Transform[parent.childCount];
		int i = 0;
		foreach (Transform child in parent) {
			childs[i] = child;
			i++;
		}
		return childs;
	}
	
	public void CreateGrid (SimpleNode[] nodes) {
		SimpleNode[][] nds = new SimpleNode[1][];
		nds[0] = nodes;
		CreateGrid (nds);
	}
	
	
	public void CreateGrid (SimpleNode[][] nodes) {
		if (nodes.Length < 1) {
			Debug.LogError ("Make sure you use at least one grid");
		}
		
		
			
		Grid[] allGrids = new Grid[nodes.Length];
		for (int i=0;i<allGrids.Length;i++) {
			allGrids[i] = new Grid ();
			allGrids[i].width = nodes[i].Length;
			allGrids[i].depth = 1;
			
			if (allGrids[i].width < 1) {
				Debug.LogError ("Make sure you use at least one node for each grid");
				return;
			}
		}
		
		staticNodes = new Node[allGrids.Length][,];
		totalNodeAmount = 0;
		
		for (int i=0;i<allGrids.Length;i++) {
			Grid grid = allGrids[i];
			staticNodes[i] = new Node [grid.width,grid.depth];
			totalNodeAmount+= grid.width*grid.depth;
		}
		
		for (int y=0;y<allGrids.Length;y++) {
			
			SimpleNode[] gridNodes = nodes[y];
			
			for (int x=0;x<gridNodes.Length;x++) {
				Node node = new Node ();
				node.pos = new Int3(x,y,0);
				gridNodes[x].pos = node.pos;
				node.vectorPos = gridNodes[x].vectorPos;
				staticNodes[y][x,0] = node;
			}
		}
		
		for (int y=0;y<allGrids.Length;y++) {
			Grid grid = allGrids[y];
			
			SimpleNode[] gridNodes = nodes[y];
			
			for (int x=0;x<gridNodes.Length;x++) {
				Node node = staticNodes[y][x,0];
				
				node.connections = new Connection[gridNodes[x].neighbours.Length];
				
				for (int i=0;i<node.connections.Length;i++) {
					Node neighbour = GetNode (gridNodes[x].neighbours[i].pos);
					int cost = gridNodes[x].costs == null ? (int)(Vector3.Distance (neighbour.vectorPos,node.vectorPos)*100) : gridNodes[x].costs[i];
					float angle = gridNodes[x].angles == null ? 0 : gridNodes[x].angles[i];
					
					node.connections[i] = new Connection (neighbour,cost,angle,true);
				}
				
				node.GenerateEnabledConnections ();
			}
			
			
			Bounds b = new Bounds (staticNodes[y][0,0].vectorPos,Vector3.zero);
			for (int x=0;x<gridNodes.Length;x++) {
				b.Encapsulate (staticNodes[y][x,0].vectorPos);
			}
			
			b.extents += Vector3.one * boundsMargin;
			
			grid.globalWidth = b.size.x;
			grid.globalDepth = b.size.z;
			grid.globalHeight = b.size.y;
			
			grid.globalOffset = (b.center-b.extents);
		}
		grids = allGrids;
		FloodFillAll ();
		
		binaryHeap = new BinaryHeap (Mathf.CeilToInt (totalNodeAmount*heapSize));
	}
	
	//This function should be called before the script generates the connections between the nodes, but after the point where the script calculates if the nodes are walkable or not
	//This function applies the NodeDisabler and NodeEnabler links
	public void ApplyEnablerLinks () {
		for (int i=0;i<links.Length;i++) {
			NodeLink link = links[i];
			
			Int3 from = ToLocal (link.fromVector);
			Node fromNode = null;
			if (from != new Int3 (-1,-1,-1)) {
				fromNode = GetNode (from);
			} else {
				continue;
			}
			
			if (link.linkType == LinkType.NodeDisabler) {
				fromNode.walkable = false;
			} else if (link.linkType == LinkType.NodeEnabler) {
				fromNode.walkable = true;
			}
			
		}
	}
	
	//This function should be called after a navmesh has been generated, but before the FloodFill, it sets up all links to work properly
	//This function applies the Link link (including one-way links)
	public void ApplyLinks () {
		for (int i=0;i<links.Length;i++) {
			NodeLink link = links[i];
			
			//These types of links are not processed here
			if (link.linkType == LinkType.NodeDisabler || link.linkType == LinkType.NodeEnabler) {
				continue;
			}
			
			Int3 from = ToLocal (link.fromVector);
			Node fromNode = null;
			
			Int3 to = ToLocal (link.toVector);
			Node toNode = null;
			if (from != new Int3 (-1,-1,-1)) {
				fromNode = GetNode (from);
			} else {
				continue;
			}
			
			if (to != new Int3 (-1,-1,-1)) {
				toNode = GetNode (to);
			} else {
				continue;
			}
			
			if (!fromNode.walkable || !toNode.walkable) {
				continue;
			}
			
			if (link.linkType == LinkType.Link) {
				
				int cost = 0;
				if (gridGenerator == GridGenerator.Texture || gridGenerator == GridGenerator.Grid) {
					float nodeSize = (grids[fromNode.pos.y].nodeSize+grids[toNode.pos.y].nodeSize)/2.0F;
					cost = (int)((Vector3.Distance (fromNode.vectorPos,toNode.vectorPos)/nodeSize)*10);
				} else {
					cost = (int)(Vector3.Distance (fromNode.vectorPos,toNode.vectorPos)*100);
				}
				
				Vector3 dir = toNode.vectorPos-fromNode.vectorPos;
				Vector3 dir2 = dir;
				dir2.y = 0;
				float angle = Vector3.Angle (dir,dir2);
				
				fromNode.AddConnection (new Connection(toNode,cost,angle,true));
				
				if (!link.oneWay) {
					toNode.AddConnection (new Connection(fromNode,cost,angle,true));
				}
			}
			
			toNode.GenerateEnabledConnections ();
			fromNode.GenerateEnabledConnections ();
		}
	}
	
	public void GenerateNavmesh (Vector3[] points) {
		Bounds bounds = new Bounds ();
		for (int i=0;i<points.Length;i++) {
			bounds.Encapsulate (points[i]);
		}
		
		bounds.extents += Vector3.one * boundsMargin;
		
		Grid grid = new Grid (20);
		grids = new Grid[1] {grid};
		grid.width = points.Length;
		grid.depth = 1;
		grid.globalWidth = Mathf.CeilToInt (bounds.size.x);
		grid.globalDepth = Mathf.CeilToInt (bounds.size.z);
		grid.globalHeight = Mathf.CeilToInt (bounds.size.y);
		grid.nodeSize = 1;
		grid.offset = bounds.center-bounds.extents;
		//grid.nodeSize = 1;
		
		staticNodes = new Node[1][,];
		staticNodes[0] = new Node [grid.width,grid.depth];
		
		totalNodeAmount = grid.depth * grid.width;
		//for (int z=0;z<grid.depth;z++) {//Depth
		for (int x=0;x<grid.width;x++) {//Width
			Node node = new Node ();
			node.pos = new Int3 (x,0,0);
			node.vectorPos = points[x];
			
			staticNodes[0][x,0] = node;
		}
		
		for (int x=0;x<grid.width;x++) {//Width
			Node node = staticNodes[0][x,0];
			for (int i=0;i<grid.width;i++) {//Width
				Node otherNode = staticNodes[0][i,0];
				if (otherNode.vectorPos == node.vectorPos && otherNode != node) {
					//Debug.LogWarning ("Similar "+x+" "+i);
				}
			}
		}
		
		ApplyEnablerLinks ();
		
		//Loop through all nodes
		for (int x=0;x<grid.width;x++) {//Width
			Node node = staticNodes[0][x,0];
			
			if (!node.walkable) {
				node.connections = new Connection[0];
				node.GenerateEnabledConnections ();
				continue;
			}
			
			ArrayList connections = new ArrayList ();
			
			//Loop through all nodes and see which of them qualifies for being neighbours
			for (int i=0;i<grid.width;i++) {//Width
				Node otherNode = staticNodes[0][i,0];
				
				//Limit nodes from being at the same position and from being to far away from each other on the Y axis
				if (otherNode.vectorPos == node.vectorPos || Mathf.Abs (node.vectorPos.y-otherNode.vectorPos.y) > yLimit || !otherNode.walkable) {
					continue;
				}
				if (otherNode == node) {
					continue;
				}
				
				RaycastHit hit;
				if (Physics.Linecast (node.vectorPos,otherNode.vectorPos,out hit,boundsRayHitMask) || Physics.Linecast (otherNode.vectorPos,node.vectorPos,out hit,boundsRayHitMask)) {
					continue;
				} else {
					float dist = (node.vectorPos-otherNode.vectorPos).sqrMagnitude;
					if (dist <= neighbourDistanceLimit*neighbourDistanceLimit) {
						
						//This node qualifies for being a neighbour, so now we create a connection to it
						int cost = Mathf.RoundToInt (Mathf.Sqrt(dist)*100);
						Vector3 dir = otherNode.vectorPos-node.vectorPos;
						Vector3 dir2 = dir;
						dir2.y = 0;
						float angle = Vector3.Angle (dir,dir2);
						connections.Add (new Connection(otherNode,cost,angle,true));
					}
				}
			}
			
			node.connections = connections.ToArray (typeof(Connection)) as Connection[];
			
			node.GenerateEnabledConnections ();
		}
		
		ApplyLinks ();
		FloodFillAll ();
		
	}
	
	public static Vector3[] BoundPoints (Bounds b) {
		
		Vector3[] points = new Vector3[4];
		
		points[0] = new Vector3 (b.extents.x,0,b.extents.z) + b.center;
		
		points[1] = new Vector3 (-b.extents.x,0,-b.extents.z) + b.center;
		
		points[2] = new Vector3 (b.extents.x,0,-b.extents.z) + b.center;
		points[2] = new Vector3 (-b.extents.x,0,b.extents.z) + b.center;
		
		return points;
	}
	
	//------------ Saving & Loading ----- AstarData
	
	public void SaveAstarData () {
		if (astarData != null) {
			//astarData.staticNodes = new Node[3] {path.staticNodes[0][0,0],path.staticNodes[0][1,0],path.staticNodes[0][2,0]};
			astarData.staticNodes = new SerializedNode[staticNodes[0].Length];
			int width = staticNodes[0].GetLength (0);
			for (int x=0;x<staticNodes[0].GetLength (0);x++) {
				for (int z=0;z<staticNodes[0].GetLength (1);z++) {
					Node node = staticNodes[0][x,z];
					astarData.staticNodes[z*width+x] = new SerializedNode (node);
				}
			}
			astarData.grid = grids[0];
			//AssetDatabase.AddObjectToAsset (h,astarData);
			Debug.Log (astarData.staticNodes.Length + " "+astarData.staticNodes.Length);
		} else {
			Debug.LogWarning ("No AstarData to save to");
		}
	}
	
	public void LoadAstarData () {
		if (astarData != null) {
			Debug.Log ("Loading...");
			Debug.Log ((astarData.staticNodes != null ? ""+astarData.staticNodes.Length/* + (astarData.staticNodes.Length > 0 ? astarData.staticNodes[0].Length+"" : " Length Is Zero ")*/ : "Static Nodes Is Null "));
			staticNodes = new Node[1][,];
			staticNodes[0] = new Node[astarData.grid.width,astarData.grid.depth];
			grids = new Grid[1] {astarData.grid};
			meshGrid = grids[0];
			GenerateRotationMatrix (meshGrid);
			for (int x=0;x<astarData.grid.width;x++) {
				for (int z=0;z<astarData.grid.depth;z++) {
					Node node = astarData.staticNodes[z*astarData.grid.width+x];
					node.vectorPos.y += 2;
					node.pos = new Int3 (x,0,z);
					staticNodes[0][x,z] = node;
					//Debug.Log (node.pos.x);
				}
			}
			
			FloodFillAll ();
		} else {
			Debug.LogWarning ("No AstarData to load from");
		}
	}
	
	//Coroutines can't be called in editor scripts so I have to place it here
	public void SendBugReport (string email, string message) {
		StartCoroutine (SendBugReport2 (email,message));
	}
	
	public IEnumerator SendBugReport2 (string email, string message) {
		WWWForm form = new WWWForm();
		form.AddField("email",email);
		form.AddField("message",message);
		
		WWW w = new WWW("http://arongranberg.com/wp-content/uploads/astarpathfinding/bugreport.php", form);
		yield return w;
		if (w.error != null) {
			Debug.LogError ("Error: "+w.error);
		} else {
			Debug.Log ("Bug report sent");
		}
	}
	
			
	
	//Binary Heap
	
	public class BinaryHeap { 
		private Node[] binaryHeap; 
		public int numberOfItems; 

		public BinaryHeap( int numberOfElements ) { 
			binaryHeap = new Node[numberOfElements]; 
			numberOfItems = 2;
		} 
		
		public void Add(Node node) {
			if (this.numberOfItems == this.binaryHeap.Length) {
				numberOfItems--;
			}
			
			this.binaryHeap[this.numberOfItems] = node;
			
			int bubbleIndex = this.numberOfItems;
			while (bubbleIndex != 1) {
				int parentIndex = bubbleIndex / 2;
				if (this.binaryHeap[bubbleIndex].f <= this.binaryHeap[parentIndex].f) {
					Node tmpValue = this.binaryHeap[parentIndex];
					this.binaryHeap[parentIndex] = this.binaryHeap[bubbleIndex];
					this.binaryHeap[bubbleIndex] = tmpValue;
					bubbleIndex = parentIndex;
				} else {
					break;
				}
			}						 
			this.numberOfItems++;
		}
		
		public Node Remove() {
			this.numberOfItems--;
			Node returnItem = this.binaryHeap[1];
		 	
			this.binaryHeap[1] = this.binaryHeap[this.numberOfItems];
		 
			int swapItem = 1, parent = 1;
			do {
				parent = swapItem;
				if ((2 * parent + 1) <= this.numberOfItems) {
					// Both children exist
					if (this.binaryHeap[parent].f >= this.binaryHeap[2 * parent].f) {
						swapItem = 2 * parent;
					}
					if (this.binaryHeap[swapItem].f >= this.binaryHeap[2 * parent + 1].f) {
						swapItem = 2 * parent + 1;
					}
				} else if ((2 * parent) <= this.numberOfItems) {
					// Only one child exists
					if (this.binaryHeap[parent].f >= this.binaryHeap[2 * parent].f) {
						swapItem = 2 * parent;
					}
				}
				// One if the parent's children are smaller or equal, swap them
				if (parent != swapItem) {
					Node tmpIndex = this.binaryHeap[parent];
					this.binaryHeap[parent] = this.binaryHeap[swapItem];
					this.binaryHeap[swapItem] = tmpIndex;
				}
			} while (parent != swapItem);
			
			return returnItem;
		}
	}
	
	
	/*public class BinaryHeap { 
		public Node[] binaryHeap; 
		public int numberOfItems; 

		public BinaryHeap( int numberOfElements ) { 
			binaryHeap = new Node[numberOfElements]; 
			numberOfItems = 1;
		} 
		
		public void Add(Node node) {
			this.binaryHeap[this.numberOfItems] = node;
			
			int bubbleIndex = this.numberOfItems;
			while (bubbleIndex != 1) {
				int parentIndex = bubbleIndex / 2;
				if (this.binaryHeap[bubbleIndex].h+this.binaryHeap[bubbleIndex].g <= this.binaryHeap[parentIndex].h+this.binaryHeap[parentIndex].g) {
					Node tmpValue = this.binaryHeap[parentIndex];
					this.binaryHeap[parentIndex] = this.binaryHeap[bubbleIndex];
					this.binaryHeap[bubbleIndex] = tmpValue;
					bubbleIndex = parentIndex;
				} else {
					break;
				}
			}						 
			this.numberOfItems++;
		}
		
		public Node Remove() {
			this.numberOfItems--;
			Node returnItem = this.binaryHeap[1];
		 	
			this.binaryHeap[1] = this.binaryHeap[this.numberOfItems];
		 
			int swapItem = 1, parent = 1;
			do {
				parent = swapItem;
				if ((2 * parent + 1) <= this.numberOfItems) {
					// Both children exist
					if (this.binaryHeap[parent].h+this.binaryHeap[parent].g >= this.binaryHeap[2 * parent].g+this.binaryHeap[2 * parent].h) {
						swapItem = 2 * parent;
					}
					if (this.binaryHeap[swapItem].h+this.binaryHeap[swapItem].g >= this.binaryHeap[2 * parent + 1].h+this.binaryHeap[2 * parent + 1].g) {
						swapItem = 2 * parent + 1;
					}
				} else if ((2 * parent) <= this.numberOfItems) {
					// Only one child exists
					if (this.binaryHeap[parent].h+this.binaryHeap[parent].g >= this.binaryHeap[2 * parent].h+this.binaryHeap[2 * parent].g) {
						swapItem = 2 * parent;
					}
				}
				// One if the parent's children are smaller or equal, swap them
				if (parent != swapItem) {
					Node tmpIndex = this.binaryHeap[parent];
					this.binaryHeap[parent] = this.binaryHeap[swapItem];
					this.binaryHeap[swapItem] = tmpIndex;
				}
			} while (parent != swapItem);
			return returnItem;
		}
	}*/
}

//    For documentation see http://www.arongranberg.com/unity/a-pathfinding/docs/

//    © Copyright 2009 Aron Granberg
//    AstarPath.cs script is licenced under a Creative Commons Attribution-Noncommercial 3.0 Unported License.
//    If you want to use the script in commercial projects, please contact me at aron.g@me.com