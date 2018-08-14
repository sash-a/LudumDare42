using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    public GameObject heaterPrefab;

    private void Start()
    {
        for (int i = 0; i < WorldManager.noPlayers; i++)
        {
            var player = Instantiate(playerPrefabs[i], new Vector2(-38 + i, -17 + i), Quaternion.identity);

            var controller = player.GetComponent<PlayerController>();
            controller.playerNumber = i;
            controller.playerColour = GameInfo.playerColours[i];

            WorldManager.singleton.players.Add(player);
        }

        var heaterPos = new Vector2Int(25, 25);
        var heater = WorldManager.singleton.placeObject(heaterPos, heaterPrefab).GetComponent<Heater>();
        WorldManager.singleton.heaters.Add(heaterPos, heater);
        heater.upgrade();
        heater.upgrade();
    }
}