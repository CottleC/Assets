using UnityEngine;
using System.Collections;

public class DoorBehavior : ClickBehavior {
    public bool isOpen = true;
    public bool locked = false;
    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    public override void Selected() {
        ToggleStatus();
    }

    public override void AltSelected() {
        ToggleStatus();
    }

    public void ToggleStatus() {
        if (!locked) {
            GameObject blocker = gameObject.transform.GetChild(0).gameObject;
            if (isOpen) {
                blocker.transform.position = new Vector3(blocker.transform.position.x, blocker.transform.position.y + 1f, blocker.transform.position.z);
            }
            else {
                blocker.transform.position = new Vector3(blocker.transform.position.x, blocker.transform.position.y - 1f, blocker.transform.position.z);
            }
            isOpen = !isOpen;
        }

        foreach(GameObject g in GameObject.FindGameObjectsWithTag("MyNPCs")) {

        }
    }
}
