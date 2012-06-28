//    © Copyright 2010 Aron Granberg
//    AstarUser.cs script is licenced under a Creative Commons Attribution-Noncommercial 3.0 Unported License.
//    If you want to use the script in commercial projects, please contact me at aron.g@me.com
//    More info about the license can be found on the AstarPath homepage (see next line).

//For documentation see http://www.arongranberg.com/unity/a-pathfinding/docs/

using UnityEngine;
using System.Collections;
using AstarClasses;
using AstarMath;

/*This script is meant to provide a number of functions a user can have use of and not having to look in the deep core script AstarPath.cs
*/

public class AstarUser {

	//Returns the nearest node from a Vector3 point, this function can return null if the point is not contained inside any grids bounds.
	public Node GetNearestNode (Vector3 pos) {
		Int3 p = AstarPath.ToLocal (pos);
		if (p != new Int3(-1,-1,-1)) {
			return AstarPath.GetNode (p);
		} else {
			return null;
		}
	}
	
	//Returns the nearest node's position from a Vector3 point, this function can return null (Vector3.zero) if the point is not contained inside any grids bounds.
	public Vector3 GetNearestNodePosition (Vector3 pos) {
		Int3 p = AstarPath.ToLocal (pos);
		if (p != new Int3(-1,-1,-1)) {
			return AstarPath.GetNode (p).vectorPos;
		} else {
			return Vector3.zero;
		}
	}
	
	//Returns the nearest node's position from a Vector3 point, will never return null (Vector3.zero), except for when there are no calculated grids.
	public Vector3 GetNearestNodePositionUnclamped (Vector3 pos) {
		Int3 p = AstarPath.ToLocalUnclamped (pos);
		if (p != new Int3(-1,-1,-1)) {
			return AstarPath.GetNode (p).vectorPos;
		} else {
			return Vector3.zero;
		}
	}
	
	//This will return true if the supplied position is inside any of the grids in the scene, and false if it doesn't.
	public static bool IsPositionInsideAnyGrids (Vector3 pos) {
		for (int i=0;i<AstarPath.active.grids.Length;i++) {
			Grid grid = AstarPath.active.grids[i];
			if (grid != null && grid.Contains (pos)) {
				return true;
			}
		}
		return false;
	}
}
