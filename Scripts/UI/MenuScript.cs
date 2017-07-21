using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {
	
	public Canvas canvas;
    public CallbackDummy cbd;
	
	// Use this for initialization
	void Start () {
        //link with the only callbackdummy in the scene
        cbd = GameObject.FindGameObjectWithTag("CallbackDummy").GetComponent<CallbackDummy>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToggleHideAllMenus(){
        canvas.enabled = !canvas.isActiveAndEnabled;
    }
}
