using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathFinder : MonoBehaviour {
    public  GameObject startPoint;
    public  GameObject endPoint;
    public  List<GameObject> visitedSet = new List<GameObject>();//contains a list of visited pathways (used for pathfinding only)
    public  List<KeyValuePair<GameObject, GameObject>> allEdges = new List<KeyValuePair<GameObject, GameObject>>();
	public  List<GameObject> doors = new List<GameObject> ();
    public  float lineWidth = 1f;
    public  GameObject firstPoint; // for debug, can be removed
    public  List<List<int>> input = new List<List<int>>();//used for djikstra dfs spath

	public void InitData(){
		allEdges.Clear ();
		//foreach room, populate the allEdges dict
		List<GameObject> Rooms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Rooms"));
		//get each room
		foreach(GameObject room in Rooms) {
			List<GameObject> nodes = new List<GameObject>();
			//get each roomPOI within that room
			foreach(Transform item in room.transform) {
				if (item.tag.Equals("RoomPOI")) {
					nodes.Add(item.gameObject);
				}
			}
			//Debug.Log("Number of waypoints:" + nodes.Count);
			//build edgelist based on all these thingies
			for (int i =0; i<nodes.Count; i++) {
				for(int j = 0; j < nodes.Count; j++) {//connect every node to every node above it in the node list
					if (nodes[i] != nodes[j]) {
						allEdges.Add(new KeyValuePair<GameObject, GameObject>(nodes[i], nodes[j]));
					}
				}
			}
		}

		//Debug.Log("Built edglist of size:" + allEdges.Count);
		ApplyDoorPaths ();
		//add each connection set to the djikstra input
		for (int x = 0; x<allEdges.Count; x++) {
			if (x == 0) {
				input.Add(new List<int>{ allEdges[0].Key.GetHashCode() ,  allEdges[0].Value.GetHashCode()});
			}
			bool foundThisKey = false;
			for (int y = 0; y<input.Count; y++) {
				if (input[y][0]==allEdges[x].Key.GetHashCode()) {
					foundThisKey = true;
					if (!input[y].Contains(allEdges[x].Value.GetHashCode())) {
						input[y].Add(allEdges[x].Value.GetHashCode());
					}
				}
			}
			if (foundThisKey == false) {
				input.Add(new List<int> { allEdges[x].Key.GetHashCode(), allEdges[x].Value.GetHashCode() });
			}
		}
		//input should be populated.
		//Debug.Log("input has "+input.Count);
		foreach (List<int> il in input) {
			string s = "List: ";
			foreach (int i in il) {
				s += " ," + i;
			}
			//Debug.Log(s);
		}

		//more debug
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("RoomPOI")) {
			GameObject debugMesh = new GameObject(go.GetHashCode().ToString());
			debugMesh.AddComponent<TextMesh>();
			TextMesh tm = debugMesh.GetComponent<TextMesh>();
			tm.text = go.GetHashCode().ToString();
			//tm.fontSize = 24;
			debugMesh.transform.position = go.transform.position+Vector3.up;
			debugMesh.transform.LookAt(debugMesh.transform.position-Vector3.up);
			debugMesh.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		}

		//more debug
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("RoomDoors")) {
			GameObject debugMesh = new GameObject(go.GetHashCode().ToString());
			debugMesh.AddComponent<TextMesh>();
			TextMesh tm = debugMesh.GetComponent<TextMesh>();
			tm.text = go.GetHashCode().ToString();
			//tm.fontSize = 24;
			debugMesh.transform.position = go.transform.position+Vector3.up;
			debugMesh.transform.LookAt(debugMesh.transform.position-Vector3.up);
			debugMesh.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		}
	}

	void ApplyDoorPaths(){
		//Great! now build connections between doors!
		//door paths are just the two closest nodes to each door
		GameObject[] pois = GameObject.FindGameObjectsWithTag ("RoomPOI");
		Vector3 shipOffset = new Vector3 (1000.01f,1.1f, 1.01f);
		pois = pois.OrderBy (poi => Vector3.Distance (shipOffset, poi.transform.position)).ToArray();
		List<GameObject> POIs = new List<GameObject>(pois);

		KeyValuePair<float, GameObject> first = new KeyValuePair<float,GameObject>(float.MaxValue, null);
		KeyValuePair<float, GameObject> second = new KeyValuePair<float, GameObject>(float.MaxValue, null);
		//Debug.Log("Checking "+POIs.Count + " nodes against " + doors.Count + " doors");
		for (int i = 0; i < doors.Count; i++) {
			//get first
			for (int j = 0; j < POIs.Count; j++) {
				if (Vector3.Distance(POIs[j].transform.position, doors[i].transform.position) <= first.Key) {
					first = new KeyValuePair<float, GameObject>(Vector3.Distance(POIs[j].transform.position, doors[i].transform.position), POIs[j]);
				}
			}
			//get second
			for (int j = 0; j < POIs.Count; j++) {
				if (POIs[j] != first.Value) {
					if (Vector3.Distance(POIs[j].transform.position, doors[i].transform.position) <= second.Key) {
						second = new KeyValuePair<float, GameObject>(Vector3.Distance(POIs[j].transform.position, doors[i].transform.position), POIs[j]);
					}
				}
			}
			//found the two closest nodes, add them to the edgelist
			if (first.Value != null && second.Value != null) {
				allEdges.Add(new KeyValuePair<GameObject, GameObject>(first.Value, second.Value));
				allEdges.Add(new KeyValuePair<GameObject, GameObject>(second.Value, first.Value));
				first = new KeyValuePair<float, GameObject>(float.MaxValue, null);
				second = new KeyValuePair<float, GameObject>(float.MaxValue, null);
			}
		}
		//Debug.Log("Added doorpaths to edglist, size is now:" + allEdges.Count);

	}

	// Use this for initialization
	void Start () {
		InitData ();
    }

    // Update is called once per frame
    void Update() {
            if (startPoint != null & endPoint != null) {
                //for debug
                Debug.DrawLine(startPoint.transform.position + (Vector3.up), endPoint.transform.position + Vector3.up*0.5f, Color.red);
                
                if (firstPoint != null) {
                    Debug.DrawLine(startPoint.transform.position + (Vector3.up), firstPoint.transform.position + Vector3.up*0.5f, Color.yellow);
                }

                if (allEdges.Count > 1) {
                    foreach(KeyValuePair<GameObject,GameObject> kvp in allEdges) {
                        Debug.DrawLine(kvp.Key.transform.position + (Vector3.up), kvp.Value.transform.position + Vector3.up*0.5f, Color.magenta);
                    }
                }
                //end for debug
            }
	}

	//cleanupphaseflag prevents recursion into cleanup function
	public List<GameObject> FindPath(GameObject so, GameObject eo, bool disableRecursiveSelfCheck = false) {
		List<GameObject> intermediatePoints = new List<GameObject>();

        startPoint = so;
        endPoint = eo;
        List<GameObject> allNodes = new List<GameObject>(GameObject.FindGameObjectsWithTag("RoomPOI"));
        /*  given a start point and an end point
            get a list of all nodes
            DFS to endpoint.
            when path found, record how many steps it took to get there
            find every path - only retain the path with the shortest amount of steps
            do NOT visit a node that has already been visited
            If no paths are found, it means a door blocked the way, don't bother trying to move
        */
        //get the first point
        GameObject nearest = null;
        float dist = float.MaxValue;
        foreach (GameObject g in allNodes) {
            float thisDistance = Vector3.Distance(so.transform.position, g.transform.position);
            if (thisDistance < dist) {
                dist = thisDistance;
                nearest = g;
                //Debug.Log("Shortest distance is: "+dist);
            }
        }
        firstPoint = nearest;//for debugging, can be removed
        intermediatePoints = FindShortestPath(allNodes, firstPoint, endPoint);

        //do some cleanup if necessary
		if ((intermediatePoints.Count>0) && (intermediatePoints.ElementAt(intermediatePoints.Count - 1) == eo)) {
			CleanUpPath(ref intermediatePoints);
			ApplyBackwardShortcut(ref intermediatePoints, startPoint);
			if (!disableRecursiveSelfCheck) {
				DiscoverShorterCuts(ref intermediatePoints);
			}
        }
        else {
            intermediatePoints.Clear();
            //Debug.Log("No route to destination found");
        }

		return(intermediatePoints);
    }

    private List<GameObject> FindShortestPath(List<GameObject> map, GameObject source, GameObject destination) {
        List<GameObject> path = new List<GameObject>();
        visitedSet.Clear();
        path.Add(source);
        GameObject current = source;
        while (true) {
            //get all neighbors of current (nodes within range)
            List<GameObject> allNeighbors = new List<GameObject>();
            foreach(List<int> neighborSet in input) {
                if (neighborSet[0].GetHashCode() == current.GetHashCode()) {
                    foreach (KeyValuePair<GameObject, GameObject> kvp in allEdges) {
                        if ((kvp.Key == current)&&(!allNeighbors.Contains(kvp.Value))) {
                            allNeighbors.Add(kvp.Value);
                        }
                        else if((kvp.Value == current)&&(!allNeighbors.Contains(kvp.Key))) {
                            //allNeighbors.Add(kvp.Key);
                        }
                    }
                }
            }

            //whew... now we have a list of this GameObject's neighbors
            //remove neighbors that are already added to the path
            List<GameObject> neighbors = new List<GameObject>();
            foreach(GameObject neighbor in allNeighbors) {
                if ((!path.Contains(neighbor))&&(!neighbors.Contains(neighbor))){
                    neighbors.Add(neighbor);
                }
            }
			/*
            Debug.Log("The neighbors for: " + current.GetHashCode());
            string temp = "";
            foreach(GameObject go in neighbors) {
                temp += " " + go.GetHashCode();
            }
            Debug.Log(temp); 
            */
            

            //stop if no neighbors or destination reached
            if (neighbors.Count() == 0) {
                path.Remove(current);
                visitedSet.Add(current);
				if (path.Count > 1) {
					current = path.ElementAt (path.Count () - 1);
				} else {
					path = new List<GameObject>();
					return path;
				}
            }
            if (neighbors.Contains(destination)) {
                path.Add(destination);
				return path;
            }

            //chose next node as the neighbor with the shortest distence to the destination
            GameObject nearest = FindNearest(neighbors, destination);
			if (nearest == null) {
				path = new List<GameObject>();
				return path;
			} else {
				path.Add(nearest);
				current = nearest;
				visitedSet.Add(current);
			}
        }
        return path;
    }

    private GameObject FindNearest(List<GameObject> nodes, GameObject target) {
        float shortestDistance = -1;
        GameObject nearest = null;

        int index = 0;
        foreach(GameObject go in nodes) {
            float distance = FindDistance(go, target);
            if(index == 0) {
                shortestDistance = distance;
                nearest = go;
            }
            else if (shortestDistance > distance) {
                shortestDistance = distance;
                nearest = go;
            }
            index += 1;
        }
		//Debug.Log ("The Nearest neighbor to the destination is: "+nearest.GetHashCode());
        return nearest;
    }

    private float FindDistance(GameObject alpha, GameObject beta) {
		float result = Vector3.Distance (alpha.transform.position, beta.transform.position);
        if (visitedSet.Contains(beta)) {
            result += result;//add weight to this edge if we've already visted it
        }
        if(result == 0) {
            result = float.MaxValue;
        }
        return result;
    }

    private void CleanUpPath(ref List<GameObject> thepath){
        //for any node, if you can get to the node AFTER the next node without touching the next node, do that instead.
        List<GameObject> removeThese = new List<GameObject>();
        for(int i =0; i<thepath.Count-2; i++) {
            foreach(KeyValuePair<GameObject,GameObject> edge in allEdges) {
                if ((edge.Key==thepath[i]) && (edge.Value==thepath[i+2])) {
                    removeThese.Add(thepath[i+1]);
                }
            }
        }
			
       // Debug.Log("The path can be reduced by " + removeThese.Count());
        foreach(GameObject go in removeThese) {
            thepath.Remove(go);
        }

		//now, make sure that the route from start to each point is optimal, if it is not, restructure the path.
		//FindPath();
    }

	private void ApplyBackwardShortcut(ref List<GameObject> originalPath, GameObject theUnit){
		//if it would be faster to go to the last visited node, rather than the closest node, do that.
		if(originalPath.Count<2 || theUnit.GetComponent<NPCBehavior>()==null){
			return;
		}

		//check if point 2 is the same as lastpoint, diagonal backtracking
		if (theUnit.GetComponent<NPCBehavior> ().lastLocation == originalPath [1]) {
			originalPath.RemoveAt(0);
		}
	
		//check if point 2 is the same as nextpoint, deeper room nav
		if(theUnit.GetComponent<NPCBehavior>().nextLocation == originalPath [1]){
			originalPath.RemoveAt(0);
		}
		//Debug.Log ("originalDist: " + originalDistance + " from: "+originalPath[0].transform.position);
		//Debug.Log ("altDist: " +alternativeDistance + " from: "+alternativePath[0].transform.position);
	}

	//if we lop one off the end of the List it should result in the same path
	//if not, we found a shortcut
	private void DiscoverShorterCuts(ref List<GameObject> originalPath){
		if (originalPath.Count () < 3) {
			return;
		}

		GameObject startPoint = originalPath.ElementAt (0);
		for(int i = originalPath.Count()-2; i>0 ; i--){
			GameObject endPoint = originalPath.ElementAt(i);
			List<GameObject> toppedPath = FindPath (startPoint, endPoint, true);
			bool isSubset = !toppedPath.Except(originalPath).Any();
			if (!isSubset) {
				//original path should be modified to take the route defined by the shorter path
				originalPath.RemoveRange(0, i);
				originalPath.InsertRange(0, toppedPath);
			}
		}

	}
}
