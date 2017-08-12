using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomBehavior : ClickBehavior {
    public int roomHP;
    public Dictionary<GameObject,GameObject> POIs = new Dictionary<GameObject, GameObject>();//<POI,Occupant>
	public List<GameObject> doors = new List<GameObject>();
    public int roomUnitSlotsPerTeam;
	// Use this for initialization
	void Awake () {
        //populate the POI list for each room on startup
	    foreach(Transform child in transform) {
            if (child.CompareTag("RoomPOI")) {
                POIs.Add(child.gameObject,null);
            }
        }
        roomUnitSlotsPerTeam = POIs.Count;

		for (int i = 0; i < this.gameObject.transform.childCount; i++) {
			if (this.gameObject.transform.GetChild (i).CompareTag ("RoomDoors")) {
				this.doors.Add(this.gameObject.transform.GetChild(i).gameObject);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void Selected() {
        //if a room is clicked, depopulate the selection Q (unselect selected officers)
        MouseScript.ClearSelectionQ();
    }

    public override void AltSelected() {
        int npcAccumulator = 0; //counts how many npcs we've gathered for this cycle
        for(int i = 0; i<MouseScript.selectionQ.Count; i++) {
            if (MouseScript.selectionQ[i].GetComponent<NPCBehavior>() != null) {
                MouseScript.selectionQ[i].GetComponent<NPCBehavior>().RegisterWithRoom(this);
                npcAccumulator++;
                if (npcAccumulator >= POIs.Count) {
                    break;
                }
            }
        }
    }

    public bool HasSpace() {
        foreach(GameObject g in POIs.Values) {
            if (g == null) {
                return true;
            }
        }
        return false;
    }

    public void Unregister(GameObject g) {
        //remove an occupant from a POI if it exists
        if (POIs.ContainsValue(g)) {
            List<KeyValuePair<GameObject,GameObject>> kvps = new List<KeyValuePair<GameObject, GameObject>>(POIs);
            foreach(KeyValuePair<GameObject, GameObject> kvp in kvps) {
                if (kvp.Value == g) {
                    POIs[kvp.Key] = null;
                }
            }
        }
    }

    public void Register(GameObject g) {
        //find the first tile with null occupant and put this gameobject into that slot
        List<KeyValuePair<GameObject, GameObject>> kvps = new List<KeyValuePair<GameObject, GameObject>>(POIs);
        foreach (KeyValuePair<GameObject, GameObject> kvp in kvps) {
            if (kvp.Value == null) {
                POIs[kvp.Key] = g;
                NPCBehavior npc = POIs[kvp.Key].GetComponent<NPCBehavior>();
                if (npc != null) {
                    npc.SetNavTarget(kvp.Key.transform);
                }
                break;
            }
        }
    }
}
