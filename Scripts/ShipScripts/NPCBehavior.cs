using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NPCBehavior : ClickBehavior {
    public Transform navigationTarget;
	public GameObject lastLocation;
	public GameObject nextLocation;
    public RoomBehavior currentRoom;
	public float unitSpeed = 3.0f;
	public List<GameObject> intermediatePoints = new List<GameObject>();
	public List<GameObject> selectionQ = MouseScript.selectionQ;
    // Use this for initialization
    void Start() {
		//find the nearest open roomslot and move there.
		MoveToNearestOpenSlot();
    }

    // Update is called once per frame
    void Update() {
		DebugActions();
		PerformNPCAction();
    }

    public override void Selected() {
        MouseScript.AddToSelectionQ(gameObject);
        //MouseScript.selectionQ.Add(gameObject);
    }

    public override void AltSelected() {
        if (MouseScript.navTarget != null) {
            //navigationTarget = MouseScript.navTarget;
        }
    }

    public void SetNavTarget(Transform t) {
        navigationTarget = t;
    }

    public void RegisterWithRoom(RoomBehavior rb) {

		if (rb == this.currentRoom) {
			return;
		};

        //check if new room has space
        if (rb.HasSpace()) {
            //deregister with current room
            if (currentRoom != null) {
                currentRoom.Unregister(gameObject);
            }
            //register with new room
            rb.Register(gameObject);
            this.currentRoom = rb;
			if (intermediatePoints == null) {
				Debug.Log ("No route to location");
			}
        }
        else {
            Debug.Log("Target Room has no space");
        }

    }

	public void PerformNPCAction(){
		if (navigationTarget != null) {
			MoveToTarget();
		}
	}

	public void MoveToNearestOpenSlot(){
		List<GameObject> rooms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Rooms"));

		rooms.Sort(delegate(GameObject c1, GameObject c2){
			return Vector3.Distance(this.transform.position, c1.transform.position).CompareTo
				((Vector3.Distance(this.transform.position, c2.transform.position)));   
		});

		foreach(GameObject go in rooms){
			bool success = go.GetComponentInParent<RoomBehavior>().HasSpace();
			if (success) {
				Debug.Log ("Found nearest location " +go);
				if (currentRoom != null) {
					currentRoom.Unregister(gameObject);
				}
				//register with new room
				go.GetComponentInParent<RoomBehavior>().Register(gameObject);
				currentRoom = go.GetComponentInParent<RoomBehavior>();
				this.intermediatePoints.Add(this.navigationTarget.gameObject);
				break;
			}
		}
	}

	public void MoveToTarget(){
		float step = this.unitSpeed * Time.deltaTime;

		//if there is a next waypoint, go to it and remove it from the nav list if we are close to it.
		if (this.intermediatePoints.Count > 0) {
			if (Vector3.Distance (this.transform.position, this.intermediatePoints.ElementAt (0).transform.position) < 0.01f) {
				this.lastLocation = this.intermediatePoints.ElementAt (0);
				this.intermediatePoints.RemoveAt (0);
			}
		} else {
			this.nextLocation = null;
		}

		if (this.navigationTarget != null) {
			if (this.intermediatePoints.Count > 0) {
				transform.position = Vector3.MoveTowards (transform.position, intermediatePoints.First().transform.position, step);
				this.nextLocation = this.intermediatePoints.First();
			}
		}
	}

	public void DebugActions(){
		if (this.intermediatePoints.Count > 1) {
			for (int i = 0; i<this.intermediatePoints.Count-1; i++) {
				Debug.DrawLine(this.intermediatePoints[i].transform.position + (Vector3.up), this.intermediatePoints[i+1].transform.position + Vector3.up, Color.green);
			}
		}
	}
}
