using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public PlayerController shooter;
    public ParticleSystem hitEffect;

    public enum Type
    {
        Bullet,
        AirBullet
    };

    public Type type;

    public float bulletDamage;
    Rigidbody2D body;

    // Use this for initialization
    void Start()
    {
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        float angle = (Mathf.Atan2(velocity.y, velocity.x)) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (type.Equals(Type.AirBullet))
        {
            transform.localScale += Vector3.one * 0.1f;
            body.velocity -= body.velocity.normalized * 0.02f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Player")) return;
        if (type.Equals(Type.AirBullet))
        {
            Destroy(gameObject, 0.5f);
        }
        else
        {
            if (collision.gameObject.name.Contains("Enemy"))
            {
                collision.gameObject.GetComponent<WhosHitMe>().onHit(this);
                collision.gameObject.GetComponent<Health>().damage(bulletDamage);
                StartCoroutine(playEffect());
            }

            Destroy(gameObject);
        }
    }

    IEnumerator playEffect()
    {
        yield return null;
        hitEffect.Play();
    }
}