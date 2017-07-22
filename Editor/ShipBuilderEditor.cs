using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(ShipBuilderScript))]
public class DoorBuilderEditor : Editor {
	public string args = "";
	public GameObject ship;
	private string gameDataProjectFilePath = "/StreamingAssets/data.dat";

	public void Awake(){
		Debug.Log ("Ship bound");
		ship = GameObject.FindGameObjectWithTag ("AShip");
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

		//F_M_L
		//a group of open doors, start, end map to a List of nodes to visit
		Dictionary<byte[],byte[]> lookupTable = new Dictionary<byte[], byte[]>();



		byte inc = 1;
		for (int i = 0; i < 254; i++) {
			byte[] mapState = { inc, 0, 0 };
			byte[] path = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
			lookupTable.Add (mapState, path);
			inc++;
		}

		inc = 1;
		for (int i = 0; i < 255; i++) {
			byte[] mapState = { 0, inc, 0 };
			byte[] path = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
			lookupTable.Add (mapState, path);
			inc++;
		}

		inc = 1;
		for (int i = 0; i < 255; i++) {
			byte[] mapState = { 0, 0, inc };
			byte[] path = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
			lookupTable.Add (mapState, path);
			inc++;
		}

			
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
		{
			foreach (byte[] key in lookupTable.Keys)
			{
				binaryWriter.Write(key[0]); //doors
				binaryWriter.Write(key[1]); //start
				binaryWriter.Write(key[2]); //end
				binaryWriter.Write(lookupTable[key]); //path
			}
		}

		lookupTable.Clear();

		using (BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
		{
			while (true)
			{
				byte[] key = binaryReader.ReadBytes (3);
				byte[] value = binaryReader.ReadBytes (3);
				lookupTable.Add(key,value);

				if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
					break;
			}
		}

		Debug.Log(lookupTable.Keys.Count);
	}
}
