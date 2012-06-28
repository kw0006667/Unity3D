using UnityEngine;
using System.Collections;
using AstarClasses;
using AstarMath;

public class AstarProcessing {
}

namespace AstarProcess {
	
	public class PostProcessSplines {
		
		public static Vector3[] CatmullRom (Vector3[] path,int subdivisions) {
		
			Vector3[] newPath = new Vector3[path.Length*(int)Mathf.Pow (2,subdivisions)];
			int c = 0;
			
			float step = 1.0F/Mathf.Pow (2,subdivisions);
			
			for (int i=0;i<path.Length;i++) {
				//Debug.DrawRay (nodes[i],Vector3.up*Random.value*3,new Color(Random.value,Random.value,Random.value,0.9F));
				for (float t=0.0F; t<1.0F; t+=step) {
					Vector3 previous = GetPoint (i-1,path);
					Vector3 start = GetPoint (i,path);
					Vector3 end = GetPoint (i+1,path);
					Vector3 next = GetPoint (i+2,path);
					
					//Uncomment for more corners in the smooth, but not as risky to use since it will not be a such big chance the generated path will overlap colliders
					/*float magn = (start-end).magnitude;
					next = end+(next-end).normalized*magn*0.5F;
					previous = start+(previous-start).normalized*magn*0.5F;*/
					
					Vector3 point = AstarSplines.CatmullRom (previous, start, end, next,t);
					//Gizmos.DrawLine (prePoint,point);
					newPath[c] = point;
					c++;
				}
			}
			
			return newPath;
		}
		
		public static Vector3 GetPoint (int i,Vector3[] path) {
			i = i > path.Length-1 ? path.Length-1 : i;
			i = i < 0 ? 0 : i;
			return path[i];
		}
	
		public static Vector3[] CubicSmooth (Vector3[] path, int subdivisions, bool normalizeTangents, float tangentLength) {
		
			if (path.Length < 3) {
				return path;
			}
			
			Vector3[] path2 = new Vector3[(path.Length-1)*(int)Mathf.Pow (2,subdivisions)+1];
			
			Vector3[] tangents = new Vector3[path.Length];
			
			tangents[0] = (path[1]-path[0]);
			if (normalizeTangents) { tangents[0].Normalize (); }
			
			tangents[tangents.Length-1] = (path[tangents.Length-1]-path[tangents.Length-2]);
			if (normalizeTangents) { tangents[tangents.Length-1].Normalize (); }
			
			float[] tangentLengths = new float[tangents.Length*2];
			
			for (int i=1;i<tangents.Length-1;i++) {
				Vector3 dir1 = path[i-1]-path[i];
				Vector3 dir2 = path[i+1]-path[i];
				
				Vector3 tangent = (dir2.normalized-dir1.normalized);
				
				if (normalizeTangents) {
					tangentLengths[i*2] = dir1.magnitude;
					tangentLengths[i*2+1] = dir2.magnitude;
				} else {
					tangentLengths[i*2] = 1;
					tangentLengths[i*2+1] = 1;
				}
				
				if (normalizeTangents) { tangent = tangent.normalized*tangentLength; }
				
				tangents[i] = tangent;
			}
			
			int c = 0;
			for (int p=0;p<path.Length-1;p++) {
				
				float step = 1.0F/Mathf.Pow (2,subdivisions);
				
				Vector3 tangent = tangents[p];
				Vector3 tangent2 = tangents[p+1];
				
				Vector3 p1 = path[p]+tangent*tangentLengths[p*2+1];
				Vector3 p2 = path[p+1]-tangent2*tangentLengths[(p+1)*2];
				
				for (float i=0;i<1.0F;i+= step) {
					path2[c] = CubicBezier (path[p],p1,p2,path[p+1],i);
					c++;
				}
				
			}
			
			path2[c] = path[path.Length-1];
			
			path = path2;
			
			return path;
			
			/*Gizmos.color = normalizeTangents ? Color.green : Color.yellow;
			for (int i=0;i<path.Length-1;i++) {
				Gizmos.DrawLine (path[i],path[i+1]);
			}*/
		}
	
	
		public static Vector3[] SimpleSmooth (Vector3[] path,int iterations, float strength, int subdivisions) {
			
			path = Subdivide (path,subdivisions);
			
			Vector3[] path2 = path;
			
			for (int it=0;it<iterations;it++) {
				
				for (int i=1;i<path2.Length-1;i++) {
					
					Vector3 avg = (path[i-1]+path[i+1])/2.0F;
					
					path2[i] = Vector3.Lerp (path[i],avg,strength);
				}
				
				path = path2;
			}
			
			/*for (int i=0;i<path.Length-1;i++) {
				Gizmos.color = Color.red;
				Gizmos.DrawLine (path[i],path[i+1]);
			}*/
			
			return path;
			
			/*Vector3[] path3 = new Vector3[path.Length];
			
			for (int i=1;i<path.Length-1;i++) {
				Vector3 dir1 = path[i-1]-path[i];
				Vector3 dir2 = path[i+1]-path[i];
				
				Vector3 normal = (dir1.normalized+dir2.normalized)/2.0F;
				
				if (normal == Vector3.zero) {
					continue;
				}
				
				normal *= -1;
				normal.Normalize ();
				normal*=offset;
				path3[i] = path[i]+normal;
				Gizmos.DrawRay (path[i],normal);
				Gizmos.color = Color.magenta;
			}
			
			path = path3;
			
			for (int i=0;i<path.Length-1;i++) {
				Gizmos.color = Color.blue;
				Gizmos.DrawLine (path[i],path[i+1]);
			}*/
		}
		
		public static Vector3[] Subdivide (Vector3[] path, int subdivisions) {
			
			Vector3[] path2 = new Vector3[(path.Length-1)*(int)Mathf.Pow (2,subdivisions)+1];
			
			int c = 0;
			for (int p=0;p<path.Length-1;p++) {
				float step = 1.0F/Mathf.Pow (2,subdivisions);
				
				for (float i=0;i<1.0F;i+=step) {
					path2[c] = Vector3.Lerp (path[p],path[p+1],Mathf.SmoothStep (0,1, i));
					c++;
				}
			}
			
			//Debug.Log (path2.Length +" "+c);
			
			path2[c] = path[path.Length-1];
			return path2;
		}
		
		public static Vector3 CubicBezier (Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3, float t) {
			t = Mathf.Clamp01 (t);
			float t2 = 1-t;
			return Mathf.Pow(t2,3) * p0 + 3 * Mathf.Pow(t2,2) * t * p1 + 3 * t2 * Mathf.Pow(t,2) * p2 + Mathf.Pow(t,3) * p3;
		}
	}
	
	//----------------------------------------------
	
	public class PostProcess {
		
