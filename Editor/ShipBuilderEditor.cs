using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

[CustomEditor(typeof(ShipBuilderScript))]
public class DoorBuilderEditor : Editor {
	public string args = "";
	public GameObject ship;
	public PathFinder pathFinder;
	//a group of open doors, start, end map to a List of nodes to visit
	public Dictionary<byte[],byte[]> lookupTable = new Dictionary<byte[], byte[]>(new ArrayEqualityComparer());
	private string gameDataProjectFilePath = "/StreamingAssets/data.bytes";

	public void Awake(){
		Debug.Log ("Binding tools to ship in scene ");
		ship = GameObject.FindGameObjectWithTag ("AShip");
		pathFinder = GameObject.FindGameObjectWithTag ("Tools").GetComponent<PathFinder> ();
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		ShipBuilderScript DoorScript = (ShipBuilderScript)target;
		if (GUILayout.Button("Founder Doors")) {
			FounderDoors();
		}

		if (GUILayout.Button("Generate Pathing Table")) {
			Debug.Log("Generating Pathing Table...");
			InitPathingTable ();
		}

		if (GUILayout.Button(" ")) {

		}

		if (GUILayout.Button("Remove All Rooms")) {
			List<GameObject> roomList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Rooms"));
			for (int i =0; i<roomList.Count; i++) {
				GameObject.DestroyImmediate(roomList[i]);
			}
		}
	}

	private void FounderDoors() {
		List<GameObject> doors = new List<GameObject>(GameObject.FindGameObjectsWithTag("RoomDoors")); //working gameobjects
		List<GameObject> rooms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Rooms")); //working gameobjects
		List<int> validDoors = new List<int>();//list of doors that DID touch another room (allows retention of doors that don't touch multiple rooms
		Debug.Log("Generating doors from " + doors.Count + " doorzones.");

		//check each door. If it collides with a room that is NOT its own parent, then remove this door
		for (int i = 0; i < doors.Count; i++) {
			Bounds doorBounds = doors[i].GetComponent<Renderer>().bounds;
			GameObject parentRoom = doors[i].transform.parent.gameObject;
			for (int j = 0; j < rooms.Count; j++) {
				Bounds roomBounds = rooms[j].GetComponent<Renderer>().bounds;
				if (parentRoom != rooms[j].gameObject) {//only perform this comparison if its not against the door's own parent
					if (doorBounds.Intersects(roomBounds)) {
						validDoors.Add(i);
					}
					else {

					}
				}
			}
		}

		for (int i = 0; i < doors.Count; i++) {//perform initial removal
			if (!validDoors.Contains(i)) {
				GameObject.DestroyImmediate(doors[i]);
			}
		}
		doors = new List<GameObject>(GameObject.FindGameObjectsWithTag("RoomDoors")); //reset list
		Dictionary<GameObject, GameObject> doorPairs = new Dictionary<GameObject, GameObject>(); // kvp hashcodes of doors that are on top of eachother
		for (int i=0; i<doors.Count; i++) {
			for (int j = 0; j < doors.Count; j++) {
				if(i!=j) {//if the distance between two doors is less than a tolerance, just delete one of the doors because it means we have multiple doors stacked
					float dist = Vector3.Distance(doors[i].transform.position, doors[j].transform.position);
					if (dist<0.1) {
						if((!doorPairs.ContainsKey(doors[i]) && (!doorPairs.ContainsValue(doors[i])))) {
							doorPairs.Add(doors[i], doors[j]);
						}
					}
				}
			}
		}

		foreach(KeyValuePair<GameObject, GameObject> go in doorPairs) {
			GameObject.DestroyImmediate(go.Value);
		}
	}


