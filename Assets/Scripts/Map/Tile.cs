using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public Vector2Int attachedTileIndex;
    public Vector3 AnchorPosition;

    public SpriteRenderer renderer;
    public Dictionary<Vector2Int, Tile> neighbours;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    internal void resetPosition()
    {
        transform.position = AnchorPosition;
    }
}