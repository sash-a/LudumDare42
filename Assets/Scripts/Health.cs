using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Vector3 offset;

    public float health;
    public float maxHealth;

    public GameObject healthBarPrefab;

    private GameObject healthBar;

    public void damage(float amount)
    {
        bool destroy = true;
        health -= amount;
        if (health <= 0)
        {
            Heater heater = GetComponent<Heater>();
            if (heater != null)
            {
                WorldManager.singleton.removeHeaterAt(heater.attachedTileIndex);
            }

            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                player.downPlayer();
                destroy = false;
            }

            if (destroy)
            {
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        healthBar = Instantiate(healthBarPrefab);
        healthBar.transform.parent = WorldManager.singleton.healthBarsParent;

        healthBar.GetComponent<PlayerFollower>().setUp(offset, transform);

        Heater heater = GetComponent<Heater>();
        if (heater != null)
        {
            healthBar.transform.localScale += new Vector3(0.4f, 0.05f, 1);
            healthBar.GetComponent<SpriteRenderer>().color = new Color(0.8396226f, 0.1782218f, 0.1782218f, 1);
        }
        else if(GetComponent<PlayerController>() != null)
        {
            healthBar.transform.localScale += new Vector3(0.2f, 0.05f, 1);
            healthBar.GetComponent<SpriteRenderer>().color = GetComponent<PlayerController>().playerColour;
        }
    }

    void Update()
    {
        healthBar.transform.localScale = new Vector3(health / maxHealth * 0.2f, 0.035f, 1);
    }

    internal void heal(float amount)
    {
        health = Math.Min(maxHealth, health + amount);
    }

    internal bool isMax()
    {
        if (health >= maxHealth)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDestroy()
    {
        Destroy(healthBar);
    }
}