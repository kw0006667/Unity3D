//    © Copyright 2009 Aron Granberg
//    AstarClasses.cs script is licenced under a Creative Commons Attribution-Noncommercial 3.0 Unported License.
//    If you want to use the script in commercial projects, please contact me at aron.g@me.com

using UnityEngine;
using System.Collections;
using System;
using AstarProcess;

//using System.Reflection;
namespace AstarClasses {
	
	[System.Serializable]
	public class NodeLink : System.Object {
		public Vector3 fromVector;
		public Vector3 toVector;
		public LinkType linkType= LinkType.Link;
		public bool oneWay = false;
		public NodeLink () {
		}
	}
	
	[System.Serializable]
	public class Grid : System.Object {
		public string name = "New Grid";
		public bool showInEditor = true;
		public bool debug = true;
		public bool changed = false;
		//public int startPos = 0;
		
		public float _height = 10;
		public int _width = 10;
		public int _depth = 10;
		
		public float scale = 1;
		
		public float height {
			get {
				return _height;
			}
			set {
				_height = value;
				globalHeight = value;
			}
		}
		
		public int width {
			get {
				return _width;
			}
			set {
				_width = value;
				globalWidth = value;
			}
		}
		
		public int depth {
			get {
				return _depth;
			}
			set {
				_depth = value;
				globalDepth = value;
			}
		}
		
		//public int width = 100;
		//public int depth = 100;
		
		public Vector3 realOffset {
			get {
				return offset+globalOffset;
			}
		}
		
		public Vector3 offset = Vector3.zero;
		
		//This is the offset which some grid generators will use to calibrate the position of the bounds without changing the user set offset
		public Vector3 globalOffset = Vector3.zero;
		
		//The size of each node, in Mesh mode this is the scale
		public float nodeSize = 10;
		
		//This is the bounds of the grid in world units, it may differ from the width/depth multiplied with nodeSize.
		public float globalWidth = 100;
		public float globalDepth = 100;
		public float globalHeight = 50;
		
		//Show the physics settings?
		public bool showPhysics = false;
		
		public int ignoreLayer;
		public LayerMask physicsMask = -1;
		public PhysicsType physicsType = PhysicsType.TouchCapsule;
		public UpDown raycastUpDown = UpDown.Down;
		public float raycastLength = 1000;
		public float capsuleHeight = 20;
		public float physicsRadius = 1.0F;
		
		
		public Grid (float h) {
			height = h;
			width = 100;
			depth = 100;
			globalWidth = 100;
			globalDepth = 100;
			globalHeight = h;
		}
		
		public Grid () {
			height = 10;
			width = 15;
			depth = 15;
			globalWidth = 15;
			globalDepth = 15;
			globalHeight = 10;
			nodeSize = 1;
			offset = Vector3.zero;
			globalOffset = offset;
		}
		
		public Grid (Grid o) {
			height = o.height;
			width = o.width;
			depth = o.depth;
			offset = o.offset;
			nodeSize = o.nodeSize;
		}
		
		public bool Contains (Int3 p) {
			if (
			p.x >= realOffset.x && p.z >= realOffset.z && 
			p.x < realOffset.x+(globalWidth)*nodeSize && 
			p.z < realOffset.z+(globalDepth)*nodeSize && 
			p.y >= realOffset.y-0.0001F && 
			p.y < realOffset.y+globalHeight+0.0001F) {
				return true;
			}
			return false;
		}
		
		public bool Contains (Vector3 p) {
			if (
			p.x >= realOffset.x && 
			p.z >= realOffset.z && 
			p.x < realOffset.x+(globalWidth)*nodeSize && 
			p.z < realOffset.z+(globalDepth)*nodeSize && 
			p.y >= realOffset.y-0.0001F && 
			p.y < realOffset.y+globalHeight+0.0001F) {
				return true;
			}
			return false;
		}
		
		public void Reset () {
			globalOffset = Vector3.zero;
		}
	}
	
	//This is the node class you should use when you are creating procedural levels.
	public class SimpleNode {
		//The position of the node [Needed]
		public Vector3 vectorPos;
		
		//The angles to the neighbour nodes, the length must be equal to the neighbours array OR the variable should be set to null, then the CreateGrid function will fill in the data
		public float[] angles;
		