		public static Color LoopingColor (int i) {
			while (i < 0) {
				i+=4;
			}
			while (i >= 4) {
				i-=4;
			}
			switch (i) {
				case 0:
					return Color.red;
				case 1:
					return Color.green;
				case 2:
					return Color.blue;
				case 3:
					return Color.yellow;
				default:
					return Color.red;
			}
		}
		
		/*static public Vector3[] NavigationMesh (Node[] path, bool nodesAreEdges, Vector3 realStart, Vector3 realEnd, float turningRadius) {
			ArrayList a = new ArrayList (path);
			a.Reverse ();
			path = a.ToArray (typeof(Node)) as Node[];
			
			ArrayList returnPath = new ArrayList ();
			ArrayList edgeArray = new ArrayList ();
			
			ArrayList directionChangeIndexes = new ArrayList ();
			
			int edgeCount = 3;
			
			if (nodesAreEdges) {
				edgeCount = 1;
			}
			
			Node currentStart = path[0];
			Node currentEnd = path[0];
			Node currentParent = path[0];
			
			if (currentStart.parent == null || currentStart.parent.parent == null) {
				return new Vector3[2] {Vector3.zero,Vector3.one};
			}
			currentEnd = currentStart.parent;
			currentParent = currentStart.parent;
			
			//Should we add the comming node to the path? Some nodes might get processed more than once and we don't want duplicates
			bool addToPath = true;
			
			ArrayList tmpEdgeArray = new ArrayList ();
			
			if (nodesAreEdges) {
				Edge startDirection = new Edge (realEnd,currentEnd.vectorPos);
				Edge.EdgeIntersection startIntersection = Edge.IntersectionTest (currentStart.edge,startDirection);
				
				//startDirection.DebugWarning ();
				//currentStart.edge.DebugError ();
				if (startIntersection.segementsIntersects || startIntersection.type == Edge.LineClassification.LinesIntersect) {
					edgeArray.Add (currentStart.edge);
					//currentStart.edge.Debug (Color.green);
				} else {
					//Debug.Log (startIntersection.type);
					//currentStart.edge.DebugWarning ();
					//startDirection.DebugWarning ();
				}
			}
			
			//Debug.Log ("Begin All");
			
			//Do some things only the first iteration
			bool isFirst = true;
			
			bool lastCheckSucceeded = true;
			
			while (true) {
				//Debug.Log ("Adding");
				
				if (addToPath) {
					returnPath.Add (currentStart);
					directionChangeIndexes.Add (edgeArray.Count);
				}
				
				if (currentStart.parent == null || currentStart.parent.parent == null) {
					
					if (currentStart.parent != null) {
						returnPath.Add (currentStart.parent);
					}
					
					break;
				}
				
				Node start = currentStart;
				Node end = currentEnd;
				
				Vector3 rayStart = isFirst ? realEnd : start.vectorPos;
				
				Node currentNode = start;
				Node preNode = null;
				
				int c = 0;
				
				ArrayList preTmpEdgeArray = tmpEdgeArray;
				
				tmpEdgeArray = new ArrayList ();
				
				while (currentNode != end) {
					c++;
					if (c>120) {
						break;
					}
					
					//Vector3 dir = (end.vectorPos-rayStart);
					Debug.DrawLine (end.vectorPos,rayStart,Color.blue);
					
					Edge targetEdge = new Edge (rayStart,end.vectorPos);
					Edge[] edges = currentNode.edges;
					edgeCount = edges.Length;
					
					Edge.EdgeIntersection[] intersections = new Edge.EdgeIntersection[edgeCount];
					Edge[] intersectEdges = new Edge[edgeCount];
					int counter = 0;
					
					for (int i=0;i<edgeCount;i++) {
						Edge.EdgeIntersection intersection = Edge.IntersectionTest (edges[i],targetEdge);
						//Debug.Log (intersection.type);
						if (intersection.segementsIntersects) {
							//intersectEdge = edges[i];
							intersections[counter] = intersection;
							intersectEdges[counter] = edges[i];
							//Debug.DrawLine (edges[i].endPointA,edges[i].endPointB,Color.cyan);
							counter++;
						} else {
							//Debug.DrawLine (edges[i].endPointA,edges[i].endPointB,Color.yellow);
						}
					}
					
					if (c == 0) {
						Debug.LogError ("System Fail, this shouldn't happen (a very weird mesh is used, probably containing a triangle with no area)!");
						
						for (int i=0;i<edgeCount;i++) {
							Edge.EdgeIntersection intersection2 = Edge.IntersectionTest (edges[i],targetEdge);
							Debug.Log (intersection2.type);
							Debug.DrawLine (edges[i].endPointA,edges[i].endPointB,Color.red);
						}
						break;
					}
					
					bool match = false;
					if (nodesAreEdges) {
						
						for (int i=0;i<currentNode.enabledConnections.Length;i++) {
							Node neighbour = currentNode.enabledConnections[i];
							
							if (neighbour == preNode) {
								continue;
							}
							
							if (neighbour == end) {
								preNode = currentNode;
								tmpEdgeArray.Add (neighbour.edge);
								//neighbour.edge.Debug (Color.cyan);
								currentNode = neighbour;
								match = true;
								break;
							}
							
							for (int x=0;x<counter;x++) {
								Edge intersectEdge = intersectEdges[x];
								
								if (neighbour.edge == intersectEdge) {
									Edge.EdgeIntersection intersection = intersections[x];
									rayStart = intersection.intersectionA;
									tmpEdgeArray.Add (intersectEdge);
									//neighbour.edge.Debug (Color.cyan);
									preNode = currentNode;
									currentNode = neighbour;
									match = true;
									
									//Add the first edge in the direction towards the end
									//if (edgeArray.Count == 0) {
									//	edgeArray.Add (intersectEdge);
									//}
									//Don't bother to check the other edges since we know which one is correct
									break;
								}
							}
							
							//We need to break out of the second loop too
							if (match) {
								break;
							}
						}
					} else {
						for (int i=0;i<currentNode.enabledConnections.Length;i++) {
							Node neighbour = currentNode.enabledConnections[i];
							
							if (neighbour == preNode) {
								continue;
							}
							
							for (int x=0;x<counter;x++) {
								Edge intersectEdge = intersectEdges[x];
								
								//Loop through the edges and see if this neighbour is the correct one (i.e one edge is the same as intersectEdge)
								for (int e=0;e<neighbour.edges.Length;e++) {
									if (neighbour.edges[e] == intersectEdge) {
										Edge.EdgeIntersection intersection = intersections[x];
										rayStart = intersection.intersectionA;
										tmpEdgeArray.Add (intersectEdge);
										preNode = currentNode;
										currentNode = neighbour;
										match = true;
										
										//Don't bother to check the other edges since we know which one is correct
										break;
									}
								}
								
								//Break out of the second loop
								if (match) {
									break;
								}
							}
							
							//We need to break out of the third loop too
							if (match) {
								break;
							}
						}
					}
					
					if (!match) {
						//@TODO, add the edge to the currentParent
						break;
					}
					
				}
				//Debug.Log ("End Trace");
				if (currentNode != end) {
					Debug.DrawLine (currentStart.vectorPos+Vector3.up*0.1F,end.vectorPos+Vector3.up*0.1F,Color.red);
					//Debug.DrawRay (currentNode.vectorPos,Vector3.up*2,Color.red);
					//Debug.DrawLine (rayStart,end.vectorPos,Color.red);
					
					currentStart = currentParent;
					
					if (edgeArray.Count == 0 && nodesAreEdges) {
						//edgeArray.Add (currentStart.edge);
					}
					
					//if (preTmpEdgeArray.Count == 0 && isFirst) {
					//	Debug.LogWarning ("I knew this was going to happen!");
					//}
					Debug.Log ("PreTmp "+preTmpEdgeArray.Count+" Tmp "+tmpEdgeArray.Count);
					if (lastCheckSucceeded) {
						for (int i=0;i<preTmpEdgeArray.Count;i++) {
							edgeArray.Add (preTmpEdgeArray[i]);
						
							(preTmpEdgeArray[i] as Edge).DebugError ();
						
							Edge e1 = preTmpEdgeArray[i] as Edge;
						//Debug.DrawLine (Vector3.up*0.1F+e1.endPointA,Vector3.up*0.1F+e1.endPointB,Color.green);
						}
					}
					
					/*if (tmpEdgeArray.Count > 0) {
						edgeArray.Add (tmpEdgeArray[0]);
					} else {
						Debug.LogError ("Wut? Length zero??");
					}*
					
					//Debug.DrawRay (currentStart.vectorPos,Vector3.up * 5,Color.green);
					
					if (currentParent.parent != null) {
						currentParent = currentStart.parent;
					}
					
					if (currentEnd.parent != null && currentEnd == currentStart) {
						currentEnd = currentEnd.parent;
					}
					
					addToPath = true;
					lastCheckSucceeded = false;
					
				} else {
					addToPath = false;
					lastCheckSucceeded = true;
					
					Debug.DrawLine (currentStart.vectorPos,end.vectorPos,Color.green);
					if (currentEnd.parent != null) {
						currentParent = currentEnd;
						currentEnd = currentEnd.parent;
						//for (int i=0;i<tmpEdgeArray.Count;i++) {
						//	edgeArray.Add (tmpEdgeArray[i]);
						//	Edge e1 = tmpEdgeArray[i] as Edge;
							//Debug.DrawLine (Vector3.up*1.2F+e1.endPointA,Vector3.up*1.2F+e1.endPointB,Color.red);
							
						//}
					} else {
						currentStart = currentEnd;
						addToPath = true;
						
						for (int i=0;i<preTmpEdgeArray.Count;i++) {
							edgeArray.Add (preTmpEdgeArray[i]);
							(preTmpEdgeArray[i] as Edge).DebugError ();
							Edge e1 = preTmpEdgeArray[i] as Edge;
							//Debug.DrawLine (Vector3.up*0.1F+e1.endPointA,Vector3.up*0.1F+e1.endPointB,Color.magenta);
						}
					}
				}
				isFirst = false;
				
			}
			Debug.Log ("End All");
			
			
			edgeArray.Add (tmpEdgeArray[tmpEdgeArray.Count-1]);
			(tmpEdgeArray[tmpEdgeArray.Count-1] as Edge).DebugError ();
			
			for (int i=0;i<returnPath.Count-1;i++) {
				Debug.DrawLine ((returnPath[i] as Node).vectorPos,(returnPath[i+1] as Node).vectorPos,Color.cyan);
			}
			Debug.Log ("Path Length Is : "+returnPath.Count);
			
			for (int i=0;i<edgeArray.Count;i++) {
				Edge e = edgeArray[i] as Edge;
				e.Debug (Color.Lerp (Color.blue,Color.red,(float)i/(float)(edgeArray.Count-1)));
				if (e.borderA) {
					//Debug.DrawRay (e.endPointA,Vector3.up,Color.red);
				}
				if (e.borderB) {
					//Debug.DrawRay (e.endPointB,Vector3.up,Color.red);
				}
			}
			Debug.Log (edgeArray.Count);
			
			//STEP 2
			
			Edge[] clampedEdges = new Edge[edgeArray.Count+1];
			
			bool dup = false;
			for (int i=0;i<edgeArray.Count;i++) {
				clampedEdges[i] = (edgeArray[i] as Edge).TurningRadius (turningRadius);
				for (int x=0;x<edgeArray.Count;x++) {
					if (x != i && (edgeArray[i] as Edge) == (edgeArray[x] as Edge)) {
						Debug.LogError ("Duplicate detected!");
						dup = true;
						clampedEdges[i].DebugError ();
					}
				}
			}
			
			if (dup) {
				
				return new Vector3[2] {Vector3.zero,Vector3.one};
			}
			
			clampedEdges[clampedEdges.Length-1] = new Edge (realStart,realStart);
			
			Vector3 startPoint = realEnd;
			int currentStartEdge = 0;
			int currentTargetEdge = 0;
			
			ArrayList pointPath = new ArrayList ();
			
			pointPath.Add (startPoint);
			
			int lastSeenLeft = -1;
			int lastSeenRight = -1;
			
			ArrayList lastSeenEdges = new ArrayList ();
			
			int count = 0;
			
			for (int i=0;i<clampedEdges.Length;i++) {
				Edge e = clampedEdges[i];
				
				
				Edge e1A = new Edge (startPoint,e.endPointA);
				Edge e1B = new Edge (startPoint,e.endPointB);
				e1A.Debug (Color.blue);
				e1B.Debug (Color.blue);
				
				bool doBreak = false;
				
				/*if (lastSeenLeft >= 0) {
					Debug.DrawRay (clampedEdges[lastSeenLeft].endPointA,Vector3.up*2,Color.magenta);
				}
				
				if (lastSeenRight >= 0) {
					Debug.DrawRay (clampedEdges[lastSeenRight].endPointB,Vector3.up*2,Color.cyan);
				}*
				
				if (i == clampedEdges.Length-1) {
					count++;
					if (count > 300) {
						break;
					}
					
					for (int x=i-1;x>=currentStartEdge;x--) {
						e.Debug (Color.red);
						Edge e2 = clampedEdges[x];
						
						Edge.OnLinePosition p1 = Edge.DoesLinesIntersectFast (e2,e1A);
						
						if (p1 == Edge.OnLinePosition.OnLine) {
							e2.Debug (Color.green);
							continue;
						} else {
							
							for (int y=lastSeenEdges.Count-1;y>=0;y--) {
								CanSeeEdge cSE2 = (lastSeenEdges[y] as CanSeeEdge);
								
								Debug.DrawRay (cSE2.edge.endPointA,Vector3.up,cSE2.canSeeA ? Color.green : Color.red);
								Debug.DrawRay (cSE2.edge.endPointB,Vector3.up,cSE2.canSeeB ? Color.green : Color.red);
								
								Edge.EdgeIntersection intersect = Edge.DoesLinesIntersect (cSE2.edge,e1A);
								
								//Mathfx.NearestPoint (e1A.endPointA,e1A.endPointB,cSE2.edge.endPointA);
									
								cSE2.nearestPoint1 = intersect.intersectionA;
								
								if (Vector3.Dot (cSE2.nearestPoint1-cSE2.edge.endPointA,cSE2.nearestPoint1-cSE2.edge.endPointB) <= 0) {
									Debug.DrawLine (cSE2.edge.endPointA,cSE2.edge.endPointB,Color.red);
									Debug.LogWarning (cSE2.pos1+"  "+cSE2.pos2);
									continue;
								}
								
								Debug.DrawLine (cSE2.edge.endPointA,cSE2.edge.endPointB,Color.white);
								
								
								Debug.DrawRay (cSE2.nearestPoint1,Vector3.up*2,Color.cyan);
								
								float dist1 = (cSE2.nearestPoint1-cSE2.edge.endPointA).sqrMagnitude;
								float dist2 = (cSE2.nearestPoint1-cSE2.edge.endPointB).sqrMagnitude;
								
								//Debug.DrawRay (cSE2.edge.endPointA,Vector3.up,cSE2.canSeeA ? Color.green : Color.red);
								//Debug.DrawRay (cSE2.edge.endPointB,Vector3.up,cSE2.canSeeB ? Color.green : Color.red);
								
								if (dist1 <= dist2 && cSE2.canSeeA) {
									Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointA,Color.green);
									Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointB,Color.red);
									
									startPoint = cSE2.edge.endPointA;
									currentStartEdge = cSE2.index;
									break;
								} else if (dist2 <= dist1 && cSE2.canSeeB) {
									Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointA,Color.red);
									Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointB,Color.green);
									startPoint = cSE2.edge.endPointB;
									currentStartEdge = cSE2.index;
									break;
								}
							}
							
							lastSeenEdges = new ArrayList ();
							//startPoint = p1 == Edge.OnLinePosition.Right ? e2.endPointB : e2.endPointA;
							//currentStartEdge = x;
							//i--; //Enable when debugged
							i = currentStartEdge;
							//e2.Debug (Color.yellow);
							pointPath.Add (startPoint);
							doBreak = true;
							break;
						}
					}
				} else {
					
					CanSeeEdge cSE = new CanSeeEdge (e);
					cSE.index = i;
					for (int x=currentStartEdge;x<i;x++) {
						e.Debug (Color.red);
						Edge e2 = clampedEdges[x];
						
						Edge.OnLinePosition p1 = Edge.DoesLinesIntersectFast (e2,e1A);
						Edge.OnLinePosition p2 = Edge.DoesLinesIntersectFast (e2,e1B);
						
						
						if (p1 != Edge.OnLinePosition.OnLine) {
							cSE.canSeeA = false;
							e1A.Debug (Color.cyan);
							e2.Debug (Color.red);
						}
						
						if (p2 != Edge.OnLinePosition.OnLine) {
							cSE.canSeeB = false;
							e1B.Debug (Color.cyan);
							e2.Debug (Color.red);
						}
						
						if (p1 == Edge.OnLinePosition.OnLine || p2 == Edge.OnLinePosition.OnLine || (p1 == Edge.OnLinePosition.Left && p2 == Edge.OnLinePosition.Right) || (p1 == Edge.OnLinePosition.Right && p2 == Edge.OnLinePosition.Left)) {
							e2.Debug (Color.green);
							
							//yield return 0;
							continue;
						} else {
							if (p1 != p2) {
								Debug.LogError ("Similarity expected : Code Error : "+p1+" | "+p2);
							}
							
							e2.Debug (Color.yellow);
							Debug.DrawRay (e2.endPointA,Vector3.up,Color.yellow);
							bool didChange = false;
							for (int y=lastSeenEdges.Count-1;y>=0;y--) {
								CanSeeEdge cSE2 = (lastSeenEdges[y] as CanSeeEdge);
								
								Debug.DrawRay (cSE2.edge.endPointA,Vector3.up,cSE2.canSeeA ? Color.green : Color.red);
								Debug.DrawRay (cSE2.edge.endPointB,Vector3.up,cSE2.canSeeB ? Color.green : Color.red);
								
								Vector2 factor1 = Edge.IntersectionFactorFast (cSE2.edge,e1A);
								Vector2 factor2 = Edge.IntersectionFactorFast (cSE2.edge,e1B);
								
								//Mathfx.NearestPoint (e1A.endPointA,e1A.endPointB,cSE2.edge.endPointA);
								
								//Vector3.Dot (cSE2.nearestPoint1-cSE2.edge.endPointA,cSE2.nearestPoint1-cSE2.edge.endPointB) <= 0 ||
									//Vector3.Dot (cSE2.nearestPoint2-cSE2.edge.endPointA,cSE2.nearestPoint2-cSE2.edge.endPointB) <= 0
									
								//If the lines intersect on different sides of the edge or if they intersect on the edge
								
								Debug.DrawRay (cSE2.edge.endPointA+cSE2.edge.edgeVector3D*factor1.x,Vector3.up*2,Color.cyan);
								Debug.DrawRay (cSE2.edge.endPointA+cSE2.edge.edgeVector3D*factor2.x,Vector3.up*2,Color.cyan);
								Debug.DrawLine (cSE2.edge.endPointA,cSE2.edge.endPointA+cSE2.edge.edgeVector3D*factor2.x+Vector3.up,Color.cyan);
								Debug.DrawLine (cSE2.edge.endPointA,cSE2.edge.endPointA+cSE2.edge.edgeVector3D*factor1.x+Vector3.up,Color.cyan);
								
								if (!(((factor1.x <= 0 || factor1.y < 0)  && (factor2.x <= 0 || factor2.y < 0)) || ((factor1.x >= 1 || factor1.y < 0) && (factor2.x >= 1 || factor2.y < 0)))) {
									Debug.DrawLine (cSE2.edge.endPointA,cSE2.edge.endPointB,Color.red);
									//Debug.LogWarning (cSE2.pos1+"  "+cSE2.pos2);
									continue;
								}
								
								Debug.DrawLine (cSE2.edge.endPointA,cSE2.edge.endPointB,Color.white);
								
								float factor = factor1.y > 0 ? factor1.x : -factor1.x;
								//Debug.DrawRay (cSE2.edge.endPointA,Vector3.up,cSE2.canSeeA ? Color.green : Color.red);
								//Debug.DrawRay (cSE2.edge.endPointB,Vector3.up,cSE2.canSeeB ? Color.green : Color.red);
								
								if (factor <= 0 && cSE2.canSeeA) {
									//Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointA,Color.green);
									//Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointB,Color.red);
									
									startPoint = cSE2.edge.endPointA;
									currentStartEdge = cSE2.index;
									didChange = true;
									break;
								} else if (factor >= 1 && cSE2.canSeeB) {
									//Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointA,Color.red);
									//Debug.DrawLine (cSE2.nearestPoint1,cSE2.edge.endPointB,Color.green);
									startPoint = cSE2.edge.endPointB;
									currentStartEdge = cSE2.index;
									didChange = true;
									break;
								}
							}
							
							if (!didChange) {
								Debug.LogError ("Nothing changed!!! Wut!?!");
							}
							
							lastSeenEdges = new ArrayList ();
							
							//startPoint = p1.pos == Edge.OnLinePosition.Right ? clampedEdges[lastSeenRight].endPointB : clampedEdges[lastSeenLeft].endPointA;
							//currentStartEdge = p1.pos == Edge.OnLinePosition.Right ? lastSeenRight : lastSeenLeft;
							//i--; //Enable when debugged
							i = currentStartEdge;
							//CanSeeEdge cSE3 = new CanSeeEdge (clampedEdges[i]);
							//cSE3.index = i;
							//lastSeenEdges.Add (cSE3);
							
							pointPath.Add (startPoint);
							doBreak = true;
							break;
						}
					}
					
					if (cSE.canSeeAny) {
						lastSeenEdges.Add (cSE);
						
						for (int y=lastSeenEdges.Count-1;y>=0;y--) {
							CanSeeEdge cSE2 = (lastSeenEdges[y] as CanSeeEdge);
							Debug.DrawLine (cSE2.edge.endPointA+Vector3.up*0.1F,cSE2.edge.endPointB+Vector3.up*0.1F,Color.white);
						}
					} else {
						cSE.edge.Debug (Color.black);
						Debug.Log (cSE.canSeeA+"  "+cSE.canSeeB);
					}
				}
				
				//Check the end point (the point the path started on actually, but since we are checking from end to start the names are inverted)
				//if (!doBreak) {
					
				//}
			}
			
			pointPath.Add (realStart);
			
			for (int i=0;i<pointPath.Count-1;i++) {
				Debug.DrawLine ((Vector3)pointPath[i],(Vector3)pointPath[i+1],Color.cyan);
			}
			
			//pointPath[pointPath.Length-1] = realStart;//(returnPath[returnPath.Count-1] as Node).vectorPos;
			
			//for (int i=0;i<pointPath.Length-1;i++) {
				//Debug.DrawLine (pointPath[i],pointPath[i+1],Color.Lerp (Color.blue,Color.red,(float)i/(float)(pointPath.Length-1)));
			//}
			//for (int i=0;i<pP2.Length-1;i++) {
			//	Debug.DrawLine (pP2[i],pP2[i+1],Color.yellow);
			//}
			//End Step 2
			
			ArrayList a2 = new ArrayList (pointPath);
			a2.Reverse ();
			returnPath.Reverse ();
			return a2.ToArray (typeof(Vector3)) as Vector3[];
		}*/
		
