using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseScript : MonoBehaviour {
    public static List<GameObject> selectionQ = new List<GameObject>();
    public static Transform navTarget;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButtonDown(0)) {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (hit) {
                GameObject selection = hitInfo.transform.gameObject;
                Debug.Log("Clicked " + hitInfo.transform.gameObject.name);
                if (selection.GetComponent<ClickBehavior>()!=null) {
                    Debug.Log("Selection has a Click Behavior");
                    selection.GetComponent<ClickBehavior>().Selected();
                }
                else {
                    selectionQ.Clear();
                }
            }
        }
        else if (Input.GetMouseButtonDown(1)) {//apply actions, clear the list
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit){
                //if we right clicked a room, pass each selected item to the room and let the room handle nav targets
                //Debug.Log("AltSelection on point");
                //Debug.Log("Selection list is of size: "+selectionQ.Count);
                //navTarget = hitInfo.transform;
                //foreach (GameObject g in selectionQ) {
                //if (g.GetComponent<ClickBehavior>() != null) {
                //    g.GetComponent<ClickBehavior>().AltSelected();
                //}
                GameObject rClickedObject = hitInfo.transform.gameObject;
                if (rClickedObject.GetComponent<ClickBehavior>() != null) {
                    rClickedObject.GetComponent<ClickBehavior>().AltSelected();
                }
            }
        }
	}

    public static void AddToSelectionQ(GameObject g) {
        //if nothing is in this q, add something
        if (selectionQ.Count == 0) {
            selectionQ.Add(g);
        }
        //if the left shift was being held, continue adding stuff
        else if(Input.GetKey(KeyCode.LeftShift)){
            selectionQ.Add(g);
        }
        //we left clickd something while having already left clicked something
        else {
            ClearSelectionQ();
            selectionQ.Add(g);
        }
    }

    public static void ClearSelectionQ() {
        selectionQ.Clear();
    }
}