		//The connections to other nodes [Needed]
		public SimpleNode[] neighbours;
		
		//The cost of moving from this node to a neighbour node, the length must be equal to the neighbours array OR the variable should be set to null, then the CreateGrid function will fill in the data
		public int[] costs = null;
		
		//You don't need to change this, the CreateGrid function will use it for caching data
		public Int3 pos;
		
		public SimpleNode () {
		}
		
		//Typically you would first create all nodes in the first pass using this constructor, and then assign all connections in a second pass
		public SimpleNode (Vector3 pos) {
			vectorPos = pos;
		}
		
		public SimpleNode (Vector3 pos,float[] an,SimpleNode[] ne,int[] co) {
			vectorPos = pos;
			angles = an;
			neighbours = ne;
			costs = co;
			if (neighbours.Length != costs.Length) {
				Debug.LogError ("Neighbours and Costs arrays length's must be equal");
			}
		}
	}
	
	//Each point in the grid is represented by a Node, each node holds information about the nodes position, parent, neighbour nodes and a lot of other stuff
	
	[System.Serializable]
	public class DataHolder : UnityEngine.Object{
		public Node[][,] staticNodes;
		
		public DataHolder () {
		}
	}
	
	//For nodes which's vectorPos is placed in the center of the node (usually a polygonal shape
	[System.Serializable]
	public class MeshCenterNode : MeshEdgeNode {
		public Edge[] neighbourEdges;
	}
	
	//For nodes which's vectorPos is placed on one edge
	[System.Serializable]
	public class MeshEdgeNode : Node {
		public Vector2[] area2D;
		public override Vector2[] GetArea () {
			return area2D;
		}
		
	}
	
	[System.Serializable]
	public class SerializedNode : System.Object {
		
		//Global position of the node
		public Vector3 vectorPos;
		
		//Local position of the node
		public Int3 pos;
		
		public bool walkable;
		
		public int penalty;
		
		public int area;
		
		
		public Int3[] serializedConnections;
		public bool[] serilizedConnectionsEnabled;
		
		
		public SerializedNode (Node node) {
			
			vectorPos = node.vectorPos;
			pos = node.pos;
			walkable = node.walkable;
			penalty = node.penalty;
			area = node.area;
			
			
		}
		
		public static implicit operator Node (SerializedNode node) {
			Node o = new Node ();
			
			o.vectorPos = node.vectorPos;
			o.pos = node.pos;
			o.walkable = node.walkable;
			o.penalty = node.penalty;
			o.area = node.area;
			return o;
		}
	}
	
	[System.Serializable]
	public class Node : System.Object{
		//Global position of the node
		public Vector3 vectorPos;
		
		//Local position of the node
		public Int3 pos;
		//The previous parent
		//public Node parentx;
		//Current parent
		public Node parent;
		
		//Is the node walkable
		public bool walkable = true;
		
		//These variables used by the grid to indicate the cost of moving from this node's parent to this node, the basicCost is just the constant cost, and the extraCost is used for the angle cost
		public int basicCost = 0;
		//public int extraCost = 0;
		
		//Cached G and H values
		//public int _g = 0;
		//public int hx = -1;
		
		public int h = 0;
		public int g = 0;
			
		public int penalty = 0;
		
		//This is the angles to each neighbour divided by 90
		//public float[] angles = new float[9];
		
		public Edge[] edges;
		public Edge edge;
		
		//All neighbours
		/*public Node[] neighbours;
		
		/*The neighboursKeys array is an array of the direction numbers for all walkable neighbours
		Imagine a node with eight nodes around it, the numbers in this array represents the direction from the middle node (this node) to the other nodes (neighbours) with 0 as Top Left and 7 as Bottom Right
		
		0 1 2
		3 X 4
		5 6 7
		
		The numbers are also used as indexes for the cost and angles array, so the neighbour at the Right would use the index 4 in the angles array.
		However they are only used as direction in the Grid and Texture modes, for all other modes this array will only be a linear set of numbers (i.e 1,2,3,4,5...) since there are no predifined directions for those modes. *
		public int[] neighboursKeys;*/
		
