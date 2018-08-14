using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heater : Building
{
    public enum Type
    {
        Temporary,
        LV1,
        LV2,
        LV3,
        Death
    };

    public Type type = Type.LV1;
    public GameObject heatWavePrefab;

    static float firePeriod = 0.3f; //how many seconds between fires
    float timeSinceFire = 0;

    Health health;

    public bool isActive = true;

    public static GameObject heaterPrefab;

    // Effects
    public ParticleSystem heaterPlaceEffect;
    public ParticleSystem heavyDamagedEffect;
    public ParticleSystem lightDmagedEffect;
    public ParticleSystem destroyedEffectPrefab;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
        setSprite();
        heaterPrefab = Resources.Load<GameObject>("Prefabs/Map/Heater");
    }

    private void setSprite()
    {
        if (type.Equals(Type.Temporary))
        {
            renderer.sprite = Resources.Load<Sprite>("Sprites/Heater_Temporary");
            health.maxHealth = 150;
            health.health = 150;
        }
        else if (!type.Equals(Type.Death))
        {
            renderer.sprite = Resources.Load<Sprite>("Sprites/Heater_" + type.ToString());
        }
        else
        {
            renderer.sprite = Resources.Load<Sprite>("Sprites/Death_Heater");
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceFire += Time.deltaTime;
        if (timeSinceFire >= firePeriod)
        {
            timeSinceFire = 0;
            Blow(Vector2.right);
            if (type.Equals(Type.Temporary))
            {
                health.damage(43 * Time.deltaTime);
            }
        }

        if (health.health > 2 * health.maxHealth / 3 && lightDmagedEffect.isPlaying)
        {
            lightDmagedEffect.Stop();
        }
        else if (health.health < 2 * health.maxHealth / 3 && health.health > health.maxHealth / 3 &&
                 !lightDmagedEffect.isPlaying)
        {
            lightDmagedEffect.Play();
            heavyDamagedEffect.Stop();
        }
        else if (health.health < health.maxHealth / 3 && !heavyDamagedEffect.isPlaying)
        {
            lightDmagedEffect.Stop();
            heavyDamagedEffect.Play();
        }
    }

    void Blow(Vector2 direction)
    {
        if (!isActive)
        {
            return;
        }

        Rigidbody2D rigidbody = Instantiate(heatWavePrefab, transform.position, Quaternion.identity)
            .GetComponent<Rigidbody2D>();
        //rigidbody.velocity = direction.normalized * heatSpeed * 0.6f;
        Destroy(rigidbody.gameObject,
            2.5f + (type.Equals(Type.LV3) ? 2f : (type.Equals(Type.LV2) ? 1f : 0)) -
            (type.Equals(Type.Death) ? 1.4f : 0));
    }

    internal void upgrade()
    {
        if (renderer == null || health == null)
        {
            renderer = GetComponent<SpriteRenderer>();
            health = GetComponent<Health>();
        }

        type++;
        setSprite();
        health.maxHealth += 150;
        health.health = health.maxHealth;
    }

    public static Heater getDeathHeater(Vector2Int cellPos)
    {
        Vector3 pos = WorldManager.singleton.tilemap.CellToWorld(new Vector3Int(
            WorldManager.singleton.tilemap.origin.x + cellPos.x, WorldManager.singleton.tilemap.origin.y + cellPos.y,
            1));
        GameObject heater = Instantiate(heaterPrefab, pos, Quaternion.Euler(0, 0, 0));
        return heater.GetComponent<Heater>();
    }

    private void OnDestroy()
    {
        var de = Instantiate(destroyedEffectPrefab, transform.position, Quaternion.identity);
        de.Play();
    }
}