		/*public static Vector3 IntersectPoint (Vector3 start1,Vector3 start2,Vector3 dir1,Vector3 dir2) {
			Vector2 start1x = new Vector2 (start1.x,start1.z);
			Vector2 start2x = new Vector2 (start2.x,start2.z);
			Vector2 dir1x = new Vector2 (dir1.x,dir1.z);
			Vector2 dir2x = new Vector2 (dir2.x,dir2.z);
			
			Vector3 result = IntersectPoint (start1x,start2x,dir1x,dir2x);
			
			//float y1 = ((start2.y-start1.y+start1.x-start2.x)/(dir1.y-dir2.y))*dir1.y;
			return new Vector3 (result.x,start1.y+dir1.y*0.5F,result.y);
		}
		
		public static Vector3 IntersectPoint (Vector2 start1,Vector2 start2,Vector2 dir1,Vector2 dir2) {
			//if (dir1.x==dir2.x) {
			//	return Vector2.zero;
			//}
			
			float h1 = dir1.y/dir1.x;
			float h2 = dir2.y/dir2.x;
			
			if (h1==h2) {
				return Vector2.zero;
			}
			
			Vector3 line1 = new Vector2 (h1,start1.y-start1.x*h1);
			Vector3 line2 = new Vector2 (h2,start2.y-start2.x*h2);
			
			float y1 = line2.y-line1.y;
			float x1 = line1.x-line2.x;
			
			float x2 = y1 / x1;
			
			float y2 = line1.x*x2 + line1.y;
			//Debug.Log (y1+" "+x1+" "+x2+" "+y2);
			return new Vector3(x2,y2,y1);
		}*/
		
	}
	
