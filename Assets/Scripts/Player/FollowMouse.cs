using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour {

    public GameObject Player;

	// Use this for initialization
	void Start ()
    {
        // Set Cursor to not be visible
        Cursor.visible = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
		transform.position = Camera.main.ScreenToWorldPoint((Input.mousePosition)+ new Vector3(0,0,10));

    }
}