		//The area the node in is, all other nodes in the same area is accesible from this node, this is used to prevent the script from searching all possible nodes when there isn't a path availible to the target
		public int area = 0;
		
		public Connection[] connections;
		
		public Connection[] enabledConnections;
		
		//When whas the area calculated
		public int areaTimeStamp = 0;
		
		//Previous scripts
		//public AstarPath.Path scripty;
		
		//public int[] costs = null;
		
		//public AstarPath.Path scriptx;
		//Current
		public AstarPath.Path script;
		
		public float dia = 0;
		public int depth = 0;
		
		public virtual Vector2[] GetArea () {
			return null;
		}
		
		public void GenerateEnabledConnections () {
			Connection[] tmp = new Connection[connections.Length];
			int c =0;
			for (int i=0;i<connections.Length;i++) {
				if (connections[i].enabled) {
					tmp[c] = connections[i];
					c++;
				}
			}
			
			enabledConnections = new Connection[c];
			for (int i=0;i<c;i++) {
				enabledConnections[i] = tmp[i];
			}
		}
		
		public void AddConnection (Connection c) {
			Connection[] tmp = new Connection[connections.Length+1];
			for (int i=0;i<connections.Length;i++) {
				tmp[i] = connections[i];
			}
			tmp[connections.Length] = c;
			connections = tmp;
			GenerateEnabledConnections ();
		}

		
		public virtual void Open (AstarPath.BinaryHeap open, AstarPath.Path p, Node start, Node end, float angleCost) {
			
			for (int i=0;i<enabledConnections.Length;i++) {
				
				Connection connection = enabledConnections[i];
				Node node = connection.endNode;
				
				if (node == start) {
					continue;
				}
				
				//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.red); //Uncomment for debug
				//If the nodes script variable isn't refering to this path class the node counts as "not used yet" and can then be used
				if (node.script != p) {
					//Test if the angle from the current node to this one has exceded the angle limit
					//if (angle >= maxAngle) {
					//	return;
					//}
					node.parent = this;
					node.script = p;
					
					node.basicCost = connection.cost + Mathf.RoundToInt (connection.cost*connection.angle*angleCost);
					//(current.costs == null || costs.Length == 0 ? costs[node.invParentDirection] : current.costs[node.invParentDirection]);
					//Calculate the extra cost of moving in a slope
					//node.extraCost =  ;
					//Add the node to the open array
					//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.green); //Uncomment for @Debug
					
					node.UpdateH (end);
					node.UpdateG ();
					
					open.Add (node);
					
				} else {
					
					//If not we can test if the path from the current node to this one is a better one then the one already used
					int cost2 = connection.cost + Mathf.RoundToInt (connection.cost*connection.angle*angleCost);//(current.costs == null || current.costs.Length == 0 ? costs[current.neighboursKeys[i]] : current.costs[current.neighboursKeys[i]]);
					
					//int extraCost2
					
					if (g+cost2+node.penalty < node.g) {
						node.basicCost = cost2;
						//node.extraCost = extraCost2;
						node.parent = this;
						
						node.UpdateAllG ();
						
						open.Add (node);//@Quality, uncomment for better quality (I think).
						
						//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.cyan); //Uncomment for @Debug
					}
					
					 else if (node.g+cost2+penalty < g) {//Or if the path from this node ("node") to the current ("current") is better
						bool contains = false;
						
						//Make sure we don't travel along the wrong direction of a one way link now, make sure the Current node can be accesed from the Node.
						for (int y=0;y<node.connections.Length;y++) {
							if (node.connections[y].endNode == this) {
								contains = true;
								break;
							}
						}
						
						if (!contains) {
							continue;
						}
						
						parent = node;
						basicCost = cost2;
						//extraCost = extraCost2;
						
						node.UpdateAllG ();
						
						//Debug.DrawLine (current.vectorPos,current.neighbours[i].vectorPos,Color.blue); //Uncomment for @Debug
						open.Add (this);
					}
				}
			}
		}
		
		public void UpdateG () {
			g = parent.g+basicCost+penalty;
		}
		
		public void UpdateAllG () {
			g = parent.g+basicCost+penalty;
			
			foreach (Connection conn in enabledConnections) {
				if (conn.endNode.parent == this) {
					conn.endNode.UpdateAllG (g);
				}
			}
		}
		