	public void InitPathingTable(){
		string filePath = Application.dataPath + gameDataProjectFilePath;
		(new FileInfo (filePath)).Directory.Create ();

		GameObject[] doors = GameObject.FindGameObjectsWithTag ("RoomDoors");
		GameObject[] pois = GameObject.FindGameObjectsWithTag ("RoomPOI");
		Vector3 shipOffset = new Vector3 (1000.01f,1.1f, 1.01f); // need a janky position off the ship to get unique distances to pois
		doors = doors.OrderBy(door => Vector3.Distance(shipOffset, door.transform.position)).ToArray();
		pois = pois.OrderBy(poi => Vector3.Distance(shipOffset, poi.transform.position)).ToArray();
		bool[] doorState = new bool[doors.Count()];

		double count = Math.Pow(2, doorState.Length);
		for (int i = 0; i < count; i++) {
			string str = Convert.ToString (i, 2).PadLeft (doorState.Length, '0');
			doorState = str.Select ((x) => x == '1').ToArray ();
			GenerateLookupTableSlice (doors, doorState, pois);
		}

		Debug.Log ("Table Generated");
		Debug.Log ("Entries:"+lookupTable.Keys.Count());
			
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
		{
			foreach (byte[] key in lookupTable.Keys)
			{
				for (byte b = 0; b < key.Count (); b++) {
					binaryWriter.Write(key[b]);//numdoors,doors,start,end | route is numsteps,steps
				}
				binaryWriter.Write(lookupTable[key]);//path
			}
		}

		lookupTable.Clear();

		using (BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
		{
			while (true)
			{
				//	public Dictionary<byte[],byte[]> lookupTable = new Dictionary<byte[], byte[]>();
				//  numdoors,doors,start,end | route is numsteps,steps
				byte numDoors = binaryReader.ReadByte();
				byte[] doorIds = binaryReader.ReadBytes (numDoors);
				byte startId = binaryReader.ReadByte();
				byte endId =  binaryReader.ReadByte();
				//this makes the key, now read the value
				byte numNodes = binaryReader.ReadByte();
				byte[] routeIds = binaryReader.ReadBytes (numNodes);
				//need to account for 0 door situations and 0 route situation
				int keySize=numDoors+3;
				int valueSize = numNodes + 1;
				byte[] key = new byte[numDoors+3];
				byte[] value = binaryReader.ReadBytes (numNodes+1);

				if (keySize > 3) {//there were some open doors
					byte i = 1;
					key [0] = numDoors;
					for (i=i; i < keySize - 2; i++) {
						key [i] = doorIds [i - 1];
					}
					key [i] = startId;
					key [i + 1] = endId;
				} else {//there were no open doors
					key[0]=numDoors;
					key [1] = startId;
					key [2] = endId;
				}

				if (valueSize > 1) {//there were some nodes
					byte i = 1;
					value [0] = numNodes;
					for (i=i; i < valueSize; i++) {
						value [i] = routeIds [i - 1];
					}
				} else {//there were no nodes
					value[0]=numNodes;
				}

				lookupTable.Add(key,value);

				if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
					break;
			}
		}

		Debug.Log("Rebuilt table has: "+ lookupTable.Keys.Count);
	}

	public void GenerateLookupTableSlice (GameObject[] doors, bool[] doorState, GameObject[] pois){
		//given the state of the doors, build pathing between each point in the ship
		//don't need to calculate a route between b->a if a-> has already been calculated
		Dictionary<GameObject,GameObject> generatedPaths = new Dictionary<GameObject,GameObject>();
		List<GameObject> openDoorsList = new List<GameObject>();
		for (byte b = 0; b < doorState.Count (); b++) {
			if (doorState [b]) {
				openDoorsList.Add (doors.ElementAt (b));
			}
		}
		pathFinder.doors = openDoorsList;
		pathFinder.InitData();

		for (int i =0; i < pois.Count(); i++) {
			GameObject start = pois.ElementAt (i);
				for(int j =0; j < pois.Count(); j++){
					GameObject end = pois.ElementAt (j);
					if(start!=end){//don't path to self
					//if(!generatedPaths.Contains(KeyValuePair<GameObject,GameObject>(end,start))){//if this route doesn't exist backwards...
						List<GameObject> theRoute = new List<GameObject>();
						theRoute = pathFinder.FindPath (start, end);
						if ((theRoute != null) && (theRoute.Count () > 0)) {
							byte[] openDoors = ReportOpenDoors (doors, doorState);
							byte[] route = ReportRoute (pois, theRoute.ToArray());
							byte[] startFin = new byte[]{(byte)i,(byte)j};
							byte[] lookupKey = new byte[openDoors.Length+startFin.Length];
							openDoors.CopyTo (lookupKey, 0);
							startFin.CopyTo (lookupKey, openDoors.Length);
						{//don't add start->end if end->start already exists
							byte[] falseKey = new byte[openDoors.Length+startFin.Length];
							byte[] finStart = new byte[]{(byte)j,(byte)i};
							openDoors.CopyTo (falseKey, 0);
							finStart.CopyTo (falseKey, openDoors.Length);
							if(lookupTable.ContainsKey(falseKey)){
								//Debug.Log ("omg a false key!");
								continue;
							}
						}
							lookupTable.Add (lookupKey, route);
							//DebugRoute (route, start, end, openDoors);
							//Debug.Log ("Added new Route");
						} else {
							//Debug.Log ("Route was null");
							//byte[] openDoors = ReportOpenDoors (doors, doorState);
							//byte[] route = new byte[1];
							//route [0] = 0;
							//lookupTable.Add (openDoors, route);
						}
					//}
					//else{
						//TODO: Implement backwards thingy
					//}
				}//end start==end check
			}//end nested for
		}//end for
		//Debug.Log("Done with Slice");
}

	public byte[] ReportOpenDoors(GameObject[] doors, bool[] doorState){
		//TODO:ADD START AND ENDPOINT TO TAIL
		List<byte> openDoors = new List<byte>();

		for (byte b = 0; b < doorState.Count (); b++) {
			if (doorState [b]) {
				openDoors.Add (b);
			}
		}
		openDoors.Insert(0, (byte)openDoors.Count());

		return openDoors.ToArray();

	}

	//returns a list of indexes of the route if the pois list is in sorted order
	public byte[] ReportRoute (GameObject[] pois, GameObject[] goRoute){
		List<GameObject> poiList = new List<GameObject>(pois);
		List<byte> route = new List<byte>();
		route.Add ((byte)goRoute.Count());
		for (byte b = 0; b < goRoute.Count (); b++) {
			route.Add((byte)(poiList.IndexOf(goRoute.ElementAt(b))));
		}

		return route.ToArray();
	}

	public void DebugRoute(byte[] theRoute, GameObject start, GameObject end, byte[] openDoors){
		Debug.Log ("A Route: ");
		string doorsText="";
		string routeText = "";
		byte box = 0;

		for (int i = 0; i < openDoors.Count (); i++) {
			if (i == 0) {
				Debug.Log ("# open doors: " + openDoors [i] + ".");
			} else {
				doorsText += openDoors[i]+", ";
			}
		}

		for (int i = 0; i < theRoute.Count (); i++) {
			if (i == 0) {
				Debug.Log ("This route has: " + theRoute [i] + " nodes.");
			} else {
				routeText += theRoute[i]+", ";
			}
		}

		Debug.Log ("openDoorIDs:\t"+doorsText);
		Debug.Log ("Start:"+start.GetHashCode()+" : Finish:"+end.GetHashCode());
		Debug.Log ("theRouteIDs->:\t"+routeText);
		Debug.Log ("-------------------------");
	}

	public class ArrayEqualityComparer : EqualityComparer<byte[]>
	{
		// You could make this a per-instance field with a constructor parameter
		private static readonly EqualityComparer<byte> elementComparer
		= EqualityComparer<byte>.Default;
		public override bool Equals(byte[] first, byte[] second)
		{
			if (first == second)
			{
				return true;
			}
			if (first == null || second == null)
			{
				return false;
			}
			if (first.Length != second.Length)
			{
				return false;
			}
			for (int i = 0; i < first.Length; i++)
			{
				if (!(first[i]==second[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode(byte[] array)
		{
			unchecked
			{
				if (array == null)
				{
					return 0;
				}
				int hash = 17;
				foreach (byte element in array)
				{
					hash = hash * 31 + elementComparer.GetHashCode(element);
				}
				return hash;
			}
		}
	}
}
