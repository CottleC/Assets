using UnityEngine;
using System.Collections;

public class ObjectSpawnerScript : MonoBehaviour {
    public Vector3 spawnPoint;
    public GameObject currentObject;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    public GameObject BuildRoom(string args) {
        if (args.Equals("2x2")) {
            currentObject = (GameObject)Instantiate(Resources.Load<GameObject>("2x2Room"), spawnPoint, Quaternion.Euler(90, 0, 0));
            currentObject.name = "2x2";
        }
        else if (args.Equals("2x1")) {
            currentObject = (GameObject)Instantiate(Resources.Load<GameObject>("2x1Room"), spawnPoint, Quaternion.Euler(90, 270, 0));
            currentObject.name = "2x1";
        }
        else if (args.Equals("1x2")) {
            currentObject = (GameObject)Instantiate(Resources.Load<GameObject>("1x2Room"), spawnPoint, Quaternion.Euler(90, 0, 0));
            currentObject.name = "1x2";
        }
        else if (args.Equals("1x1")) {
            currentObject = (GameObject)Instantiate(Resources.Load<GameObject>("1x1Room"), spawnPoint, Quaternion.Euler(90, 90, 0));
            currentObject.name = "1x1";
        }
        else if (args.Equals("MyOfficer")) {
            currentObject = (GameObject)Instantiate(Resources.Load<GameObject>("MyOfficer"), spawnPoint, Quaternion.Euler(90, 0, 0));
            currentObject.name = "MyOfficer";
        }
        else {
            Debug.Log("Invalid args passed to ObjectSpawnerScript");
        }

        if (currentObject != null) {
            //currentObject.transform.parent = GameObject.FindGameObjectWithTag("ShipAnchor").transform;
            return currentObject;
        }
        else {
            return null;
        }
    }
}