	public class Edge
	{
		public Vector3 endPointA;
	
		public Vector3 endPointB;
	
		public Vector3 edgeVector3D;
		
		public Vector2 edgeVector;
	
		public EdgeIntersection intersection;
		
		public Node node;
		
		public int triIndex1 = -1;
		public int triIndex2 = -1;
		public int offset = 0;
		//Is any edges which also use the vertice endPointA only connected to one triangle/node
		public bool borderA1 = false;
		public bool borderA2 = false;
		public bool borderB1 = false;
		public bool borderB2 = false;
		
		public Vector3 borderA1E;
		public Vector3 borderA2E;
		public Vector3 borderB1E;
		public Vector3 borderB2E;
		
		public bool borderA {
			get {
				return borderA1 || borderA2;
			}
			set {}
		}
		
		public bool borderB {
			get {
				return borderB1 || borderB2;
			}
			set {}
		}
		
		public void SetBorderA (bool border, int tris, Vector3 edge) {
			if (tris == triIndex1) {
				borderA1 = true;
				borderA1E = edge;
			} else if (tris == triIndex2) {
				borderA2 = true;
				borderA2E = edge;
			} else {
				UnityEngine.Debug.LogWarning ("The triangle index does not match : Parameter name 'tris' = "+tris +" This edge's triangle indexes are "+triIndex1 +" and "+triIndex2);
			}
		}
		
