using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinLifeTime : MonoBehaviour
{
    public int lifeTime;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}