		public void UpdateAllG (int parentG) {
			g = parentG+basicCost+penalty;
			
			foreach (Connection conn in enabledConnections) {
				if (conn.endNode.parent == this) {
					conn.endNode.UpdateAllG (g);
				}
			}
		}
		
		public int gScore {
			get {
				//Does this node have a parent?
				if (parent == null) {
					return 0;
				}
				
				return basicCost+penalty+parent.gScore;
			}
		}
		
		/*public int g {
			get {
				if (parent == null) {//Does this node have a parent?
					return 0;
				}
				
				//AstarPath.gCalls++;
				
				//@Performance, uncomment the next IF (and 'parentx = parent') to get better performance but at a cost of lower accuracy
				
				if (parent == parentx) {
					return _g;
				} else {
					parentx = parent;
					
					//Trace back to the starting point
					_g = basicCost+extraCost+penalty+parent.g;
				}
				
				return _g;
			}
		}*/
		
		public void UpdateH (Node end) {
			//If useWorldPositions is True, then the script will calculate the distance between this node and the target using world positions, otherwise it will use the array indexes (grids are made up of 2D arrays), which is a lot faster since it only uses integers instead of floats. It is recommended to use world positions when not using the Grid or Texture mode since all other modes does not place the nodes in an array which indexes can be used as positions.
			//It can also be good to use world positions when using multiple grids since the array indexes wont create a good direction when this node and the target node are in different grids.
					
			if (AstarPath.active.useWorldPositions) {
				
				h = (int) (Mathf.Abs(end.vectorPos.x-vectorPos.x)*10
			
				+  Mathf.Abs(end.vectorPos.y-vectorPos.y)*10
			
				+ Mathf.Abs(end.vectorPos.z-vectorPos.z)*10);
				
			} else {
				
				h = Mathf.Abs(end.pos.x-pos.x)*10
			
				//@Performance, comment out the next line if you dont use multiple grids
				+ Mathf.Abs((int)AstarPath.active.grids[end.pos.y].offset.y-(int)AstarPath.active.grids[pos.y].offset.y)*AstarPath.active.levelCost
			
				+ Mathf.Abs(end.pos.z-pos.z)*10;
				
				//This is another heuristic, try it if you want
				/*int xDistance = Mathf.Abs(script.end.pos.x-pos.x);
				int zDistance = Mathf.Abs(script.end.pos.z-pos.z);
				if (xDistance > zDistance) {
				     hx = 14*zDistance + 10*(xDistance-zDistance);
				} else {
				     hx = 14*xDistance + 10*(zDistance-xDistance);
				}*/
			}
		}
		
		/*public int h {
			get {
				
				//Has the end changed since last time? Acctually it is checking if the path calculating right now is the same as the one when the H value was calculated last time. If it is then we can just return the cached value since the end isn't going to change during one path calculation
				if (script == scripty) {
					return hx;
				} else {
					scripty = script;
					
					
					
				}
				
				return hx;
			}
		}*/
		
		//The script will follow the nodes which has the lowest F scores, read a bit about A* to get more info about the F score.
		public int f {
			get {
				//@Performance, choose the option you want, but hardcoded is faster. This switch statement could get executed several thousand times in one frame!
				switch (AstarPath.active.formula) {
					case Formula.HG:
						//This is the typical A* formula
						return h+g;
					case Formula.H:
						//The formula which only uses F is called Best First Search (I think)
						return h;
						
					case Formula.G:
						//The formula which only uses G is called Dijkstra's algorithm
						return g;
					default:
						return h+g;
				}
			}
		}
		
		public Node () {
			walkable = true;
		}
		public Node (Node o) {//Copy
			walkable = o.walkable;
			vectorPos = o.vectorPos;
			pos = o.pos;
		}
		
	}
	
	public struct Int3 {
		public int x;
		public int y;
		public int z;
		
		public static Int3 Null {
			get {
				return new Int3 (-1,-1,-1);
			}
		}
		