		public void SetBorderB (bool border, int tris, Vector3 edge) {
			if (tris == triIndex1) {
				borderB1 = true;
				borderB1E = edge;
			} else if (tris == triIndex2) {
				borderB2 = true;
				borderB2E = edge;
			} else {
				UnityEngine.Debug.LogWarning ("The triangle index does not match : Parameter name 'tris' = "+tris +" This edge's triangle indexes are "+triIndex1 +" and "+triIndex2);
			}
		}
		
		int triA;
		int triB;
	
		private Vector3 _center = Vector3.zero;
		
		public Edge Merge (Edge rhs) {
			//this.borderA = (this.borderA || rhs.borderA);
			//this.borderB = (this.borderB || rhs.borderB);
			return this;
		}
		
		public Vector3 center {
			get {
				if (_center != Vector3.zero) {
					return _center;
				}
				_center = (endPointA+endPointB)/2.0F;
				return _center;
			}
		}
		
		public enum OnLinePosition
		{
			Right = 0,
			Left,
			OnLine,
			Paralell
		}
	
		public enum LineClassification
		{
			Collinear,			// both lines are parallel and overlap each other
			LinesIntersect,	// lines intersect, but their segments do not
			SegmentIntersect,	// both line segments bisect each other
			A_BISECTS_B,		// line segment B is crossed by line A
			B_BISECTS_A,		// line segment A is crossed by line B
			Paralell			// the lines are paralell
		};
	
