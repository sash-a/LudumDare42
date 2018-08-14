using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    float MinCameraSize = 5.5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        moveToCenter();
        if (WorldManager.singleton.players.Count > 1) {
            rescaleCamera();
        }
	}

    private void rescaleCamera()
    {
        double maxDistance = 0;
        foreach (GameObject player in WorldManager.singleton.players)
        {
            double dist = biasedDistance(new Vector2(transform.position.x, transform.position.y), new Vector2(player.transform.position.x, player.transform.position.y));
            if (dist > maxDistance) {
                maxDistance = dist;
            }
        }
        float scale = (float)Math.Max(MinCameraSize, 1.2*Math.Pow(maxDistance,0.83));
        Camera.main.orthographicSize = scale;
        //Debug.Log("setting camera scale to : " + scale + " max dist: " + maxDistance);
    }

    private void moveToCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (GameObject player in WorldManager.singleton.players) {
            center += player.transform.position;
        }
        center /= WorldManager.singleton.players.Count;
        center.z = 10;
        transform.position = center;
    }

    double biasedDistance(Vector2 a, Vector2 b) {
        float xDiff = (a.x - b.x) * 0.7f;
        float yDiff = (a.y - b.y) * 1.6f;
        return Math.Pow(Math.Pow(xDiff,2)+ Math.Pow(yDiff,2), 0.5);
    }
}