		public Int3 (int x,int y,int z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		public Int3 (float x2,float y2) {
			x = Mathf.RoundToInt (x2);
			y = 0;
			z = Mathf.RoundToInt (y2);
		}
		
		public Int3 (float x2,float y2,float z2) {
			x = Mathf.RoundToInt (x2);
			y = Mathf.RoundToInt (y2);
			z = Mathf.RoundToInt (z2);
		}
		
		public static Int3 operator + (Int3 lhs, Int3 rhs) {
      		return new Int3 (lhs.x+rhs.x,lhs.y+rhs.y,lhs.z+rhs.z);
  	  	}
  	  	public static Int3 operator - (Int3 lhs, Int3 rhs) {
      		return new Int3 (lhs.x-rhs.x,lhs.y-rhs.y,lhs.z-rhs.z);
  	  	}
  	  	
  	  	public static bool operator == (Int3 lhs, Int3 rhs) {
      		return lhs.x==rhs.x&&lhs.y==rhs.y&&lhs.z==rhs.z;
  	  	}
  	  	
  	  	public static bool operator != (Int3 lhs, Int3 rhs) {
      		return lhs.x!=rhs.x||lhs.y!=rhs.y||lhs.z!=rhs.z;
  	  	}
  	  	
		public static implicit operator Int3 (Vector3 i)
			{
			Int3 temp = new Int3 (i.x,i.y,i.z);
			return temp;
		}
  	  	
  	  	public static implicit operator Vector3 (Int3 i)
			{
			Vector3 temp = new Vector3 (i.x,i.y,i.z);
			return temp;
		}
		
		public static implicit operator Vector2 (Int3 i)
			{
			Vector2 temp = new Vector2 (i.x,i.z);
			return temp;
		}
		
		public override bool Equals(System.Object obj) {
  	  		if (obj == null) {
  	  			return false;
  	  		}
  	  		
  	  		Int3 rhs = (Int3)obj;
  	  		
  	  		return this.x==rhs.x&&this.y==rhs.y&&this.z==rhs.z;
  	  	}
  	  	
  	  	//Returns unique values up to 300*300 grids or 90000 lists (when x = 0,1,2,3,4,5... and z is always 0)
  	  	public override int GetHashCode() {
        	return (int)(y*90000+z*300+x);
		}
		
		public override string ToString () {
  	  		return "("+x+","+y+","+z+")";
  	  	}
  	  	
	}
	
	public class Connection {
		public Node startNode;
		public Node endNode;
			
		public float angle;
		public int cost;
		
		public bool enabled = false;
		
		public Connection twin;
		
		public Connection () {
		}
		
		public Connection (Node s,Node e) {
			startNode = s;
			endNode = e;
		}
		
		public Connection (Node e,int c,float a,bool en) {
			endNode = e;
			cost = c;
			angle = a;
			enabled = en;
		}
		
		/*public Connection (Node s,Node e,int c,float a,bool en) {
			startNode = s;
			endNode = e;
			cost = c;
			angle = a;
			enabled = en;
		}*/
		
		public static implicit operator Node (Connection i)
			{
			return i.endNode;
		}
	}
	
	public enum CoordinateSystem {
		LeftHanded,
		RightHanded
	}
	
	public enum LinkType {
		Link,
		NodeDisabler,
		NodeEnabler,
		Disabler
	}
	
	public enum MeshNodePosition {
		Edge,
		Center
	}
	
	public enum IsNeighbour {
		Eight,
		Four
	}
	
	public enum Formula {
		HG,
		H,
		G
	}
	
	public enum PhysicsType {
		OverlapSphere,
		TouchSphere,
		TouchCapsule,
		Raycast
	}
	
	public enum GridGenerator {
		Grid,
		Texture,
		Mesh,
		Bounds,
		List,
		Procedural
	}
	
	public enum UpDown {
		Up,
		Down
	}
	
	public enum DebugMode {
		Areas,
		Angles,
		H,
		G,
		F
	}
	
	public enum Height {
		Flat,
		Terrain,
		Raycast
	}
	
	public enum Simplify {
		None,
		Full,
		Simple
	}
	
}

//    © Copyright 2009 Aron Granberg
//    AstarClasses.cs script is licenced under a Creative Commons Attribution-Noncommercial 3.0 Unported License.
//    If you want to use the script in commercial projects, please contact me at aron.g@me.com