		public Edge(Vector3 endPointA, Vector3 endPointB)
		{
			this.endPointA = endPointA;
			this.endPointB = endPointB;
			edgeVector3D = endPointB - endPointA;
			this.edgeVector = new Vector2(edgeVector3D.x, edgeVector3D.z);
		}
		
		public Edge(Vector3 endPointA, Vector3 endPointB, Node node)
		{
			this.endPointA = endPointA;
			this.endPointB = endPointB;
			edgeVector3D = endPointB - endPointA;
			this.edgeVector = new Vector2(edgeVector3D.x, edgeVector3D.z);
			this.node = node;
		}
		
		public void Debug (Color c) {
			UnityEngine.Debug.DrawLine (endPointA,endPointB,c);
		}
		
		public void DebugWarning () {
			UnityEngine.Debug.DrawLine (endPointA,endPointB,new Color (1,0.5F,0));//Orange
		}
		
		public void DebugError () {
			UnityEngine.Debug.DrawLine (endPointA,endPointB,new Color (1,0,0));//Orange
		}
		
		public static bool operator == (Edge lhs, Edge rhs) {
      		return (lhs.endPointA == rhs.endPointA || lhs.endPointA == rhs.endPointB) && (lhs.endPointB == rhs.endPointA || lhs.endPointB == rhs.endPointB);
  	  	}
  	  	
  	  	public static bool operator != (Edge lhs, Edge rhs) {
      		return 
      		lhs.endPointA == rhs.endPointA ? (lhs.endPointB != rhs.endPointB) : (lhs.endPointA == rhs.endPointB ? (lhs.endPointB != rhs.endPointA) : true);
  	  	}
  	  	
  	  	public override bool Equals(System.Object obj) {
  	  		if (obj == null) {
  	  			return false;
  	  		}
  	  		
  	  		Edge o = obj as Edge;
  	  		if (o == null) {
  	  			return false;
  	  		}
  	  		
  	  		return (endPointA == o.endPointA || endPointA == o.endPointB) && (endPointB == o.endPointA || endPointB == o.endPointB);
  	  	}
  	  	
  	  	public override int GetHashCode() {
        	return (int)(endPointA.x + endPointB.x + endPointA.y + endPointB.y + endPointA.z + endPointB.z);
		}
		
		public Edge()
		{
		}
		
		public Edge TurningRadius (float rad) {
			Edge e1;
			
			//if (borderA || borderB) {
				float magn = edgeVector3D.magnitude;
				Vector3 norm = edgeVector3D/magn;
				
				//if (borderA && borderB) {
					if (rad*2.0F >= magn) {
						//e1 = new Edge (center,center);
						e1 = new Edge (endPointA+edgeVector3D*0.45F,endPointB-edgeVector3D*0.45F);
					} else {
						e1 = new Edge (endPointA+norm*rad,endPointB-norm*rad);
					}
				//} else if (borderA) {
				//	e1 = new Edge (endPointA+norm*rad,endPointB);
				//} else {
				//	e1 = new Edge (endPointA,endPointB-norm*rad);
				//}
			//} else {
			//	e1 = new Edge (endPointA,endPointB);	
			//}
			return e1;
		}
	
	
	
		/*	SegmentIntersection
		------------------------------------------------------------------------------------------
		
			Determines if two segments intersect, and if so the point of intersection. The current
			member line is considered line AB and the incomming parameter is considered line CD for
			the purpose of the utilized equations.
	
			A = PointA of the member line
			B = PointB of the member line
			C = PointA of the provided line	
			D = PointB of the provided line	
	
		------------------------------------------------------------------------------------------
		*/
	
		public class EdgeIntersection {
			public LineClassification type;
			public OnLinePosition pos;
			
			public Vector3 intersectionA;
			public Vector3 intersectionB;
			
			public bool intersects {
				get {
					return (type == LineClassification.LinesIntersect || type == LineClassification.SegmentIntersect || type == LineClassification.A_BISECTS_B || type == LineClassification.B_BISECTS_A);
				}
			}
			
			public bool segementsIntersects {
				get {
					return type == LineClassification.SegmentIntersect;
				}
			}
			
			public EdgeIntersection (LineClassification t) {
				type = t;
			}
			
			public EdgeIntersection (LineClassification t, Vector3 a, Vector3 b) {
				type = t;
				intersectionA = a;
				intersectionB = b;
			}
			
			public EdgeIntersection (Vector3 a, Vector3 b, OnLinePosition p) {
				intersectionA = a;
				intersectionB = b;
				pos = p;
			}
			
		}
		
