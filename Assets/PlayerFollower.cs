using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;

    void Update()
    {
        transform.position = target.position + Vector3.up + offset;
    }

    public void setUp(Vector3 offset, Transform target)
    {
        this.offset = offset;
        this.target = target;
    }
}