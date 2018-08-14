using UnityEngine;

public class FallTile : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "Player")
        {
            Debug.Log("Player died");
            Destroy(other.gameObject);

            // TODO falling sound + animations
        }
    }
}