		public static EdgeIntersection IntersectionTest(Edge memberEdge, Edge testEdge)
		{
			Vector2 memberEdgePointA = new Vector2(memberEdge.endPointA.x, memberEdge.endPointA.z);
			Vector2 memberEdgePointB = new Vector2(memberEdge.endPointB.x, memberEdge.endPointB.z);
			Vector2 testEdgePointA = new Vector2(testEdge.endPointA.x, testEdge.endPointA.z);
			Vector2 testEdgePointB = new Vector2(testEdge.endPointB.x, testEdge.endPointB.z);
	
			float Ax_Minus_Cx = memberEdgePointA.x - testEdgePointA.x;
			float Ay_Minus_Cy = memberEdgePointA.y - testEdgePointA.y;
	
	
			float Dx_Minus_Cx = testEdgePointB.x - testEdgePointA.x;
			float Dy_Minus_Cy = testEdgePointB.y - testEdgePointA.y;
	
			float Bx_Minus_Ax = memberEdgePointB.x - memberEdgePointA.x;
			float By_Minus_Ay = memberEdgePointB.y - memberEdgePointA.y;
	
			float numerator = (Ay_Minus_Cy * Dx_Minus_Cx) - (Ax_Minus_Cx * Dy_Minus_Cy);
			float denominator = (Bx_Minus_Ax * Dy_Minus_Cy) - (By_Minus_Ay * Dx_Minus_Cx);
	
			// if lines do not intersect, return now
			if (denominator == 0)
			{
				if (numerator == 0)
				{
					return new EdgeIntersection (LineClassification.Collinear);
				}
	
				// Debug.Log("Intersect Reuslt	\t" + LineClassification.Paralell);
				return new EdgeIntersection (LineClassification.Paralell);
			}
	
			float factorAB = numerator / denominator;
			float factorCD = ((Ay_Minus_Cy * Bx_Minus_Ax) - (Ax_Minus_Cx * By_Minus_Ay)) / denominator;
	
			Vector3 intersectA = memberEdge.endPointA+(memberEdge.endPointB-memberEdge.endPointA)*factorAB;
			Vector3 intersectB = testEdge.endPointA+(testEdge.endPointB-testEdge.endPointA)*factorCD;
			
			//Debug.DrawRay (memberEdge.endPointA+(memberEdge.endPointB-memberEdge.endPointA)*factorAB,Vector3.up*3,Color.red);
			//Debug.DrawRay (testEdge.endPointA+(testEdge.endPointB-testEdge.endPointA)*factorCD,Vector3.up*2,Color.blue);
			//now determine the type of intersection
			if ((factorAB >= 0f) && (factorAB <= 1f) && (factorCD >= 0f) && (factorCD <= 1f))
			{
				//Debug.Log("Intersect Reuslt	\t" + LineClassification.SegmentIntersect);
				return new EdgeIntersection (LineClassification.SegmentIntersect,intersectA,intersectB);
	
			}
			else if ((factorCD >= 0f) && (factorCD <= 1f))
			{
				//Debug.Log("Intersect Reuslt	\t" + LineClassification.A_BISECTS_B);
				return new EdgeIntersection (LineClassification.A_BISECTS_B,intersectA,intersectB);
			}
			else if ((factorAB >= 0f) && (factorAB <= 1.0f))
			{
				//Debug.Log("Intersect Reuslt	\t" + LineClassification.B_BISECTS_A);
				return new EdgeIntersection (LineClassification.B_BISECTS_A,intersectA,intersectB);
			}
	
			//Debug.Log("Intersect Reuslt	\t" + LineClassification.LinesIntersect);
			return new EdgeIntersection (LineClassification.LinesIntersect,intersectA,intersectB);
	
		}
		
		public static OnLinePosition DoesLinesIntersectFast (Edge memberEdge, Edge testEdge) {
			Vector2 memberEdgePointA = new Vector2(memberEdge.endPointA.x, memberEdge.endPointA.z);
			Vector2 memberEdgePointB = new Vector2(memberEdge.endPointB.x, memberEdge.endPointB.z);
			Vector2 testEdgePointA = new Vector2(testEdge.endPointA.x, testEdge.endPointA.z);
			Vector2 testEdgePointB = new Vector2(testEdge.endPointB.x, testEdge.endPointB.z);
	
			float Ax_Minus_Cx = memberEdgePointA.x - testEdgePointA.x;
			float Ay_Minus_Cy = memberEdgePointA.y - testEdgePointA.y;
	
	
			float Dx_Minus_Cx = testEdgePointB.x - testEdgePointA.x;
			float Dy_Minus_Cy = testEdgePointB.y - testEdgePointA.y;
	
			float Bx_Minus_Ax = memberEdgePointB.x - memberEdgePointA.x;
			float By_Minus_Ay = memberEdgePointB.y - memberEdgePointA.y;
	
			float numerator = (Ay_Minus_Cy * Dx_Minus_Cx) - (Ax_Minus_Cx * Dy_Minus_Cy);
			float denominator = (Bx_Minus_Ax * Dy_Minus_Cy) - (By_Minus_Ay * Dx_Minus_Cx);
	
			// if lines do not intersect, return now
			if (denominator == 0)
			{
				return OnLinePosition.Paralell;
			}
	
			float factorAB = numerator / denominator;
			
			//If the factor is less than 0 or greater than 1, then the nearest point is not on the line
			if (factorAB < 0) {
				return OnLinePosition.Left;
			} else if (factorAB > 1) {
				return OnLinePosition.Right;
			}
			
			return OnLinePosition.OnLine;
		}
		
		public static EdgeIntersection DoesLinesIntersect (Edge memberEdge, Edge testEdge) {
			Vector2 memberEdgePointA = new Vector2(memberEdge.endPointA.x, memberEdge.endPointA.z);
			Vector2 memberEdgePointB = new Vector2(memberEdge.endPointB.x, memberEdge.endPointB.z);
			Vector2 testEdgePointA = new Vector2(testEdge.endPointA.x, testEdge.endPointA.z);
			Vector2 testEdgePointB = new Vector2(testEdge.endPointB.x, testEdge.endPointB.z);
	
			float Ax_Minus_Cx = memberEdgePointA.x - testEdgePointA.x;
			float Ay_Minus_Cy = memberEdgePointA.y - testEdgePointA.y;
	
	
			float Dx_Minus_Cx = testEdgePointB.x - testEdgePointA.x;
			float Dy_Minus_Cy = testEdgePointB.y - testEdgePointA.y;
	
			float Bx_Minus_Ax = memberEdgePointB.x - memberEdgePointA.x;
			float By_Minus_Ay = memberEdgePointB.y - memberEdgePointA.y;
	
			float numerator = (Ay_Minus_Cy * Dx_Minus_Cx) - (Ax_Minus_Cx * Dy_Minus_Cy);
			float denominator = (Bx_Minus_Ax * Dy_Minus_Cy) - (By_Minus_Ay * Dx_Minus_Cx);
	
			// if lines do not intersect, return now
			if (denominator == 0)
			{
				return new EdgeIntersection (Vector3.zero,Vector3.zero,OnLinePosition.Paralell);
			}
	
			float factorAB = numerator / denominator;
			float factorCD = ((Ay_Minus_Cy * Bx_Minus_Ax) - (Ax_Minus_Cx * By_Minus_Ay)) / denominator;
	
			Vector3 intersectA = memberEdge.endPointA+(memberEdge.endPointB-memberEdge.endPointA)*factorAB;
			Vector3 intersectB = testEdge.endPointA+(testEdge.endPointB-testEdge.endPointA)*factorCD;
			
			//If the factor is less than 0 or greater than 1, then the nearest point is not on the line
			if (factorAB < 0) {
				return new EdgeIntersection (intersectA,intersectB,OnLinePosition.Left);
			} else if (factorAB > 1) {
				return new EdgeIntersection (intersectA,intersectB,OnLinePosition.Right);
			}
			
			return new EdgeIntersection (intersectA,intersectB,OnLinePosition.OnLine);
		}
		
