using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowTile : Tile
{
    Rigidbody2D body;
    int blowerPower = 300;
    int heaterPower = 300;
    float anchorStrength = 3;

    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInAnchorPosition())
        {
            if ((Vector3.Distance(transform.position, AnchorPosition) > 1))
            {//has broken away
                WorldManager.singleton.destroySnowTile(attachedTileIndex);
            }
            else
            {//still anchored
                body.AddForce((AnchorPosition - transform.position).normalized * anchorStrength);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("snow tile triggered by: " + collision.gameObject.name);
        if ((collision.gameObject.name.Contains("Bullet") || collision.gameObject.name.Contains("HeatWave")) && collision.gameObject.GetComponent<Bullet>().type.Equals(Bullet.Type.AirBullet))
        {
            Vector2 force = Vector2.zero;
            //Debug.Log("adding force of mag: " + force.magnitude);

            if (collision.gameObject.name.Contains("Bullet"))
            {
                force = collision.gameObject.GetComponent<Rigidbody2D>().velocity * blowerPower / Mathf.Pow(collision.gameObject.transform.localScale.magnitude, 3);
                Destroy(collision.gameObject, 0.5f);
            }
            if (collision.gameObject.name.Contains("HeatWave"))
            {
                force = (transform.position - collision.gameObject.transform.position).normalized * heaterPower * blowerPower / Mathf.Pow(collision.gameObject.transform.localScale.magnitude, 2);
                //Destroy(collision.gameObject, 1f);
            }
            body.AddForce(force);

        }
        else if (collision.gameObject.tag.Equals("Player"))
        {
            //collision.gameObject.GetComponent<Rigidbody2D>().velocity *= -1;
            //collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            //float bounce = 2f;
            //collision.gameObject.GetComponent<Rigidbody2D>().MovePosition(new Vector2(collision.gameObject.transform.position.x, collision.gameObject.transform.position.y) - collision.gameObject.GetComponent<Rigidbody2D>().velocity * Time.deltaTime * bounce);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("snow tile collided by: " + collision.gameObject.name);
        if (collision.gameObject.name.Contains("Bullet"))
        {
            body.AddForce(collision.gameObject.GetComponent<Rigidbody2D>().velocity);
            Destroy(collision.gameObject);
        }
    }

    public bool isInAnchorPosition()
    {
        return Vector3.Distance(transform.position, AnchorPosition) < 0.1f;
    }
}
