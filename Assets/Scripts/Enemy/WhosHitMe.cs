using System.Collections.Generic;
using UnityEngine;

public class WhosHitMe : MonoBehaviour
{
    public Dictionary<int, PlayerController> playersHit;
    public GameObject coin;
    public GameObject powerUpPrefab;
    public ParticleSystem hitEffect;

    private void Start()
    {
        playersHit = new Dictionary<int, PlayerController>();
    }

    public void onHit(Bullet bullet)
    {
        var shooter = bullet.gameObject.GetComponent<Bullet>().shooter;

        if (!playersHit.ContainsKey(shooter.playerNumber))
            playersHit.Add(shooter.playerNumber, shooter);
        hitEffect.Play();
    }

    private void OnDestroy()
    {
        // Increment player kills
        foreach (var player in playersHit.Values)
        {
            player.kills++;
        }

        float ran = UnityEngine.Random.Range(0f, 1f);
        //Debug.Log("ran: " + ran);
        if (ran> 0.16f)
        {
            Instantiate(coin, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }
    }
}