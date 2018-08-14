using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public int cash;
    public AudioSource coinSound;
    PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    public void useCurrency(int amout)
    {
        cash -= amout;
        cash = Mathf.Max(cash, 0);
    }

    public void gainCurrency(int amount)
    {
        cash += amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name.Contains("Coin"))
        {
            gainCurrency(2);
            coinSound.Play();
            Destroy(other.gameObject);
        }
        if (other.name.Contains("PowerUp"))
        {
            PowerUp power = other.gameObject.GetComponent<PowerUp>();
            
            if (power.type.Equals(PowerUp.Type.HEALTH))
            {
                player.GetComponent<Health>().health = player.GetComponent<Health>().maxHealth;
            }
            if (player.powerTime > 0)
            {
                return;
            }
            if (power.type.Equals(PowerUp.Type.FIRE_RATE))
            {
                player.firePeriod = 0.07f;
                player.powerTime = 10;
            }

            if (power.type.Equals(PowerUp.Type.SPEED))
            {
                player.moveSpeed = 8;
                player.powerTime = 10;
            }
            if (power.type.Equals(PowerUp.Type.SHOTGUN))
            {
                player.usingShotgun = true;
                player.firePeriod = 0.15f;
                player.powerTime = 10;
            }
            coinSound.Play();
            Destroy(other.gameObject);
        }
    }
}