		public static Vector2 IntersectionFactorFast (Edge memberEdge, Edge testEdge) {
			Vector2 memberEdgePointA = new Vector2(memberEdge.endPointA.x, memberEdge.endPointA.z);
			Vector2 memberEdgePointB = new Vector2(memberEdge.endPointB.x, memberEdge.endPointB.z);
			Vector2 testEdgePointA = new Vector2(testEdge.endPointA.x, testEdge.endPointA.z);
			Vector2 testEdgePointB = new Vector2(testEdge.endPointB.x, testEdge.endPointB.z);
	
			float Ax_Minus_Cx = memberEdgePointA.x - testEdgePointA.x;
			float Ay_Minus_Cy = memberEdgePointA.y - testEdgePointA.y;
	
	
			float Dx_Minus_Cx = testEdgePointB.x - testEdgePointA.x;
			float Dy_Minus_Cy = testEdgePointB.y - testEdgePointA.y;
	
			float Bx_Minus_Ax = memberEdgePointB.x - memberEdgePointA.x;
			float By_Minus_Ay = memberEdgePointB.y - memberEdgePointA.y;
	
			float numerator = (Ay_Minus_Cy * Dx_Minus_Cx) - (Ax_Minus_Cx * Dy_Minus_Cy);
			float denominator = (Bx_Minus_Ax * Dy_Minus_Cy) - (By_Minus_Ay * Dx_Minus_Cx);
	
			// if lines do not intersect, return now
			if (denominator == 0)
			{
				return new Vector2 (0,0);
			}
	
			float factorAB = numerator / denominator;
			float factorCD = ((Ay_Minus_Cy * Bx_Minus_Ax) - (Ax_Minus_Cx * By_Minus_Ay)) / denominator;
			
			return new Vector2 (factorAB,factorCD);
		}
		
		 /*public static PositionToEdgeResult LineTest(Edge edge, Vector3 testPoint)
		 {
			 Vector3 direction = edge.endPointB - edge.endPointA;
	
			 Vector2 dir2D = new Vector2(direction.x, direction.z);
	
			 Vector2 normal = GetNormal(dir2D);
	
			 Vector3 testDir = (testPoint - edge.endPointA).normalized;
			 Vector2 testDir2D = new Vector3(testDir.x, testDir.z);
	
			 float dotValue = Vector2.Dot(testDir2D, normal);
	
			 if (dotValue > 0) { return PositionToEdgeResult.onRightSide; }
			 else if (dotValue < 0) { return PositionToEdgeResult.onLeftSide; }
	
			 return PositionToEdgeResult.onLine;
		 }*/
	
	
	
		 public static Vector2 GetNormal(Vector2 dir2D)
		 {
			 Vector2 normal = Vector2.zero;
			 normal[0] = dir2D[1];
			 normal[1] = -dir2D[0];
			 normal = ((Vector3)normal).normalized;
			 return normal;
		 }
		
		public float NearestPointOnEdges (Edge e2) {
			return NearestPointsOnEdges (this,e2).x;
		}
		
		public static Vector2 NearestPointsOnEdges (Edge e1, Edge e2) {
		    Vector3   u = e1.endPointB - e1.endPointA;
		    Vector3   v = e2.endPointB - e2.endPointA;
		    Vector3   w = e1.endPointA - e2.endPointA;
		    float    a = Vector3.Dot(u,u);        // always >= 0
		    float    b = Vector3.Dot(u,v);
		    float    c = Vector3.Dot(v,v);        // always >= 0
		    float    d = Vector3.Dot(u,w);
		    float    e = Vector3.Dot(v,w);
		    float    D = a*c - b*b;       // always >= 0
		    float    sc, sN, sD = D;      // sc = sN / sD, default sD = D >= 0
		    float    tc, tN, tD = D;      // tc = tN / tD, default tD = D >= 0
		
		    // compute the line parameters of the two closest points
		    if (D < 0.05F) { // the lines are almost parallel
		        sN = 0.0F;        // force using point P0 on segment S1
		        sD = 1.0F;        // to prevent possible division by 0.0 later
		        tN = e;
		        tD = c;
		    }
		    else {                // get the closest points on the infinite lines
		        sN = (b*e - c*d);
		        tN = (a*e - b*d);
		        if (sN < 0.0F) {       // sc < 0 => the s=0 edge is visible
		            sN = 0.0F;
		            tN = e;
		            tD = c;
		        }
		        else if (sN > sD) {  // sc > 1 => the s=1 edge is visible
		            sN = sD;
		            tN = e + b;
		            tD = c;
		        }
		    }
		
		    if (tN < 0.0F) {           // tc < 0 => the t=0 edge is visible
		        tN = 0.0F;
		        // recompute sc for this edge
		        if (-d < 0.0F)
		            sN = 0.0F;
		        else if (-d > a)
		            sN = sD;
		        else {
		            sN = -d;
		            sD = a;
		        }
		    }
		    else if (tN > tD) {      // tc > 1 => the t=1 edge is visible
		        tN = tD;
		        // recompute sc for this edge
		        if ((-d + b) < 0.0F)
		            sN = 0;
		        else if ((-d + b) > a)
		            sN = sD;
		        else {
		            sN = (-d + b);
		            sD = a;
		        }
		    }
		    // finally do the division to get sc and tc
		    sc = (Mathf.Abs(sN) < 0.05F ? 0.0F : sN / sD);
		    tc = (Mathf.Abs(tN) < 0.05F ? 0.0F : tN / tD);
		
		    // get the difference of the two closest points
		    //Vector3   dP = w + (sc * u) - (tc * v);  // = S1(sc) - S2(tc)
		
		    return new Vector2 (sc,tc);   // return the closest distance
		}
		
	}
	
	public class CanSeeEdge {
		public Edge edge;
		public bool canSeeA = true;
		public bool canSeeB = true;
		
		public Vector3 nearestPoint1;
		public Vector3 nearestPoint2;
		
		public Edge.OnLinePosition pos1;
		public Edge.OnLinePosition pos2;
		
		public int index = 0;
		public float distA = 9999999;
		public float distB = 9999999;
		
		public bool canSeeAny {
			get {
				return canSeeA || canSeeB;
			}
		}
		
		public CanSeeEdge () {
		}
		
		public CanSeeEdge (Edge e) {
			edge = e;
			canSeeA = true;
			canSeeB = true;
		}
	}
}
